using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

namespace TPPreenchedor.Services
{
    public static class AppIconProvider
    {
        private static Icon _cachedIcon;

        public static Icon GetIcon()
        {
            if (_cachedIcon != null)
            {
                return _cachedIcon;
            }

            var iconPath = ResolveIconPath();
            if (File.Exists(iconPath))
            {
                _cachedIcon = new Icon(iconPath);
                return _cachedIcon;
            }

            var executablePath = Application.ExecutablePath;
            if (!string.IsNullOrWhiteSpace(executablePath) && File.Exists(executablePath))
            {
                _cachedIcon = Icon.ExtractAssociatedIcon(executablePath);
            }

            return _cachedIcon;
        }

        private static string ResolveIconPath()
        {
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory.TrimEnd(
                Path.DirectorySeparatorChar,
                Path.AltDirectorySeparatorChar);

            var current = new DirectoryInfo(baseDirectory);
            while (current != null)
            {
                var candidate = Path.Combine(current.FullName, "TPIcon1.ico");
                if (File.Exists(candidate))
                {
                    return candidate;
                }

                current = current.Parent;
            }

            return Path.Combine(baseDirectory, "TPIcon1.ico");
        }
    }
}
