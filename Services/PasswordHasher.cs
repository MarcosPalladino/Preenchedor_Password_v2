using System;
using System.Security.Cryptography;

namespace TPPreenchedor.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            using (var deriveBytes = new Rfc2898DeriveBytes(password, 16, 10000))
            {
                var salt = deriveBytes.Salt;
                var key = deriveBytes.GetBytes(32);

                return string.Join(".", Convert.ToBase64String(salt), Convert.ToBase64String(key));
            }
        }

        public static bool Verify(string password, string passwordHash)
        {
            if (string.IsNullOrWhiteSpace(passwordHash))
            {
                return false;
            }

            var parts = passwordHash.Split('.');
            if (parts.Length != 2)
            {
                return false;
            }

            var salt = Convert.FromBase64String(parts[0]);
            var expectedKey = Convert.FromBase64String(parts[1]);

            using (var deriveBytes = new Rfc2898DeriveBytes(password, salt, 10000))
            {
                var actualKey = deriveBytes.GetBytes(32);
                return FixedTimeEquals(actualKey, expectedKey);
            }
        }

        private static bool FixedTimeEquals(byte[] left, byte[] right)
        {
            if (left == null || right == null || left.Length != right.Length)
            {
                return false;
            }

            var diff = 0;
            for (var i = 0; i < left.Length; i++)
            {
                diff |= left[i] ^ right[i];
            }

            return diff == 0;
        }
    }
}
