using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using Employee.Application.Features.Roles.Commands.CreateRole;
using Employee.Application.Features.Roles.Queries.GetAllRoles;

namespace Employee.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IMediator mediator,
        ILogger<RoleController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string name)
    {
        _logger.LogInformation(
            "Received request to create role: {RoleName}",
            name);

        var result = await _mediator.Send(new CreateRoleCommand(name));

        _logger.LogInformation(
            "Role created successfully: {RoleName}",
            name);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all roles");

        var roles = await _mediator.Send(new GetAllRolesQuery());

        return Ok(roles);
    }
}