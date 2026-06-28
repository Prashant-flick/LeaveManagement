using System.Net;
using System.Net.Http.Json;
using Leave.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace Leave.Infrastructure.Services;

public class EmployeeClient : IEmployeeClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeClient> _logger;

    public EmployeeClient(HttpClient httpClient, ILogger<EmployeeClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<int?> GetManagerIdAsync(int employeeId)
    {
        var response = await _httpClient.GetFromJsonAsync<EmployeeManagerResponse>(
            $"/api/employee/{employeeId}/manager"
        );

        return response?.ManagerId;
    }

    private class EmployeeManagerResponse
    {
        public int? ManagerId { get; set; }
    }


    public async Task<bool> EmployeeExistsAsync(int employeeId)
    {
        try
        {
            _logger.LogInformation("Calling EmployeeService for EmployeeId: {EmployeeId}", employeeId);

            var response = await _httpClient.GetAsync($"/api/employee/{employeeId}");

            _logger.LogInformation("response: {}" , response);

            if (response.StatusCode == HttpStatusCode.OK)
            {

                _logger.LogInformation("Employee exists: {EmployeeId}", employeeId);
                return true;
            }

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Employee NOT found: {EmployeeId}", employeeId);
                return false;
            }

            _logger.LogError(
                "Unexpected response from EmployeeService. StatusCode: {StatusCode}",
                response.StatusCode);

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calling EmployeeService");
            throw new Exception("Error communicating with EmployeeService");
        }
    }

}