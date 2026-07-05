using Application.Features.Admins.Commands.CreateAdmin;
using Application.Features.Admins.Commands.DeleteAdmin;
using Application.Features.Admins.Commands.UpdateAdmin;
using Application.Features.Admins.Queries.GetAdminById;
using Application.Features.Admins.Queries.GetAllAdmins;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DoctorBooking.Controllers;

[Route("api/[controller]")]
[ApiController]
public sealed class AdminsController : ControllerBase
{
    private readonly ISender _mediator;

    public AdminsController(ISender mediator) => _mediator = mediator;

    /// <summary>Get all admins (paginated).</summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        var result = await _mediator.Send(new GetAllAdminsQuery(page, pageSize));
        return Ok(result);
    }

    /// <summary>Get admin by ID.</summary>
    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetById(long id)
    {
        var result = await _mediator.Send(new GetAdminByIdQuery(id));
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Create a new admin.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateAdminCommand command)
    {
        var result = await _mediator.Send(command);
        return result.Success
            ? CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result)
            : BadRequest(result);
    }

    /// <summary>Update an existing admin.</summary>
    [HttpPut("{id:long}")]
    public async Task<IActionResult> Update(long id, [FromBody] UpdateAdminRequest request)
    {
        var command = new UpdateAdminCommand(id, request.FullName, request.Email, request.Role);
        var result = await _mediator.Send(command);
        return result.Success ? Ok(result) : NotFound(result);
    }

    /// <summary>Delete an admin.</summary>
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Delete(long id)
    {
        var result = await _mediator.Send(new DeleteAdminCommand(id));
        return result.Success ? Ok(result) : NotFound(result);
    }
}

/// <summary>Input DTO for admin updates.</summary>
public sealed record UpdateAdminRequest(
    string FullName,
    string Email,
    string Role);
