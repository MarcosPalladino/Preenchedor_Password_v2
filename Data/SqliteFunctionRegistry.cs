using System;
using System.Text;
using Microsoft.Data.Sqlite;

namespace TPPreenchedor.Data
{
    public static class SqliteFunctionRegistry
    {
        public static void Register(SqliteConnection connection)
        {
            if (connection == null)
            {
                throw new ArgumentNullException(nameof(connection));
            }

            connection.CreateFunction<string, string>("fncBase64_Encode", Encode);
            connection.CreateFunction<string, string>("fncBase64_Decode", Decode);
        }

        public static string EncodeValue(string value)
        {
            return Encode(value);
        }

        public static string DecodeValue(string value)
        {
            return Decode(value);
        }

        public static bool IsEncodedValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return true;
            }

            try
            {
                var bytes = Convert.FromBase64String(value);
                return Convert.ToBase64String(bytes) == value;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private static string Encode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            var bytes = Encoding.UTF8.GetBytes(value);
            return Convert.ToBase64String(bytes);
        }

        private static string Decode(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value ?? string.Empty;
            }

            try
            {
                var bytes = Convert.FromBase64String(value);
                return Encoding.UTF8.GetString(bytes);
            }
            catch (FormatException)
            {
                // Compatibilidade com dados legados persistidos em texto plano.
                return value;
            }
        }
    }
}
