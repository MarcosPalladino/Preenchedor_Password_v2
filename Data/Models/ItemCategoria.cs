using System.Collections.Generic;

namespace TPPreenchedor.Data.Models
{
    public class ItemCategoria
    {
        public int Id { get; set; }
        public string Nome { get; set; }
        public int OrdemExibicao { get; set; }
        public bool Ativo { get; set; }
        public virtual ICollection<ItemPreenchimento> Itens { get; set; } = new List<ItemPreenchimento>();
    }
}
