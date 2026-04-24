using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using TPPreenchedor.Data;
using TPPreenchedor.Forms;
using TPPreenchedor.Services;

namespace TPPreenchedor
{
    static class Program
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool FreeConsole();

        [STAThread]
        static void Main()
        {

#if !DEBUG
            if (Process.GetProcessesByName(System.IO.Path.GetFileNameWithoutExtension(
                    System.Reflection.Assembly.GetEntryAssembly()?.Location)).Length > 1)
            {
                return;
            }
#endif
            FreeConsole();
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            DatabaseBootstrapper.Initialize();

            var authService = new AuthService();
            using (var loginForm = new LoginForm(authService))
            {
                if (loginForm.ShowDialog() != DialogResult.OK || loginForm.UsuarioAutenticado == null)
                {
                    return;
                }

                Application.Run(new Preenchedor(loginForm.UsuarioAutenticado));
            }
        }
    }
}
