using Entrelinhas.ViewModels;
using Entrelinhas.Services;
namespace Entrelinhas.Views;

public partial class EstantePage : ContentPage
{
    private readonly DatabaseService _db;

    private bool _mostrandoDesejos;

    private static Color cor(string chave, string fallbakc) => Application.Current?.Resources.TryGetValue(chave, out var v) == true && v is Color c
        ? c : Color.FromArgb(fallbakc);
    private readonly EstanteViewModel _viewModel;
    public EstantePage(EstanteViewModel viewModel, DatabaseService db)
	{
		InitializeComponent();
		_viewModel = viewModel;
        BindingContext = _viewModel;
        _db = db;
    }

    private Button? latestButtonClicked = null;

    private void ResetLastButton()
    {
        if(latestButtonClicked != null)
        {
            latestButtonClicked.BackgroundColor = Color.FromArgb("#10D85A30");
            latestButtonClicked.TextColor = Color.FromArgb("#4A1B0C");
        }
    }

    private void HighlightButton(Button btn)
    {
        ResetLastButton();
        btn.BackgroundColor = Color.FromArgb("#D85A30");
        btn.TextColor = Color.FromArgb("#EDEADE");
        latestButtonClicked = btn;
    }

    private void btAllBooks_Clicked(object sender, EventArgs e)
    {
        HighlightButton((Button)sender);
        ApplyFilter();
    }

    private void btReading_Clicked(object sender, EventArgs e)
    {
        HighlightButton((Button)sender);
        ApplyFilter();
    }

    private void btRead_Clicked(object sender, EventArgs e)
    {
        HighlightButton((Button)sender);
        ApplyFilter();
    }

    private void btFavorites_Clicked(object sender, EventArgs e)
    {
        HighlightButton((Button)sender);
        ApplyFilter();
    }

    private async void btAddNewBook_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("//scanner");
    }

    private void ApplyFilter()
    {
        var todos = _viewModel.CachedTodos ?? new List<Livro>();
        List<Livro> livros;

        if (latestButtonClicked == btReading)
            livros = todos.Where(l => l.Status == StatusLeitura.Lendo).ToList();
        else if (latestButtonClicked == btRead)
            livros = todos.Where(l => l.Status == StatusLeitura.Lido).ToList();
        else if (latestButtonClicked == btFavorites)
            livros = todos.Where(l => l.Favorito).ToList();
        else
            livros = todos;

        BookList.ItemsSource = livros;
        EmptyState.IsVisible = livros.Count() == 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        await _viewModel.LoadLivrosAsync();

        // Ensure a default filter is selected
        if (latestButtonClicked == null)
        {
            HighlightButton(btAllBooks);
        }

        if (_mostrandoDesejos)
        {
            await CarregarDesejosAsync();
            return;
        }

        ApplyFilter();
    }

    private async void OnLivroTocado(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Livro livro) return;
            await Shell.Current.GoToAsync($"detalhelivro?livroId={livro.Id}");
    }

    private async void btListaDesejo_Clicked(object sender, EventArgs e)
    {
        _mostrandoDesejos = !_mostrandoDesejos;
        AtualizarModo();

        if (_mostrandoDesejos)
            await CarregarDesejosAsync();
        else
            btAllBooks_Clicked(btAllBooks, EventArgs.Empty);
    }

    private async Task CarregarDesejosAsync()
    {
        var livros = await _db.ObterListaDesejoAsync();

        BookList.ItemsSource = livros;
        BookList.IsVisible = livros.Count > 0;
        EmptyState.IsVisible = livros.Count == 0;
    }

    private void AtualizarModo()
    {
        bool desejos = _mostrandoDesejos;

        btAllBooks.IsVisible = !desejos;
        btReading.IsVisible = !desejos;
        btRead.IsVisible = !desejos;
        btFavorites.IsVisible = !desejos;

        lbModoDesejos.IsVisible = desejos;

        btListaDesejo.Margin = desejos ? new Thickness(0)
            : new Thickness(33, 0, 0, 0);
    }
}