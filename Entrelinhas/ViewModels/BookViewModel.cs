using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class BookViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    public BookViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    private int? _livroId;
    public int? LivroId
    {
        get => _livroId;
        set => SetProperty(ref _livroId, value);
    }

    private Livro? _livro;
    public Livro? Livro // Propriedade que representa o livro encontrado
    {
        get => _livro;
        set => SetProperty(ref _livro, value);
    }

    public async Task BuscarLivroAsync()
    {
        if (!LivroId.HasValue) return;
        IsLoading = true;
        try
        {
            Livro = await _databaseService.ObterPorIdAsync(LivroId.Value);
        }
        finally
        {
            IsLoading = false;
        }
    }
}
