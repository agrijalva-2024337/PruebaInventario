using QRCoder;

namespace SLCDM.Application.Features.Activos;

internal static class ActivoQr
{
    public static string Url(string? publicAppUrl, int idActivo)
    {
        var origin = string.IsNullOrWhiteSpace(publicAppUrl)
            ? "http://localhost:5173"
            : publicAppUrl.Trim();

        if (Uri.TryCreate(origin, UriKind.Absolute, out var uri))
        {
            origin = uri.GetLeftPart(UriPartial.Authority);
        }

        return $"{origin.TrimEnd('/')}/consulta/{idActivo}";
    }

    public static byte[] Png(string payload)
    {
        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(payload, QRCodeGenerator.ECCLevel.M);
        var png = new PngByteQRCode(data);
        return png.GetGraphic(8);
    }
}
