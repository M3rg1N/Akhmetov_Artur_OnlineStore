
using System;
using System.Security.Cryptography;
using System.Text;

namespace OnlineStore.Helpers
{
    public static class PasswordHelper
    {
        public static string Hash(string password)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(password);
            byte[] hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }

        public static bool Verify(string password, string stored)
        {
            return Hash(password) == stored;
        }
    }
}