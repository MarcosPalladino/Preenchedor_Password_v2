using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using TPPreenchedor.Data.Models;

namespace TPPreenchedor.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<ItemCategoria> Categorias { get; set; }
        public DbSet<ItemPreenchimento> ItensPreenchimento { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (optionsBuilder.IsConfigured)
            {
                return;
            }

            var provider = AppDatabaseSettings.Provider.ToLowerInvariant();
            if (provider == "sqlite")
            {
                var connection = new SqliteConnection(AppDatabaseSettings.SqliteConnectionString);
                connection.Open();
                SqliteFunctionRegistry.Register(connection);
                optionsBuilder.UseSqlite(connection);
                return;
            }

            throw new System.NotSupportedException(
                "O provider configurado ainda nao esta habilitado. Use 'sqlite' por enquanto.");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Usuario>(entity =>
            {
                entity.ToTable("AppUsuarios");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Login).IsRequired().HasMaxLength(100);
                entity.Property(x => x.PasswordHash).IsRequired().HasMaxLength(512);
                entity.Property(x => x.NomeExibicao).IsRequired().HasMaxLength(150);
                entity.HasIndex(x => x.Login).IsUnique();
            });

            modelBuilder.Entity<ItemCategoria>(entity =>
            {
                entity.ToTable("AppCategorias");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Nome).IsRequired().HasMaxLength(100);
                entity.HasIndex(x => x.Nome).IsUnique();
            });

            modelBuilder.Entity<ItemPreenchimento>(entity =>
            {
                entity.ToTable("AppItensPreenchimento");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.TituloExibicao).IsRequired().HasMaxLength(150);
                entity.Property(x => x.LoginReferencia).HasMaxLength(150);
                entity.Property(x => x.Valor).HasDefaultValue(string.Empty);
                entity.Property(x => x.Observacao).HasMaxLength(300);

                entity.HasOne(x => x.Usuario)
                    .WithMany(x => x.Itens)
                    .HasForeignKey(x => x.UsuarioId)
                    .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(x => x.Categoria)
                    .WithMany(x => x.Itens)
                    .HasForeignKey(x => x.CategoriaId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
