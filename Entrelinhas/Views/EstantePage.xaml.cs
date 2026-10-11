using Entrelinhas.Services;

namespace Entrelinhas.Views;

public partial class EstantePage : ContentPage
{
    private readonly DatabaseService _databaseService;
    private List<Livro> _cachedTodos = new();
    public EstantePage(DatabaseService databaseService)
	{
		InitializeComponent();
		_databaseService = databaseService;
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

    private void btAddNewBook_Clicked(object sender, EventArgs e)
    {

    }

    private void ApplyFilter()
    {
        var todos = _cachedTodos ?? new List<Livro>();
        List<Livro> livros;

        if (latestButtonClicked == btReading)
            livros = todos.Where(l => l.Status == StatusLeitura.Lendo).ToList();
        else if (latestButtonClicked == btRead)
            livros = todos.Where(l => l.Status == StatusLeitura.Lido).ToList();
        else
            livros = todos;

        BookList.ItemsSource = livros;
        EmptyState.IsVisible = livros.Count() == 0;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _cachedTodos = await _databaseService.ObterTodosAsync() ?? new List<Livro>();

        // Ensure a default filter is selected
        if (latestButtonClicked == null)
        {
            HighlightButton(btAllBooks);
        }

        ApplyFilter();
  

        //if (todosLivros != null)
        //{
        //    BookList.ItemsSource = todosLivros;
        //    if (todosLivros.Count > 0)
        //    {
        //        EmptyState.IsVisible = false;
        //    }
        //    else
        //    {
        //        EmptyState.IsVisible = true;
        //    }
        //}
        //else
        //{
        //    EmptyState.IsVisible = true;
        //}
    }

    private async void OnLivroTocado(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Livro livro) return;

        await Shell.Current.GoToAsync($"detalhelivro?livroId={livro.Id}");
    }
}