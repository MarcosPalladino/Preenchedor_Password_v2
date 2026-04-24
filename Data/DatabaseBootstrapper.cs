using System;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TPPreenchedor.Data.Models;
using TPPreenchedor.Services;

namespace TPPreenchedor.Data
{
    public static class DatabaseBootstrapper
    {
        public static void Initialize()
        {
            using (var context = new ApplicationDbContext())
            {
                EnsureSchema(context);
                EnsureAdminSeed(context);
                EnsureEncodedItemValues();
            }
        }

        private static void EnsureSchema(ApplicationDbContext context)
        {
            context.Database.EnsureCreated();

            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                return;
            }

            context.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS AppUsuarios (
    Id INTEGER NOT NULL CONSTRAINT PK_AppUsuarios PRIMARY KEY AUTOINCREMENT,
    Login TEXT NOT NULL,
    PasswordHash TEXT NOT NULL,
    NomeExibicao TEXT NOT NULL,
    Ativo INTEGER NOT NULL,
    DataCriacao TEXT NOT NULL,
    UltimoAcesso TEXT NULL
);");

            context.Database.ExecuteSqlRaw(
                @"CREATE UNIQUE INDEX IF NOT EXISTS IX_AppUsuarios_Login ON AppUsuarios (Login);");

            context.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS AppCategorias (
    Id INTEGER NOT NULL CONSTRAINT PK_AppCategorias PRIMARY KEY AUTOINCREMENT,
    Nome TEXT NOT NULL,
    OrdemExibicao INTEGER NOT NULL,
    Ativo INTEGER NOT NULL
);");

            context.Database.ExecuteSqlRaw(
                @"CREATE UNIQUE INDEX IF NOT EXISTS IX_AppCategorias_Nome ON AppCategorias (Nome);");

            context.Database.ExecuteSqlRaw(@"
CREATE TABLE IF NOT EXISTS AppItensPreenchimento (
    Id INTEGER NOT NULL CONSTRAINT PK_AppItensPreenchimento PRIMARY KEY AUTOINCREMENT,
    UsuarioId INTEGER NOT NULL,
    CategoriaId INTEGER NULL,
    TituloExibicao TEXT NOT NULL,
    LoginReferencia TEXT NULL,
    TipoItem INTEGER NOT NULL,
    Valor TEXT NULL DEFAULT '',
    OrdemExibicao INTEGER NOT NULL,
    Ativo INTEGER NOT NULL,
    Observacao TEXT NULL,
    CONSTRAINT FK_AppItensPreenchimento_AppUsuarios_UsuarioId FOREIGN KEY (UsuarioId) REFERENCES AppUsuarios (Id) ON DELETE CASCADE,
    CONSTRAINT FK_AppItensPreenchimento_AppCategorias_CategoriaId FOREIGN KEY (CategoriaId) REFERENCES AppCategorias (Id) ON DELETE RESTRICT
);");

            context.Database.ExecuteSqlRaw(
                @"CREATE INDEX IF NOT EXISTS IX_AppItensPreenchimento_UsuarioId ON AppItensPreenchimento (UsuarioId);");

            context.Database.ExecuteSqlRaw(
                @"CREATE INDEX IF NOT EXISTS IX_AppItensPreenchimento_CategoriaId ON AppItensPreenchimento (CategoriaId);");
        }

