using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SLCDM.Api.Extensions;
using SLCDM.Application.Common.Interfaces;
using SLCDM.Application.Features.Consulta;
using SLCDM.Application.Features.Consulta.Queries;

namespace SLCDM.Api.Controllers;

[AllowAnonymous]
[ApiController]
[Route("api/consulta")]
[Produces("application/json")]
public sealed class ConsultaController : ControllerBase
{
    private readonly IQueryHandler<GetConsultaActivoQuery, ConsultaActivoDto> _getActivo;

    public ConsultaController(IQueryHandler<GetConsultaActivoQuery, ConsultaActivoDto> getActivo)
    {
        _getActivo = getActivo;
    }

    [HttpGet("activos/{id:int}")]
    [EnableRateLimiting(RateLimitingExtensions.ConsultaPolicy)]
    [ProducesResponseType(typeof(ConsultaActivoDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
    public async Task<ActionResult<ConsultaActivoDto>> GetActivo(
        int id,
        CancellationToken cancellationToken) =>
        Ok(await _getActivo.HandleAsync(new GetConsultaActivoQuery(id), cancellationToken));
}
