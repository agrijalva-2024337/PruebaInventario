namespace SLCDM.Application.Features.Dispositivos;

public sealed record DispositivoTokenDto(
    int Id,
    int IdActivo,
    string TokenCrudo,
    DateTime CreadoEn,
    DateTime? ExpiraEn);
