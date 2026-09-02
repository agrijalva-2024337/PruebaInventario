using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Common.Security;

/// <summary>
/// Argon2id, mismo enfoque que el auth-service de Biblioteca Virtual La 33.
/// Comparacion en tiempo constante (sin logs de debug).
/// </summary>
public sealed class PasswordHashService : IPasswordHashService
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 2;
    private const int MemoryKb = 65536;
    private const int Parallelism = 1;

    public string HashPassword(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        byte[] hash = ComputeHash(password, salt, MemoryKb, Iterations, Parallelism, HashSize);
        return FormatHash(salt, hash, MemoryKb, Iterations, Parallelism);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        if (string.IsNullOrWhiteSpace(hashedPassword))
        {
            return false;
        }

        try
        {
            if (!hashedPassword.StartsWith("$argon2id$", StringComparison.Ordinal))
            {
                return false;
            }

            var parts = hashedPassword.Split('$', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 5)
            {
                return false;
            }

            var parameters = parts[2].Split(',');
            var memory = int.Parse(parameters[0].Split('=')[1]);
            var iterations = int.Parse(parameters[1].Split('=')[1]);
            var parallelism = int.Parse(parameters[2].Split('=')[1]);
            var salt = Convert.FromBase64String(FromBase64UrlSafe(parts[3]));
            var expected = Convert.FromBase64String(FromBase64UrlSafe(parts[4]));
            var computed = ComputeHash(password, salt, memory, iterations, parallelism, expected.Length);

            return CryptographicOperations.FixedTimeEquals(expected, computed);
        }
        catch
        {
            return false;
        }
    }

    private static byte[] ComputeHash(
        string password,
        byte[] salt,
        int memoryKb,
        int iterations,
        int parallelism,
        int hashSize)
    {
        using var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password))
        {
            Salt = salt,
            DegreeOfParallelism = parallelism,
            Iterations = iterations,
            MemorySize = memoryKb
        };

        return argon2.GetBytes(hashSize);
    }

    private static string FormatHash(byte[] salt, byte[] hash, int memoryKb, int iterations, int parallelism) =>
        $"$argon2id$v=19$m={memoryKb},t={iterations},p={parallelism}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}";

    private static string FromBase64UrlSafe(string value)
    {
        var base64 = value.Replace('-', '+').Replace('_', '/');
        return (base64.Length % 4) switch
        {
            2 => base64 + "==",
            3 => base64 + "=",
            _ => base64
        };
    }
}
