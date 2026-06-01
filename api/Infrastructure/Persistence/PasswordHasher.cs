namespace Infrastructure.Persistence;

public static class PasswordHasher
{
    public static string HashPassword(string password) =>
        BCrypt.Net.BCrypt.HashPassword(password);
}
