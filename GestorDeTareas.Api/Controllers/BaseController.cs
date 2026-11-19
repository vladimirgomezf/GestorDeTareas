using GestorDeTareas.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestorDeTareas.Api.Controllers;

[ApiController]
[Produces("application/json")]
public abstract class BaseController : ControllerBase
{
    protected IActionResult ToActionResult<T>(TaskServiceResult<T> result, int? taskId = null)
    {
        if (result.Errors.Count > 0)
        {
            var validationProblem = new ValidationProblemDetails(
                result.Errors.ToDictionary(error => error.Key, error => error.Value));
            return BadRequest(validationProblem);
        }

        if (result.NotFound)
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Tarea no encontrada",
                detail: $"No existe una tarea con el identificador {taskId}.");
        }

        return Ok(result.Value);
    }
}