using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TPPreenchedor.Data.Models;

namespace TPPreenchedor.Data
{
    public class ItemPreenchimentoRepository
    {
        public List<ItemPreenchimento> ObterItensPorUsuario(int usuarioId, TipoItemPreenchimento tipo)
        {
            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                return ObterItensPorUsuarioEf(usuarioId, tipo);
            }

            using (var connection = CreateSqliteConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT
    Id,
    UsuarioId,
    CategoriaId,
    TituloExibicao,
    LoginReferencia,
    TipoItem,
    fncBase64_Decode(COALESCE(Valor, '')) AS Valor,
    OrdemExibicao,
    Ativo,
    Observacao
FROM AppItensPreenchimento
WHERE UsuarioId = $usuarioId
  AND TipoItem = $tipo
  AND Ativo = 1
ORDER BY OrdemExibicao, TituloExibicao;";
                command.Parameters.AddWithValue("$usuarioId", usuarioId);
                command.Parameters.AddWithValue("$tipo", (int)tipo);

                var itens = new List<ItemPreenchimento>();
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        itens.Add(MapItem(reader));
                    }
                }

                return itens;
            }
        }

        public ItemPreenchimento Adicionar(ItemPreenchimento item)
        {
            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                using (var context = new ApplicationDbContext())
                {
                    context.ItensPreenchimento.Add(item);
                    context.SaveChanges();
                    return item;
                }
            }

            using (var connection = CreateSqliteConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
INSERT INTO AppItensPreenchimento
(
    UsuarioId,
    CategoriaId,
    TituloExibicao,
    LoginReferencia,
    TipoItem,
    Valor,
    OrdemExibicao,
    Ativo,
    Observacao
)
VALUES
(
    $usuarioId,
    $categoriaId,
    $titulo,
    $loginReferencia,
    $tipoItem,
    fncBase64_Encode($valor),
    $ordemExibicao,
    $ativo,
    $observacao
);
SELECT last_insert_rowid();";

                BindParameters(command, item);
                item.Id = Convert.ToInt32(command.ExecuteScalar());
                return item;
            }
        }

        public void Atualizar(ItemPreenchimento item)
        {
            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                using (var context = new ApplicationDbContext())
                {
                    context.ItensPreenchimento.Update(item);
                    context.SaveChanges();
                }

                return;
            }

            using (var connection = CreateSqliteConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
UPDATE AppItensPreenchimento
SET
    UsuarioId = $usuarioId,
    CategoriaId = $categoriaId,
    TituloExibicao = $titulo,
    LoginReferencia = $loginReferencia,
    TipoItem = $tipoItem,
    Valor = fncBase64_Encode($valor),
    OrdemExibicao = $ordemExibicao,
    Ativo = $ativo,
    Observacao = $observacao
WHERE Id = $id;";

                BindParameters(command, item);
                command.Parameters.AddWithValue("$id", item.Id);
                command.ExecuteNonQuery();
            }
        }

        public void Remover(int id)
        {
            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                using (var context = new ApplicationDbContext())
                {
                    var item = context.ItensPreenchimento.FirstOrDefault(x => x.Id == id);
                    if (item == null)
                    {
                        return;
                    }

                    context.ItensPreenchimento.Remove(item);
                    context.SaveChanges();
                }

                return;
            }

            using (var connection = CreateSqliteConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM AppItensPreenchimento WHERE Id = $id;";
                command.Parameters.AddWithValue("$id", id);
                command.ExecuteNonQuery();
            }
        }

        public int ObterProximaOrdem(int usuarioId, TipoItemPreenchimento tipo)
        {
            if (AppDatabaseSettings.Provider.ToLowerInvariant() != "sqlite")
            {
                using (var context = new ApplicationDbContext())
                {
                    var ultimo = context.ItensPreenchimento
                        .Where(x => x.UsuarioId == usuarioId && x.TipoItem == tipo)
                        .OrderByDescending(x => x.OrdemExibicao)
                        .FirstOrDefault();

                    return ultimo == null ? 1 : ultimo.OrdemExibicao + 1;
                }
            }

            using (var connection = CreateSqliteConnection())
            using (var command = connection.CreateCommand())
            {
                command.CommandText = @"
SELECT COALESCE(MAX(OrdemExibicao), 0)
FROM AppItensPreenchimento
WHERE UsuarioId = $usuarioId
  AND TipoItem = $tipo;";
                command.Parameters.AddWithValue("$usuarioId", usuarioId);
                command.Parameters.AddWithValue("$tipo", (int)tipo);

                return Convert.ToInt32(command.ExecuteScalar()) + 1;
            }
        }

        private static List<ItemPreenchimento> ObterItensPorUsuarioEf(int usuarioId, TipoItemPreenchimento tipo)
        {
            using (var context = new ApplicationDbContext())
            {
                return context.ItensPreenchimento
                    .Include(x => x.Categoria)
                    .Where(x => x.UsuarioId == usuarioId && x.TipoItem == tipo && x.Ativo)
                    .OrderBy(x => x.OrdemExibicao)
                    .ThenBy(x => x.TituloExibicao)
                    .ToList();
            }
        }

        private static SqliteConnection CreateSqliteConnection()
        {
            var connection = new SqliteConnection(AppDatabaseSettings.SqliteConnectionString);
            connection.Open();
            SqliteFunctionRegistry.Register(connection);
            return connection;
        }

        private static ItemPreenchimento MapItem(IDataRecord reader)
        {
            return new ItemPreenchimento
            {
                Id = reader.GetInt32(0),
                UsuarioId = reader.GetInt32(1),
                CategoriaId = reader.IsDBNull(2) ? (int?)null : reader.GetInt32(2),
                TituloExibicao = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                LoginReferencia = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                TipoItem = (TipoItemPreenchimento)reader.GetInt32(5),
                Valor = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                OrdemExibicao = reader.GetInt32(7),
                Ativo = reader.GetBoolean(8),
                Observacao = reader.IsDBNull(9) ? string.Empty : reader.GetString(9)
            };
        }

        private static void BindParameters(SqliteCommand command, ItemPreenchimento item)
        {
            command.Parameters.AddWithValue("$usuarioId", item.UsuarioId);
            command.Parameters.AddWithValue("$categoriaId", (object)item.CategoriaId ?? DBNull.Value);
            command.Parameters.AddWithValue("$titulo", item.TituloExibicao ?? string.Empty);
            command.Parameters.AddWithValue("$loginReferencia", (object)item.LoginReferencia ?? DBNull.Value);
            command.Parameters.AddWithValue("$tipoItem", (int)item.TipoItem);
            command.Parameters.AddWithValue("$valor", item.Valor ?? string.Empty);
            command.Parameters.AddWithValue("$ordemExibicao", item.OrdemExibicao);
            command.Parameters.AddWithValue("$ativo", item.Ativo ? 1 : 0);
            command.Parameters.AddWithValue("$observacao", (object)item.Observacao ?? DBNull.Value);
        }
    }
}
