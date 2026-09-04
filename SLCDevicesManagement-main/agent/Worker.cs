using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.RegularExpressions;

namespace SLCDM.Agent;

public sealed class Worker : BackgroundService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<Worker> _logger;
    private static readonly TimeSpan Intervalo = TimeSpan.FromMinutes(15);

    public Worker(IHttpClientFactory httpFactory, IConfiguration configuration, ILogger<Worker> logger)
    {
        _httpFactory = httpFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var token = CredencialAlmacen.LeerToken();

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (token is null)
                {
                    token = await AutoRegistrarseAsync(stoppingToken);
                    CredencialAlmacen.GuardarToken(token);
                    _logger.LogInformation("Dispositivo auto-registrado.");
                }

                var bssid = ObtenerBssidConectado();
                var coords = UbicacionEquipo.Leer();
                if (bssid is null && coords is null)
                {
                    _logger.LogWarning("No se pudo leer BSSID ni coordenadas del equipo.");
                }
                else
                {
                    await EnviarPingAsync(token, bssid, coords, stoppingToken);
                }
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                token = CredencialAlmacen.LeerToken();
                _logger.LogError(ex, "El ciclo de rastreo fallo. Se reintenta en {Intervalo}.", Intervalo);
            }

            await Task.Delay(Intervalo, stoppingToken);
        }
    }

    private async Task<string> AutoRegistrarseAsync(CancellationToken cancellationToken)
    {
        var numeroSerie = HuellaHardware.LeerNumeroSerieBios()
            ?? throw new InvalidOperationException("No se pudo leer el numero de serie del equipo.");

        var installKey = _configuration["Backend:InstallKey"]
            ?? throw new InvalidOperationException("Falta Backend:InstallKey en la configuracion.");

        var http = _httpFactory.CreateClient("Backend");
        var response = await http.PostAsJsonAsync("api/dispositivos/auto-registro", new
        {
            NumeroSerie = numeroSerie,
            InstallKey = installKey
        }, cancellationToken);

        response.EnsureSuccessStatusCode();

        var dto = await response.Content.ReadFromJsonAsync<RespuestaAutoRegistro>(cancellationToken: cancellationToken);
        return dto!.TokenCrudo;
    }

    private static string? ObtenerBssidConectado()
    {
        var psi = new ProcessStartInfo("netsh", "wlan show interfaces")
        {
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var proceso = Process.Start(psi);
        if (proceso is null)
        {
            return null;
        }

        var salida = proceso.StandardOutput.ReadToEnd();
        proceso.WaitForExit();

        var match = Regex.Match(salida, @"BSSID\s*:\s*([0-9a-fA-F:]{17})");
        return match.Success ? match.Groups[1].Value.ToLowerInvariant() : null;
    }

    private async Task EnviarPingAsync(
        string token,
        string? bssid,
        (decimal Latitud, decimal Longitud)? coords,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/dispositivos/ping");
        request.Headers.Add("X-Device-Token", token);
        request.Content = JsonContent.Create(new
        {
            Bssid = bssid,
            Latitud = coords?.Latitud,
            Longitud = coords?.Longitud
        });

        try
        {
            var http = _httpFactory.CreateClient("Backend");
            using var response = await http.SendAsync(request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Ping fallido: {Status}", response.StatusCode);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Sin conexion al backend. Se reintenta en el siguiente ciclo.");
        }
    }

    private sealed record RespuestaAutoRegistro(int Id, int IdActivo, string TokenCrudo, DateTime CreadoEn, DateTime? ExpiraEn);
}