        private static void SeedDefaultData(ApplicationDbContext context)
        {
            var categoriaCredenciais = new ItemCategoria
            {
                Nome = "Credenciais",
                OrdemExibicao = 1,
                Ativo = true
            };

            var categoriaTextos = new ItemCategoria
            {
                Nome = "Textos",
                OrdemExibicao = 2,
                Ativo = true
            };

            context.Categorias.Add(categoriaCredenciais);
            context.Categorias.Add(categoriaTextos);

            var usuario = new Usuario
            {
                Login = "admin",
                NomeExibicao = "Administrador",
                PasswordHash = PasswordHasher.Hash("admin123"),
                Ativo = true,
                DataCriacao = DateTime.UtcNow
            };

            context.Usuarios.Add(usuario);
            context.SaveChanges();

            context.ItensPreenchimento.Add(new ItemPreenchimento
            {
                UsuarioId = usuario.Id,
                CategoriaId = categoriaCredenciais.Id,
                TituloExibicao = "PWD USUARIO TPB",
                LoginReferencia = @"tpb\palladino.11",
                TipoItem = TipoItemPreenchimento.TextoCurto,
                OrdemExibicao = 1,
                Ativo = true,
                Valor = string.Empty
            });

            context.ItensPreenchimento.Add(new ItemPreenchimento
            {
                UsuarioId = usuario.Id,
                CategoriaId = categoriaCredenciais.Id,
                TituloExibicao = "PWD USUARIO ADM1",
                LoginReferencia = @"tpb\palladino.11-adm1",
                TipoItem = TipoItemPreenchimento.TextoCurto,
                OrdemExibicao = 2,
                Ativo = true,
                Valor = string.Empty
            });

            context.ItensPreenchimento.Add(new ItemPreenchimento
            {
                UsuarioId = usuario.Id,
                CategoriaId = categoriaCredenciais.Id,
                TituloExibicao = "PWD USUARIO ADM2",
                LoginReferencia = @"tpb\palladino.11-adm2",
                TipoItem = TipoItemPreenchimento.TextoCurto,
                OrdemExibicao = 3,
                Ativo = true,
                Valor = string.Empty
            });

            context.ItensPreenchimento.Add(new ItemPreenchimento
            {
                UsuarioId = usuario.Id,
                CategoriaId = categoriaCredenciais.Id,
                TituloExibicao = "PWD USUARIO ITAU",
                LoginReferencia = @"tpitau\palladino.11",
                TipoItem = TipoItemPreenchimento.TextoCurto,
                OrdemExibicao = 4,
                Ativo = true,
                Valor = string.Empty
            });

            context.ItensPreenchimento.Add(new ItemPreenchimento
            {
                UsuarioId = usuario.Id,
                CategoriaId = categoriaCredenciais.Id,
                TituloExibicao = "PWD USUARIO UOL",
                LoginReferencia = @"uol\palladino.11.uol",
                TipoItem = TipoItemPreenchimento.TextoCurto,
                OrdemExibicao = 5,
                Ativo = true,
                Valor = string.Empty
            });

            context.ItensPreenchimento.Add(new ItemPreenchimento
            {
                UsuarioId = usuario.Id,
                CategoriaId = categoriaTextos.Id,
                TituloExibicao = "Texto padrao",
                LoginReferencia = "Uso livre",
                TipoItem = TipoItemPreenchimento.TextoLongo,
                OrdemExibicao = 1,
                Ativo = true,
                Valor = "Digite aqui um texto longo para preenchimento automatico.",
                Observacao = "Seed inicial"
            });

            context.SaveChanges();
        }

        public static void RestoreAdminAccess()
        {
            using (var context = new ApplicationDbContext())
            {
                EnsureSchema(context);

                var admin = context.Usuarios.FirstOrDefault(x => x.Login == "admin");
                if (admin == null)
                {
                    SeedDefaultData(context);
                    return;
                }

                admin.NomeExibicao = "Administrador";
                admin.PasswordHash = PasswordHasher.Hash("admin123");
                admin.Ativo = true;
                context.SaveChanges();
                EnsureEncodedItemValues();
            }
        }

        private static void EnsureAdminSeed(ApplicationDbContext context)
        {
            if (!context.Usuarios.Any())
            {
                SeedDefaultData(context);
                return;
            }

            var admin = context.Usuarios.FirstOrDefault(x => x.Login == "admin");
            if (admin == null)
            {
                admin = new Usuario
                {
                    Login = "admin",
                    NomeExibicao = "Administrador",
                    PasswordHash = PasswordHasher.Hash("admin123"),
                    Ativo = true,
                    DataCriacao = DateTime.UtcNow
                };
                context.Usuarios.Add(admin);
                context.SaveChanges();
            }
        }

        private static void EnsureEncodedItemValues()
        {
            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                return;
            }

            using (var connection = new SqliteConnection(AppDatabaseSettings.SqliteConnectionString))
            {
                connection.Open();
                SqliteFunctionRegistry.Register(connection);
                var pendentes = new System.Collections.Generic.List<Tuple<int, string>>();

                using (var select = connection.CreateCommand())
                {
                    select.CommandText = "SELECT Id, COALESCE(Valor, '') FROM AppItensPreenchimento;";

                    using (var reader = select.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var id = reader.GetInt32(0);
                            var valorAtual = reader.GetString(1);

                            if (SqliteFunctionRegistry.IsEncodedValue(valorAtual))
                            {
                                continue;
                            }

                            pendentes.Add(Tuple.Create(id, valorAtual));
                        }
                    }
                }

                foreach (var item in pendentes)
                {
                    using (var update = connection.CreateCommand())
                    {
                        update.CommandText = "UPDATE AppItensPreenchimento SET Valor = fncBase64_Encode($valor) WHERE Id = $id;";
                        update.Parameters.AddWithValue("$valor", item.Item2);
                        update.Parameters.AddWithValue("$id", item.Item1);
                        update.ExecuteNonQuery();
                    }
                }
            }
        }
    }
}
