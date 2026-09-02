using System.Security.Cryptography;
using SLCDM.Application.Common.Interfaces;

namespace SLCDM.Application.Common.Security;

/// <summary>
/// Contraseña temporal para altas de usuario. Usa RNG criptográfico,
/// garantiza mayúscula, minúscula, dígito y símbolo, y evita caracteres
/// ambiguos (0/O, 1/l/I). El resultado se hashea con Argon2id; esta
/// cadena en claro solo se entrega una vez en la respuesta de creación.
/// </summary>
public sealed class PasswordGenerator : IPasswordGenerator
{
    private const string Lowers = "abcdefghijkmnpqrstuvwxyz";
    private const string Uppers = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits = "23456789";
    private const string Symbols = "!@#$%&*?";
    private const string All = Lowers + Uppers + Digits + Symbols;
    private const int MinLength = 8;

    public string Generate(int length = 12)
    {
        if (length < MinLength)
        {
            throw new ArgumentOutOfRangeException(nameof(length), length, "La contraseña debe tener al menos 8 caracteres.");
        }

        var buffer = new char[length];
        buffer[0] = Pick(Uppers);
        buffer[1] = Pick(Lowers);
        buffer[2] = Pick(Digits);
        buffer[3] = Pick(Symbols);

        for (var i = 4; i < length; i++)
        {
            buffer[i] = Pick(All);
        }

        Shuffle(buffer);
        return new string(buffer);
    }

    private static char Pick(string alphabet) =>
        alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

    private static void Shuffle(Span<char> buffer)
    {
        for (var i = buffer.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (buffer[i], buffer[j]) = (buffer[j], buffer[i]);
        }
    }
}
