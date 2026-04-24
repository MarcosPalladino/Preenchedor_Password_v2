using TPPreenchedor.Data;
using TPPreenchedor.Data.Models;

namespace TPPreenchedor.Services
{
    public class AuthService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public AuthService()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        public Usuario Autenticar(string login, string senha)
        {
            if (string.IsNullOrWhiteSpace(login) || string.IsNullOrWhiteSpace(senha))
            {
                return null;
            }

            var usuario = _usuarioRepository.ObterPorLogin(login.Trim());
            if (usuario == null)
            {
                return null;
            }

            if (!PasswordHasher.Verify(senha, usuario.PasswordHash))
            {
                return null;
            }

            _usuarioRepository.AtualizarUltimoAcesso(usuario.Id);
            return usuario;
        }
    }
}
