using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

[QueryProperty(nameof(Isbn), "isbn")]
public partial class AddLivroPage : ContentPage
{
    private readonly AddLivroViewModel _viewModel;

    public AddLivroPage(AddLivroViewModel viewModel)
    {
        get => _isbn;
        set
        {
            if (_isbn == value) return;
            _isbn = value;
            if (!string.IsNullOrWhiteSpace(_isbn))
            {
                
                _ = BuscarLivroAsync(_isbn);
            }
        }
    }
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    Livro NovoLivro = new Livro();

    public AddLivroPage()
    {
        InitializeComponent();
        BindingContext = this;
    public string? Isbn
    {
        get => _viewModel.Isbn;
        set => _viewModel.Isbn = value; // repassa para o ViewModel
    }

    private async void btSalvar_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.NovoLivro == null) return;

        var resultado = await _viewModel.SalvarLivroAsync();

            var items = data.RootElement.GetProperty("items");
            if (items.GetArrayLength() == 0)
            {
                await DisplayAlertAsync("Aviso", "Livro não encontrado.", "OK");
                return;
            }

            var info = items[0].GetProperty("volumeInfo");

            // Preenche os campos da tela (na thread principal)
            MainThread.BeginInvokeOnMainThread(() =>
            {
                NovoLivro.Isbn = info.TryGetProperty("isbn", out var i) ? i.GetString() : null;
                NovoLivro.Titulo = info.TryGetProperty("title", out var t) ? t.GetString() : null;
                NovoLivro.Autor = info.TryGetProperty("authors", out var a) ? a[0].GetString() : null;
                NovoLivro.Editora = info.TryGetProperty("publisher", out var p) ? p.GetString() : null;
                NovoLivro.Ano = info.TryGetProperty("publishedDate", out var d) ? int.Parse(d.GetString().Substring(0, 4)) : null;
                NovoLivro.Paginas = info.TryGetProperty("pageCount", out var pg) ? pg.GetInt32() : null;
                NovoLivro.Sinopse = info.TryGetProperty("description", out var s) ? s.GetString() : null;
                NovoLivro.Genero = info.TryGetProperty("categories", out var c) ? c[0].GetString() : null;
                NovoLivro.CapaUrl = info.TryGetProperty("imageLinks", out var img)
                    && img.TryGetProperty("thumbnail", out var th)
                    ? th.GetString()?.Replace("http://", "https://")
                    : null;

                // Atualiza os Labels/Entries da tela
                imgCapa.Source = NovoLivro.CapaUrl;
                lbTitulo.Text = NovoLivro.Titulo;
                lbAutor.Text = NovoLivro.Autor;
                lbEditora.Text = NovoLivro.Editora;
                lbAno.Text = $"{NovoLivro.Ano?.ToString()}";
                lbPaginas.Text = $"{NovoLivro.Paginas?.ToString()}";
                lbSinopse.Text = NovoLivro.Sinopse;
            });
        if (resultado > 0)
        {
            await DisplayAlertAsync("Sucesso", "Livro salvo!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        else if (resultado == -1)
        {
            await DisplayAlertAsync("Aviso", "Livro já existe na estante.", "OK");
        }
        else
        {
            await DisplayAlertAsync("Erro", "Não foi possível salvar o livro.", "OK");
        }
    }

    private async void btCancelar_Clicked(object sender, EventArgs e)
    {

    }

    private Button latestButtonClicked = null;

    private void ResetLastButton()
    {
        if (latestButtonClicked != null)
        {
            latestButtonClicked.BackgroundColor = Color.FromArgb("#10D85A30");
            latestButtonClicked.TextColor = Color.FromArgb("#4A1B0C");
        }
    }

    private void btWantRead_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLer = (Button)sender;
        btLer.BackgroundColor = Color.FromArgb("#D85A30");
        btLer.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLer;
    }

    private void btReading_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLendo = (Button)sender;
        btLendo.BackgroundColor = Color.FromArgb("#D85A30");
        btLendo.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLendo;
    }

    private void btRead_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLido = (Button)sender;
        btLido.BackgroundColor = Color.FromArgb("#D85A30");
        btLido.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLido;
        await Shell.Current.GoToAsync("..");
    }
}