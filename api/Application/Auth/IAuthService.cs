namespace Application.Auth;

public interface IAuthService
{
    Task<LoginResult?> LoginAsync(string email, string password);
}
