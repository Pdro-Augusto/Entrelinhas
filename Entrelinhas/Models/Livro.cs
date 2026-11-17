using SQLite;
namespace Entrelinhas;

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
    public int? Paginas { get; set; } = 0;
    public string? Sinopse { get; set; }
    public string? Genero { get; set; }
    public string? CapaUrl { get; set; }

    // Dados do usuário
    public StatusLeitura Status { get; set; } = StatusLeitura.QueroLer;
    public int PaginaAtual { get; set; } = 0;

    public int PaginasLidas { get; set; }

    public double Avaliacao { get; set; } = 0;
    public bool Favorito { get; set; } = false;
    public bool ListaDesejo { get; set; } = false;
    public Livro Clone() => (Livro)MemberwiseClone();
    public string? Resumo { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public DateTime? DataConclusao {  get; set; }
}

public enum StatusLeitura
{
    QueroLer,
    Lendo,
    Lido
}

public class RegistroLeitura
{
    [PrimaryKey, AutoIncrement]
    public int Id { get; set; }

    [Indexed]
    public int LivroId { get; set; }

    public DateTime Data { get; set; }
    public int Paginas { get; set; }
}