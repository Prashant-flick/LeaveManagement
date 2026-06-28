using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Auth.Application.Interfaces;

namespace Auth.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            _logger.LogInformation("Register request received for Email: {Email}", request.Email);

            var response = await _authService.RegisterAsync(request);

            _logger.LogInformation("User registered successfully for Email: {Email}", request.Email);

            return Ok(response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            _logger.LogInformation("Login attempt for Email: {Email}", request.Email);

            var response = await _authService.LoginAsync(request);

            _logger.LogInformation("Login successful for Email: {Email}", request.Email);

            return Ok(response);
        }

        [Authorize]
        [HttpGet("me")]
        public IActionResult Me()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            _logger.LogInformation("Fetching user details for UserId: {UserId}", userId);

            var response = new MeResponse
            {
                UserId = userId,
                Email = User.FindFirstValue(ClaimTypes.Email),
                EmployeeId = User.FindFirstValue("EmployeeId"),
                Roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value)
            };

            return Ok(response);
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            _logger.LogInformation("Refresh token request received");

            var response = await _authService.RefreshAsync(request);

            _logger.LogInformation("Token refreshed successfully");

            return Ok(response);
        }
    }
}