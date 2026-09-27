using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class AddLivroViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;
    public event Action? LivroNaoEncontrado;

    public AddLivroViewModel(DatabaseService databaseService, ApiService apiService)
    {
        _databaseService = databaseService;
        _apiService = apiService;
    }

    private string? _isbn;
    public string? Isbn // Recebe o ISBN da página e dispara a busca
    {
        get => _isbn;
        set
        {
            if (SetProperty(ref _isbn, value))
            {
                if (!string.IsNullOrEmpty(value))
                    _ = BuscarLivroAsync(value);
            }
        }
    }

    private Livro? _novoLivro;
    public Livro? NovoLivro // Propriedade que representa o livro a ser adicionado
    {
        get => _novoLivro;
        set => SetProperty(ref _novoLivro, value);
    }

    public async Task BuscarLivroAsync(string isbn)
    {
        IsLoading = true;
        try
        {
            NovoLivro = await _apiService.BuscarPorIsbnAsync(isbn);
            if (NovoLivro == null)
                LivroNaoEncontrado?.Invoke();
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<int> SalvarLivroAsync(int? statusLeitura)
    {
        if (NovoLivro == null) return 0;

        IsLoading = true;
        try
        {
            if (statusLeitura == null)
                statusLeitura = 0; // Default para QueroLer
            NovoLivro.Status = (StatusLeitura)statusLeitura;
            return await _databaseService.InserirLivroAsync(NovoLivro);
        }
        finally
        {
            IsLoading = false;
        }
    }
}