using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class AddLivroViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    private readonly ApiService _apiService;

    public AddLivroViewModel(DatabaseService databaseService, ApiService apiService)
    {
        _databaseService = databaseService;
        _apiService = apiService;
    }

    private string? _isbn;
    public string? Isbn
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
    public Livro? NovoLivro
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
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task<int> SalvarLivroAsync()
    {
        if (NovoLivro == null) return 0;

        IsLoading = true;
        try
        {
            return await _databaseService.InserirLivroAsync(NovoLivro);
        }
        finally
        {
            IsLoading = false;
        }
    }
}