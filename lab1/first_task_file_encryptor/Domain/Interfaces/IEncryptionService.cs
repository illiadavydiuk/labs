using System.Threading.Tasks;

namespace first_task_file_encryptor.Domain.Interfaces
{
    public interface IEncryptionService
    {
        Task<bool> EncryptFileAsync(string inputFile, string outputFile, string password);
        Task<bool> DecryptFileAsync(string inputFile, string outputFile, string password);
    }
}