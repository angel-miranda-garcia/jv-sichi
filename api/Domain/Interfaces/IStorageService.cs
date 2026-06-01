namespace Domain.Interfaces;

public interface IStorageService
{
    Task<string> SaveFileAsync(Stream stream, string filename);
    Task DeleteFileAsync(string filePath);
    Task<string> GetChecksumAsync(string filePath);
}
