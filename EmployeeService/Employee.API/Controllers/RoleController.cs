using Employee.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Employee.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RoleController : ControllerBase
{
    private readonly IRoleService _service;
    private readonly ILogger<RoleController> _logger;

    public RoleController(
        IRoleService service,
        ILogger<RoleController> logger)
    {
        _service = service;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] string name)
    {
        _logger.LogInformation(
            "Received request to create role: {RoleName}",
            name);

        var result = await _service.CreateRoleAsync(name);

        _logger.LogInformation(
            "Role created successfully: {RoleName}",
            name);

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        _logger.LogInformation("Fetching all roles");

        var roles = await _service.GetAllRolesAsync();

        return Ok(roles);
    }
}