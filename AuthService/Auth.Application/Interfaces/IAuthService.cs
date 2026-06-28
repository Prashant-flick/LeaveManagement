namespace Auth.Application.Interfaces
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        Task<LoginResponse> LoginAsync(LoginRequest request);

        Task<LoginResponse> RefreshAsync(RefreshRequest request);
    }
}
