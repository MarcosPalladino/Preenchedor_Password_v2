namespace TPPreenchedor.Data.Models
{
    public class ItemPreenchimento
    {
        public int Id { get; set; }
        public int UsuarioId { get; set; }
        public int? CategoriaId { get; set; }
        public string TituloExibicao { get; set; }
        public string LoginReferencia { get; set; }
        public TipoItemPreenchimento TipoItem { get; set; }
        public string Valor { get; set; }
        public int OrdemExibicao { get; set; }
        public bool Ativo { get; set; }
        public string Observacao { get; set; }
        public virtual Usuario Usuario { get; set; }
        public virtual ItemCategoria Categoria { get; set; }
    }
}
