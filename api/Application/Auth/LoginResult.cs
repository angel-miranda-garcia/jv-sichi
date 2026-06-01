namespace Application.Auth;

public record LoginResult(string Token, DateTime ExpiresAt);
