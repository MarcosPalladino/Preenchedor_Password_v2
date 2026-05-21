using System;
using System.Collections.Generic;
using TPPreenchedor.Data;
using TPPreenchedor.Data.Models;

namespace TPPreenchedor.Services
{
    public class UsuarioService
    {
        private readonly UsuarioRepository _usuarioRepository;

        public UsuarioService()
        {
            _usuarioRepository = new UsuarioRepository();
        }

        public List<Usuario> ListarUsuarios()
        {
            return _usuarioRepository.ObterTodos();
        }

        public Usuario CriarUsuario(string login, string nomeExibicao, string senha)
        {
            ValidarDadosBasicos(login, nomeExibicao);

            if (string.IsNullOrWhiteSpace(senha) || senha.Trim().Length < 4)
            {
                throw new InvalidOperationException("Informe uma senha com pelo menos 4 caracteres.");
            }

            if (_usuarioRepository.ObterPorLoginSemFiltro(login.Trim()) != null)
            {
                throw new InvalidOperationException("Ja existe um usuario com esse login.");
            }

            var usuario = new Usuario
            {
                Login = login.Trim(),
                NomeExibicao = nomeExibicao.Trim(),
                PasswordHash = PasswordHasher.Hash(senha),
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            return _usuarioRepository.Adicionar(usuario);
        }

        public Usuario AtualizarUsuario(int id, string login, string nomeExibicao, bool ativo)
        {
            ValidarDadosBasicos(login, nomeExibicao);

            var usuario = _usuarioRepository.ObterPorId(id);
            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario nao encontrado.");
            }

            var existente = _usuarioRepository.ObterPorLoginSemFiltro(login.Trim());
            if (existente != null && existente.Id != id)
            {
                throw new InvalidOperationException("Ja existe um usuario com esse login.");
            }

            usuario.Login = login.Trim();
            usuario.NomeExibicao = nomeExibicao.Trim();
            usuario.Ativo = ativo;

            _usuarioRepository.Atualizar(usuario);
            return usuario;
        }

        public void RedefinirSenha(int id, string novaSenha)
        {
            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Trim().Length < 4)
            {
                throw new InvalidOperationException("Informe uma nova senha com pelo menos 4 caracteres.");
            }

            var usuario = _usuarioRepository.ObterPorId(id);
            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario nao encontrado.");
            }

            usuario.PasswordHash = PasswordHasher.Hash(novaSenha);
            _usuarioRepository.Atualizar(usuario);
        }

        public void AlterarSenhaAtual(int usuarioId, string senhaAtual, string novaSenha, string confirmacao)
        {
            if (string.IsNullOrWhiteSpace(novaSenha) || novaSenha.Trim().Length < 4)
            {
                throw new InvalidOperationException("Informe uma nova senha com pelo menos 4 caracteres.");
            }

            if (novaSenha != confirmacao)
            {
                throw new InvalidOperationException("A confirmacao de senha nao confere.");
            }

            var usuario = _usuarioRepository.ObterPorId(usuarioId);
            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario nao encontrado.");
            }

            if (!PasswordHasher.Verify(senhaAtual, usuario.PasswordHash))
            {
                throw new InvalidOperationException("A senha atual esta incorreta.");
            }

            usuario.PasswordHash = PasswordHasher.Hash(novaSenha);
            _usuarioRepository.Atualizar(usuario);
        }

        public void ExcluirUsuario(int id, int? usuarioLogadoId = null)
        {
            if (usuarioLogadoId.HasValue && usuarioLogadoId.Value == id)
            {
                throw new InvalidOperationException("Nao e permitido excluir o usuario atualmente logado.");
            }

            var usuario = _usuarioRepository.ObterPorId(id);
            if (usuario == null)
            {
                throw new InvalidOperationException("Usuario nao encontrado.");
            }

            _usuarioRepository.Remover(id);
        }

        private static void ValidarDadosBasicos(string login, string nomeExibicao)
        {
            if (string.IsNullOrWhiteSpace(login))
            {
                throw new InvalidOperationException("Informe o login do usuario.");
            }

            if (string.IsNullOrWhiteSpace(nomeExibicao))
            {
                throw new InvalidOperationException("Informe o nome de exibicao do usuario.");
            }
        }
    }
}
