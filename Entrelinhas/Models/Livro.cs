using SQLite;

public class Livro
{
    // Identificação
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    // Dados da API
    [Unique]
    public string? Isbn { get; set; }

    public string? Titulo { get; set; }
    public string? Autor { get; set; }
    public string? Editora { get; set; }
    public int? Ano { get; set; }
    public int? Paginas { get; set; }
    public string? Sinopse { get; set; }
    public string? Genero { get; set; }
    public string? CapaUrl { get; set; }

    // Dados do usuário
    public StatusLeitura Status { get; set; } = StatusLeitura.QueroLer;
    public int PaginaAtual { get; set; } = 0;
    public int Avaliacao { get; set; } = 0;
    public bool Favorito { get; set; } = false;
    public bool ListaDesejo { get; set; } = false;
    public string? Resumo { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.Now;
}

public enum StatusLeitura
{
    QueroLer,
    Lendo,
    Finalizado
}