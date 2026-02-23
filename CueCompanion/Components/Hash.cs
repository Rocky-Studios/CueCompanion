using System.Security.Cryptography;
using System.Text;

namespace CueCompanion.Components;

public static class Hash
{
    public static string HashPassword(string password)
    {
        using SHA256 sha256 = SHA256.Create();
        byte[] salt = "ReecesPieces".Select(c => (byte)c).ToArray();

        byte[] passwordBytes = Encoding.UTF8.GetBytes(password);
        byte[] saltedPassword = new byte[salt.Length + passwordBytes.Length];
        Buffer.BlockCopy(salt, 0, saltedPassword, 0, salt.Length);
        Buffer.BlockCopy(passwordBytes, 0, saltedPassword, salt.Length, passwordBytes.Length);

        byte[] hash = sha256.ComputeHash(saltedPassword);
        byte[] hashWithSalt = new byte[salt.Length + hash.Length];
        Buffer.BlockCopy(salt, 0, hashWithSalt, 0, salt.Length);
        Buffer.BlockCopy(hash, 0, hashWithSalt, salt.Length, hash.Length);

        return Convert.ToBase64String(hashWithSalt);
    }
}