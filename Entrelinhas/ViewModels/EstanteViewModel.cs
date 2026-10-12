using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class EstanteViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;
    public List<Livro> CachedTodos = new();
    public EstanteViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public async Task LoadLivrosAsync()
    {
        IsLoading = true;
        try
        {
            CachedTodos = await _databaseService.ObterTodosAsync() ?? new List<Livro>();
        }
        catch (Exception ex)
        {
            // Handle exceptions (e.g., log them)
        }
        finally
        {
            IsLoading = false;
        }
    }
}
