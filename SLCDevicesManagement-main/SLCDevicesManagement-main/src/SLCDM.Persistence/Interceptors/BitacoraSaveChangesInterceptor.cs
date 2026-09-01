using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Domain.Common;
using SLCDM.Domain.Entities;
using SLCDM.Domain.Enums;

namespace SLCDM.Persistence.Interceptors;

/// <summary>
/// BE-19: registra Create/Update/Delete en <see cref="Bitacora"/> de forma
/// automatica. Omite la propia bitacora para no recursar y no audita si no
/// hay usuario autenticado (migraciones, health, login).
/// </summary>
public sealed class BitacoraSaveChangesInterceptor : SaveChangesInterceptor
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly HashSet<string> PropiedadesSensibles = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(Usuario.PasswordHash),
        nameof(Asignacion.FirmaEntrega),
        nameof(Asignacion.FirmaRecibe)
    };

    private readonly ICurrentUserService _currentUser;
    private bool _writingAudit;
    private List<PendingAudit>? _pending;

    public BitacoraSaveChangesInterceptor(ICurrentUserService currentUser)
    {
        _currentUser = currentUser;
    }

    public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
    {
        Capture(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Capture(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    public override int SavedChanges(SaveChangesCompletedEventData eventData, int result)
    {
        var saved = base.SavedChanges(eventData, result);
        Flush(eventData.Context);
        return saved;
    }

    public override async ValueTask<int> SavedChangesAsync(
        SaveChangesCompletedEventData eventData,
        int result,
        CancellationToken cancellationToken = default)
    {
        var saved = await base.SavedChangesAsync(eventData, result, cancellationToken);
        await FlushAsync(eventData.Context, cancellationToken);
        return saved;
    }

    private void Capture(DbContext? context)
    {
        if (_writingAudit || context is null || !_currentUser.UserId.HasValue)
        {
            _pending = null;
            return;
        }

        var pending = new List<PendingAudit>();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.Entity is Bitacora)
            {
                continue;
            }

            if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
            {
                continue;
            }

            if (entry.State == EntityState.Modified && !entry.Properties.Any(p => p.IsModified))
            {
                continue;
            }

            pending.Add(new PendingAudit
            {
                Entity = entry.Entity,
                State = entry.State,
                Tipo = entry.State switch
                {
                    EntityState.Added => TipoOperacionBitacora.Creacion,
                    EntityState.Deleted => TipoOperacionBitacora.Eliminacion,
                    _ => TipoOperacionBitacora.Modificacion
                },
                Entidad = entry.Metadata.ClrType.Name,
                InformacionAnterior = entry.State == EntityState.Added ? null : ToJson(entry, original: true),
                InformacionNueva = entry.State == EntityState.Deleted ? null : ToJson(entry, original: false)
            });
        }

        _pending = pending.Count == 0 ? null : pending;
    }

    private void Flush(DbContext? context)
    {
        if (!TryPrepareFlush(context, out var rows))
        {
            return;
        }

        _writingAudit = true;
        try
        {
            context!.Set<Bitacora>().AddRange(rows);
            context.SaveChanges();
        }
        finally
        {
            _writingAudit = false;
        }
    }

    private async Task FlushAsync(DbContext? context, CancellationToken cancellationToken)
    {
        if (!TryPrepareFlush(context, out var rows))
        {
            return;
        }

        _writingAudit = true;
        try
        {
            context!.Set<Bitacora>().AddRange(rows);
            await context.SaveChangesAsync(cancellationToken);
        }
        finally
        {
            _writingAudit = false;
        }
    }

    private bool TryPrepareFlush(DbContext? context, out List<Bitacora> rows)
    {
        rows = [];
        if (_writingAudit || context is null || _pending is null || _pending.Count == 0 || !_currentUser.UserId.HasValue)
        {
            _pending = null;
            return false;
        }

        var now = DateTime.UtcNow;
        var userId = _currentUser.UserId.Value;

        foreach (var item in _pending)
        {
            if (item.State == EntityState.Added)
            {
                var entry = context.Entry(item.Entity);
                if (entry.State != EntityState.Detached)
                {
                    item.InformacionNueva = ToJson(entry, original: false);
                }
            }

            var idTexto = TryGetEntityId(item.Entity);
            var descripcion = $"{item.Tipo} {item.Entidad}{(idTexto is null ? string.Empty : $" #{idTexto}")}";
            if (descripcion.Length > 300)
            {
                descripcion = descripcion[..300];
            }

            rows.Add(new Bitacora
            {
                IdUsuario = userId,
                FechaHora = now,
                TipoOperacion = item.Tipo,
                EntidadAfectada = item.Entidad,
                Descripcion = descripcion,
                InformacionAnterior = item.InformacionAnterior,
                InformacionNueva = item.InformacionNueva
            });
        }

        _pending = null;
        return rows.Count > 0;
    }

    private static string ToJson(EntityEntry entry, bool original)
    {
        var values = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var property in entry.Properties)
        {
            if (ShouldSkip(property))
            {
                continue;
            }

            values[property.Metadata.Name] = original ? property.OriginalValue : property.CurrentValue;
        }

        try
        {
            return JsonSerializer.Serialize(values, JsonOptions);
        }
        catch (NotSupportedException)
        {
            return "{}";
        }
    }

    private static bool ShouldSkip(PropertyEntry property)
    {
        if (property.Metadata.ClrType == typeof(byte[]))
        {
            return true;
        }

        return PropiedadesSensibles.Contains(property.Metadata.Name);
    }

    private static string? TryGetEntityId(object entity) =>
        entity is BaseEntity baseEntity ? baseEntity.Id.ToString() : null;

    private sealed class PendingAudit
    {
        public required object Entity { get; init; }
        public required EntityState State { get; init; }
        public required TipoOperacionBitacora Tipo { get; init; }
        public required string Entidad { get; init; }
        public string? InformacionAnterior { get; set; }
        public string? InformacionNueva { get; set; }
    }
}
