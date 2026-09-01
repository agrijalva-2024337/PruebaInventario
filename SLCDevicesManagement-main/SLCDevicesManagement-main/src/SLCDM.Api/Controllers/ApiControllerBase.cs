using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SLCDM.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
[Produces("application/json")]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult IdMismatch() =>
        BadRequest(new { detail = "El id de la ruta no coincide con el del cuerpo." });

    protected IActionResult CreatedId(string actionName, int id) =>
        CreatedAtAction(actionName, new { id }, new { id });

    protected IActionResult CreatedId(string actionName, int id, object value) =>
        CreatedAtAction(actionName, new { id }, value);
}
