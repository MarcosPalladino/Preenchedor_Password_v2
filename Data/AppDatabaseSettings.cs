using System.Configuration;
using System;
using System.IO;
using System.Linq;

namespace TPPreenchedor.Data
{
    public static class AppDatabaseSettings
    {
        private const string DefaultSqliteRelativePath = @"Database\TpPreenchedor.db";

        public static string Provider => ConfigurationManager.AppSettings["databaseProvider"] ?? "sqlite";

        public static string SqliteConnectionString => BuildSqliteConnectionString();

        public static string SqliteDatabasePath => BuildSqliteDatabasePath();

        public static string SqlServerConnectionString =>
            ConfigurationManager.AppSettings["sqlServerConnectionString"] ?? string.Empty;

        private static string BuildSqliteConnectionString()
        {
            return "Data Source=" + BuildSqliteDatabasePath();
        }

        private static string BuildSqliteDatabasePath()
        {
            var configured = ConfigurationManager.AppSettings["sqliteConnectionString"] ?? "Data Source=" + DefaultSqliteRelativePath;
            var prefix = "Data Source=";
            if (!configured.StartsWith(prefix))
            {
                return configured;
            }

            var dbPath = configured.Substring(prefix.Length).Trim();
            if (string.IsNullOrWhiteSpace(dbPath))
            {
                dbPath = DefaultSqliteRelativePath;
            }

            if (!Path.IsPathRooted(dbPath))
            {
                dbPath = Path.Combine(GetProjectRootDirectory(), dbPath);
            }

            EnsureDatabaseDirectory(dbPath);
            TryMigrateLegacyDatabase(dbPath);
            return dbPath;
        }

        private static string GetProjectRootDirectory()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var current = new DirectoryInfo(baseDirectory);

            while (current != null)
            {
                if (File.Exists(Path.Combine(current.FullName, "TPPreenchedor.sln")) ||
                    File.Exists(Path.Combine(current.FullName, "TPPreenchedor.csproj")))
                {
                    return current.FullName;
                }

                current = current.Parent;
            }

            return baseDirectory;
        }

        private static void EnsureDatabaseDirectory(string dbPath)
        {
            var directory = Path.GetDirectoryName(dbPath);
            if (!string.IsNullOrWhiteSpace(directory))
            {
                Directory.CreateDirectory(directory);
            }
        }

        private static void TryMigrateLegacyDatabase(string targetPath)
        {
            if (File.Exists(targetPath) && new FileInfo(targetPath).Length > 0)
            {
                return;
            }

            var candidate = GetLegacyCandidates()
                .Where(File.Exists)
                .Select(path => new FileInfo(path))
                .Where(info => info.Length > 0)
                .OrderByDescending(info => info.LastWriteTimeUtc)
                .FirstOrDefault();

            if (candidate == null)
            {
                return;
            }

            if (string.Equals(candidate.FullName, targetPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            File.Copy(candidate.FullName, targetPath, true);
        }

        private static string[] GetLegacyCandidates()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            var projectRoot = GetProjectRootDirectory();
            var localAppData = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TPPreenchedor",
                "TpPreenchedor.db");

            return new[]
            {
                Path.Combine(projectRoot, DefaultSqliteRelativePath),
                localAppData,
                Path.Combine(baseDirectory, "TpPreenchedor.db"),
                Path.Combine(projectRoot, "TpPreenchedor.db")
            };
        }
    }
}
