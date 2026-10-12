using Entrelinhas.Services;
using System.Collections.ObjectModel;

namespace Entrelinhas.ViewModels;

public partial class PesquisaViewModel : BaseViewModel
{
    private readonly ApiService _apiService;
    public ObservableCollection<Livro>? Livros = new();
    public PesquisaViewModel(ApiService apiService)
    {
        _apiService = apiService;
    }

    public async Task<int> PesquisarLivrosAsync(string palavraChave)
    {
        IsLoading = true;
        try
        {
            if (string.IsNullOrWhiteSpace(palavraChave))
                return -2;

            var resultados = await _apiService.BuscarPorPalavraChaveAsync(palavraChave);

            if (resultados == null || !resultados.Any())
            {
                Livros.Clear();
                return -1;
            }

            Livros.Clear();
            foreach (var livro in resultados)
                Livros.Add(livro);

            return 1;
        }
        finally
        {
            IsLoading = false;
        }
    }


}
