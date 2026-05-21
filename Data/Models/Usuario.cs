using System;
using System.Collections.Generic;

namespace TPPreenchedor.Data.Models
{
    public class Usuario
    {
        public int Id { get; set; }
        public string Login { get; set; }
        public string PasswordHash { get; set; }
        public string NomeExibicao { get; set; }
        public bool Ativo { get; set; }
        public DateTime DataCriacao { get; set; }
        public DateTime? UltimoAcesso { get; set; }
        public virtual ICollection<ItemPreenchimento> Itens { get; set; } = new List<ItemPreenchimento>();
    }
}
