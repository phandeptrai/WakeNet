using System.Security.Cryptography;

namespace WakeNetServer.Services;

public static class PasswordHasher
{
    public static (string hashBase64, string saltBase64, int iterations) Hash(string password, int iterations = 120_000)
    {
        var salt = RandomNumberGenerator.GetBytes(16);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            outputLength: 32);

        return (Convert.ToBase64String(hash), Convert.ToBase64String(salt), iterations);
    }

    public static bool Verify(string password, string hashBase64, string saltBase64, int iterations)
    {
        var salt = Convert.FromBase64String(saltBase64);
        var expected = Convert.FromBase64String(hashBase64);
        var actual = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            iterations,
            HashAlgorithmName.SHA256,
            outputLength: expected.Length);

        return CryptographicOperations.FixedTimeEquals(actual, expected);
    }
}

