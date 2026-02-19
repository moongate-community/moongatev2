using System.Globalization;
using System.Security.Cryptography;

namespace Moongate.Core.Utils;

/// <summary>
/// Provides password hashing and verification helpers using PBKDF2-SHA256.
/// </summary>
public static class HashUtils
{
    private const string Algorithm = "pbkdf2-sha256";
    private const int DefaultIterations = 100_000;
    private const int DefaultSaltSize = 16;
    private const int DefaultHashSize = 32;

    /// <summary>
    /// Hashes a password using PBKDF2-SHA256 and returns a serialized payload.
    /// </summary>
    /// <param name="password">Plain password.</param>
    /// <returns>Serialized hash payload.</returns>
    public static string HashPassword(string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("Password cannot be null or empty.", nameof(password));
        }

        var salt = RandomNumberGenerator.GetBytes(DefaultSaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            DefaultIterations,
            HashAlgorithmName.SHA256,
            DefaultHashSize
        );

        return string.Create(
            CultureInfo.InvariantCulture,
            $"{Algorithm}${DefaultIterations}${Convert.ToBase64String(salt)}${Convert.ToBase64String(hash)}"
        );
    }

    /// <summary>
    /// Verifies a plain password against a serialized PBKDF2-SHA256 payload.
    /// </summary>
    /// <param name="password">Plain password.</param>
    /// <param name="storedHash">Serialized hash payload.</param>
    /// <returns>True when password matches; otherwise false.</returns>
    public static bool VerifyPassword(string password, string storedHash)
    {
        if (string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(storedHash))
        {
            return false;
        }

        var parts = storedHash.Split('$');

        if (parts.Length != 4 || !string.Equals(parts[0], Algorithm, StringComparison.Ordinal))
        {
            return false;
        }

        if (
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var iterations) ||
            iterations <= 0
        )
        {
            return false;
        }

        try
        {
            var salt = Convert.FromBase64String(parts[2]);
            var expectedHash = Convert.FromBase64String(parts[3]);
            var actualHash = Rfc2898DeriveBytes.Pbkdf2(
                password,
                salt,
                iterations,
                HashAlgorithmName.SHA256,
                expectedHash.Length
            );

            return CryptographicOperations.FixedTimeEquals(actualHash, expectedHash);
        }
        catch (FormatException)
        {
            return false;
        }
    }
}
