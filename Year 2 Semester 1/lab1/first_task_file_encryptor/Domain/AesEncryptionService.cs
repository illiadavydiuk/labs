using first_task_file_encryptor.Domain.Interfaces;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace first_task_file_encryptor.Domain
{
    public class AesEncryptionService : IEncryptionService
    {
        private const int SaltSize = 16;
        private const int KeySize = 32;
        private const int NonceSize = 12;
        private const int TagSize = 16;
        private const int Iterations = 100000;

        public async Task<bool> EncryptFileAsync(string inputFile, string outputFile, string password)
        {
            try
            {
                byte[] salt = RandomNumberGenerator.GetBytes(SaltSize);
                byte[] key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256).GetBytes(KeySize);
                byte[] nonce = RandomNumberGenerator.GetBytes(NonceSize);

                byte[] fileBytes = await File.ReadAllBytesAsync(inputFile);
                byte[] ciphertext = new byte[fileBytes.Length];
                byte[] tag = new byte[TagSize];

                using (var aesGcm = new AesGcm(key))
                {
                    aesGcm.Encrypt(nonce, fileBytes, ciphertext, tag);
                }

                await using (var fs = new FileStream(outputFile, FileMode.Create))
                {
                    await fs.WriteAsync(salt);
                    await fs.WriteAsync(nonce);
                    await fs.WriteAsync(tag);
                    await fs.WriteAsync(ciphertext);
                }
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DecryptFileAsync(string inputFile, string outputFile, string password)
        {
            try
            {
                byte[] encryptedFileBytes = await File.ReadAllBytesAsync(inputFile);

                byte[] salt = encryptedFileBytes[..SaltSize];
                byte[] nonce = encryptedFileBytes[SaltSize..(SaltSize + NonceSize)];
                byte[] tag = encryptedFileBytes[(SaltSize + NonceSize)..(SaltSize + NonceSize + TagSize)];
                byte[] ciphertext = encryptedFileBytes[(SaltSize + NonceSize + TagSize)..];

                byte[] key = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256).GetBytes(KeySize);
                byte[] decryptedBytes = new byte[ciphertext.Length];

                using (var aesGcm = new AesGcm(key))
                {
                    aesGcm.Decrypt(nonce, ciphertext, tag, decryptedBytes);
                }

                await File.WriteAllBytesAsync(outputFile, decryptedBytes);
                return true;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch
            {
                return false;
            }
        }
    }
}