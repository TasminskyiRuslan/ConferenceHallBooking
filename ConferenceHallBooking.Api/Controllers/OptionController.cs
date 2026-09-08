using ConferenceHallBooking.Application.DTOs.Options;
using ConferenceHallBooking.Application.Features.Options.Commands;
using ConferenceHallBooking.Application.Features.Options.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ConferenceHallBooking.Api.Controllers;

/// <summary>
/// Manages service options (e.g., projector, Wi-Fi, sound system).
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OptionController(ISender sender) : ControllerBase
{
    /// <summary>
    /// Gets a service option by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(OptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var option = await sender.Send(new GetOptionByIdQuery(id), cancellationToken);
        return Ok(option);
    }

    /// <summary>
    /// Gets all available service options.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyCollection<OptionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var options = await sender.Send(new GetAllOptionsQuery(), cancellationToken);
        return Ok(options);
    }

    /// <summary>
    /// Creates a new service option.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OptionResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(
        [FromBody] CreateOptionCommand command,
        CancellationToken cancellationToken)
    {
        var option = await sender.Send(command, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = option.Id }, option);
    }

    /// <summary>
    /// Updates an existing service option.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(OptionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateOptionRequest request,
        CancellationToken cancellationToken)
    {
        var option = await sender.Send(new UpdateOptionCommand(id, request.Name, request.Price), cancellationToken);
        return Ok(option);
    }

    /// <summary>
    /// Deletes a service option.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteOptionCommand(id), cancellationToken);
        return NoContent();
    }
}
