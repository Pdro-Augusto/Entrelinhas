namespace Entrelinhas.Views;

[QueryProperty(nameof(Isbn), "isbn")]

public partial class AddLivroPage : ContentPage
{
    private const string API_KEY = "AIzaSyAzAMQaYmO_4XMmexamwO3syWAIj2TxxoY";

    private string? _isbn;
    public string? Isbn
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

    Livro NovoLivro = new Livro();

    public AddLivroPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private async Task BuscarLivroAsync(string isbn)
    {
        try
        {
            using var http = new HttpClient();
            var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}&key={API_KEY}";
            var json = await http.GetStringAsync(url);
            var data = System.Text.Json.JsonDocument.Parse(json);


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
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private void btCancelar_Clicked(object sender, EventArgs e)
    {

    }

    private void btSalvar_Clicked(object sender, EventArgs e)
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
    }
}