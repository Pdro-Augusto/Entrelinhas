using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

[QueryProperty(nameof(Isbn), "isbn")] // Recebe o ISBN pela query string
public partial class AddLivroPage : ContentPage
{
    private readonly AddLivroViewModel _viewModel;

    private int? _statusLeitura;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public AddLivroPage(AddLivroViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public AddLivroPage(AddLivroViewModel viewModel, Livro livro) : this(viewModel)
    {
        _viewModel.NovoLivro = livro;
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
            var toast = Toast.Make("Livro salvo!", ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
            await Shell.Current.GoToAsync("..");
        }
        else if (resultado == -1)
        {
            var toast = Toast.Make("Livro já existe na estante.", ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
        }
        else
        {
            var toast = Toast.Make("Não foi possível salvar o livro.", ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
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

    private async void OnLivroNaoEncontrado()
    {
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            var toast = Toast.Make("Livro não encontrado.", ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
            await Shell.Current.GoToAsync("..");
        });
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.LivroNaoEncontrado -= OnLivroNaoEncontrado;
        _viewModel.LivroNaoEncontrado += OnLivroNaoEncontrado;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.LivroNaoEncontrado -= OnLivroNaoEncontrado;
    }
}