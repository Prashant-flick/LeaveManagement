using System.Security.Claims;

namespace Leave.API.Extensions;

public static class ClaimExtensions
{
    public static int GetEmployeeId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst("EmployeeId")?.Value;

        if (string.IsNullOrEmpty(value))
            throw new UnauthorizedAccessException("EmployeeId missing in token");

        return int.Parse(value);
    }
}