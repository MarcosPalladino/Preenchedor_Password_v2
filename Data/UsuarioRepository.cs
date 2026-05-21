using System;
using System.Collections.Generic;
using System.Linq;
using TPPreenchedor.Data.Models;

namespace TPPreenchedor.Data
{
    public class UsuarioRepository
    {
        public List<Usuario> ObterTodos()
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Usuarios
                    .OrderBy(x => x.Login)
                    .ToList();
            }
        }

        public Usuario ObterPorId(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Usuarios.FirstOrDefault(x => x.Id == id);
            }
        }

        public Usuario ObterPorLogin(string login)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Usuarios.FirstOrDefault(x => x.Login == login && x.Ativo);
            }
        }

        public Usuario ObterPorLoginSemFiltro(string login)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.Usuarios.FirstOrDefault(x => x.Login == login);
            }
        }

        public Usuario Adicionar(Usuario usuario)
        {
            using (var context = new ApplicationDbContext())
            {
                context.Usuarios.Add(usuario);
                context.SaveChanges();
                return usuario;
            }
        }

        public void Atualizar(Usuario usuario)
        {
            using (var context = new ApplicationDbContext())
            {
                context.Usuarios.Update(usuario);
                context.SaveChanges();
            }
        }

        public void Remover(int id)
        {
            using (var context = new ApplicationDbContext())
            {
                var usuario = context.Usuarios.FirstOrDefault(x => x.Id == id);
                if (usuario == null)
                {
                    return;
                }

                context.Usuarios.Remove(usuario);
                context.SaveChanges();
            }
        }

        public void AtualizarUltimoAcesso(int usuarioId)
        {
            using (var context = new ApplicationDbContext())
            {
                var usuario = context.Usuarios.FirstOrDefault(x => x.Id == usuarioId);
                if (usuario == null)
                {
                    return;
                }

                usuario.UltimoAcesso = DateTime.UtcNow;
                context.SaveChanges();
            }
        }
    }
}
