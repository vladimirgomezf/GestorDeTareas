using GestorDeTareas.Business.DTOs;
using GestorDeTareas.Business.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestorDeTareas.Api.Controllers;

[Route("api/tasks")]
public sealed class TasksController(ITaskService service) : BaseController
{
    [HttpPost("list")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskPage), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List([FromBody] TaskListRequest request,
        CancellationToken cancellationToken = default)
    {
        var result = await service.ListAsync(request, cancellationToken);
        return ToActionResult(result);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await service.GetAsync(id, cancellationToken);
        return ToActionResult(result, id);
    }

    [HttpPost]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] TaskRequest request, CancellationToken cancellationToken)
    {
        var result = await service.CreateAsync(request, cancellationToken);
        if (result.Errors.Count > 0)
        {
            return ToActionResult(result);
        }

        return CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value);
    }

    [HttpPut("{id:int}")]
    [Consumes("application/json")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        [FromRoute] int id,
        [FromBody] TaskRequest request,
        CancellationToken cancellationToken)
    {
        var result = await service.UpdateAsync(id, request, cancellationToken);
        return ToActionResult(result, id);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id, CancellationToken cancellationToken)
    {
        var result = await service.DeleteAsync(id, cancellationToken);
        if (result.Errors.Count > 0 || result.NotFound)
        {
            return ToActionResult(result, id);
        }

        return NoContent();
    }
}