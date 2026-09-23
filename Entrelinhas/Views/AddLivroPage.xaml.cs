using Entrelinhas.Services;

namespace Entrelinhas.Views;

[QueryProperty(nameof(Isbn), "isbn")]

public partial class AddLivroPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly DatabaseService _databaseService;
    private Livro? _novoLivro;
    private string? _isbn;
    public string? Isbn
    {
        get => _isbn;
        set
        {
            _isbn = value;
            if (!string.IsNullOrEmpty(value))
                _ = BuscarLivroAsync(value);
        }
    }

    public AddLivroPage(DatabaseService databaseService, ApiService apiService)
	{
		InitializeComponent();
        _databaseService = databaseService;
        _apiService = apiService;
    }

    private async Task BuscarLivroAsync(string isbn)
    {
        var livro = await _apiService.BuscarPorIsbnAsync(isbn);

        if (livro == null)
        {
            await DisplayAlertAsync("Aviso", "Livro não encontrado.", "OK");
            return;
        }

        _novoLivro = livro;

        MainThread.BeginInvokeOnMainThread(() =>
        {
            lbTitulo.Text = livro.Titulo;
            lbAutor.Text = livro.Autor;
            lbEditora.Text = livro.Editora;
            lbAno.Text = livro.Ano?.ToString();
            lbPaginas.Text = livro.Paginas?.ToString();
            lbSinopse.Text = livro.Sinopse;

            if (!string.IsNullOrEmpty(livro.CapaUrl))
                imgCapa.Source = ImageSource.FromUri(new Uri(livro.CapaUrl.Replace("http://", "https://")));
        });
    }

    private async void btSalvar_Clicked(object sender, EventArgs e)
    {
        if (_novoLivro == null) return;

        await _databaseService.InserirLivroAsync(_novoLivro);
        await Shell.Current.GoToAsync("..");
    }

    private async void btCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}