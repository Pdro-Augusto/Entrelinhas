using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

[QueryProperty(nameof(Isbn), "isbn")] // Recebe o ISBN pela query string

public partial class AddLivroPage : ContentPage
{
    private readonly AddLivroViewModel _viewModel;

    private int? _statusLeitura;
    public AddLivroPage(AddLivroViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        _viewModel.LivroNaoEncontrado += async () =>
        {
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                await DisplayAlertAsync("Aviso", "Livro não encontrado.", "OK");
                await Shell.Current.GoToAsync("..");
            });
        };
    }

    public string? Isbn
    {
        get => _viewModel.Isbn;
        set => _viewModel.Isbn = value; // Repassa o ISBN para o ViewModel
    }

    private async void btCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void btSalvar_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.NovoLivro == null) return;

        var resultado = await _viewModel.SalvarLivroAsync(_statusLeitura);

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

    private Button? latestButtonClicked = null;

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
        _statusLeitura = 0;
    }

    private void btReading_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLendo = (Button)sender;
        btLendo.BackgroundColor = Color.FromArgb("#D85A30");
        btLendo.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLendo;
        _statusLeitura = 1;
    }

    private void btRead_Clicked(object sender, EventArgs e)
    {
        ResetLastButton();

        Button btLido = (Button)sender;
        btLido.BackgroundColor = Color.FromArgb("#D85A30");
        btLido.TextColor = Color.FromArgb("#EDEADE");

        latestButtonClicked = btLido;
        _statusLeitura = 2;
    }
}