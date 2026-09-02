namespace SLCDM.Application.Features.Asignaciones;

public static class AsignacionDocumento
{
    public static string PdfRelativeUrl(int idAsignacion) => $"/api/asignaciones/{idAsignacion}/pdf";

    public static bool TieneTinta(byte[]? firma) => firma is { Length: > 32 };
}
