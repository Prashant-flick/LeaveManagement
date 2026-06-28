using System.Net;
using System.Net.Http.Json;
using Auth.Application.DTOs;
using Auth.Application.Interfaces;
using Auth.Application.Common.Exceptions;
using Microsoft.Extensions.Logging;

namespace Auth.Infrastructure.Services;

public class EmployeeClient : IEmployeeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeClient> _logger;

    public EmployeeClient(
        HttpClient httpClient,
        ILogger<EmployeeClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<UserRoleResponse> GetRolesAndEmployeeIdByUserIdAsync(int userId)
    {
        _logger.LogInformation(
            "Calling EmployeeService for UserId: {UserId}",
            userId);

        try
        {
            var response = await _httpClient.GetAsync(
                $"/api/employee/roles/{userId}");

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning(
                    "Employee not found in EmployeeService for UserId: {UserId}. Falling back to default role 'User'.",
                    userId);

                return new UserRoleResponse
                {
                    EmployeeId = 0,
                    Roles = new List<string> { "User" }
                };
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "EmployeeService returned error {StatusCode} for UserId: {UserId}",
                    response.StatusCode,
                    userId);

                throw new Exception("Error communicating with EmployeeService");
            }

            var result = await response.Content.ReadFromJsonAsync<UserRoleResponse>();

            if (result == null)
            {
                _logger.LogError(
                    "Null response from EmployeeService for UserId: {UserId}",
                    userId);

                throw new Exception("Invalid response from EmployeeService");
            }

            _logger.LogInformation(
                "Received roles for UserId: {UserId}, Roles: {Roles}, EmployeeId: {EmployeeId}",
                userId,
                string.Join(",", result.Roles ?? new List<string>()),
                result.EmployeeId);

            return result;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(
                ex,
                "HTTP error while calling EmployeeService for UserId: {UserId}",
                userId);

            throw new Exception("Employee service is unavailable");
        }
        catch (TaskCanceledException ex)
        {
            _logger.LogError(
                ex,
                "Timeout while calling EmployeeService for UserId: {UserId}",
                userId);

            throw new Exception("Employee service timeout");
        }
    }
}