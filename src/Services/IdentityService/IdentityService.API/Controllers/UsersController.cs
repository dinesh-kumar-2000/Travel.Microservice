using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IdentityService.Application.Queries.User;
using IdentityService.Application.Commands.User;
using IdentityService.Contracts.DTOs;
using IdentityService.Contracts.Requests.User;
using IdentityService.Contracts.Responses.User;
using Identity.Shared;
using SharedKernel.Models;

namespace IdentityService.API.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/[controller]")]
[ApiVersion("1.0")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ICurrentUserService _currentUser;

    public UsersController(IMediator mediator, ICurrentUserService currentUser)
    {
        _mediator = mediator;
        _currentUser = currentUser;
    }

    /// <summary>
    /// Get user by ID (Admin only)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetById(string id)
    {
        var query = new GetUserByIdQuery { Id = Guid.Parse(id) };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get current logged-in user profile
    /// </summary>
    [HttpGet("me")]
    [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserDto>> GetCurrentUser()
    {
        var query = new GetUserByIdQuery { Id = Guid.Parse(_currentUser.UserId!) };
        var result = await _mediator.Send(query);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    /// <summary>
    /// Get all users (Admin only)
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(PaginatedResult<UserResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<UserResponse>>> GetAllUsers(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? searchTerm = null)
    {
        var query = new GetAllUsersQuery
        {
            PageNumber = pageNumber,
            PageSize = pageSize,
            SearchTerm = searchTerm
        };

        var result = await _mediator.Send(query);
        return Ok(result);
    }

    /// <summary>
    /// Get user by email (Admin only)
    /// </summary>
    [HttpGet("by-email/{email}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> GetUserByEmail(string email)
    {
        var query = new GetUserByEmailQuery { Email = email };
        var result = await _mediator.Send(query);
        
        if (result == null)
            return NotFound();
            
        return Ok(result);
    }

    /// <summary>
    /// Create a new user (Admin only)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<UserResponse>> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand
        {
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Password = request.Password,
            RoleIds = request.RoleIds
        };

        var result = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Update user (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UserResponse>> UpdateUser(string id, [FromBody] UpdateUserRequest request)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.Parse(id),
            FirstName = request.FirstName,
            LastName = request.LastName,
            PhoneNumber = request.PhoneNumber
        };

        var result = await _mediator.Send(command);
        return Ok(result);
    }

    /// <summary>
    /// Delete user (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var command = new DeleteUserCommand { UserId = Guid.Parse(id) };
        await _mediator.Send(command);
        return NoContent();
    }

    /// <summary>
    /// Change user password
    /// </summary>
    [HttpPost("{id}/change-password")]
    [Authorize(Roles = "SuperAdmin,TenantAdmin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ChangePassword(string id, [FromBody] ChangePasswordRequest request)
    {
        var command = new ChangePasswordCommand
        {
            UserId = Guid.Parse(id),
            CurrentPassword = request.CurrentPassword,
            NewPassword = request.NewPassword
        };

        await _mediator.Send(command);
        return NoContent();
    }
}

