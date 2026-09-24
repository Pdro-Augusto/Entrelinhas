using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

[QueryProperty(nameof(Isbn), "isbn")]
public partial class AddLivroPage : ContentPage
{
    private readonly AddLivroViewModel _viewModel;

    public AddLivroPage(AddLivroViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    public string? Isbn
    {
        get => _viewModel.Isbn;
        set => _viewModel.Isbn = value; // repassa para o ViewModel
    }

    private async void btSalvar_Clicked(object sender, EventArgs e)
    {
        if (_viewModel.NovoLivro == null) return;

        var resultado = await _viewModel.SalvarLivroAsync();

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
        await Shell.Current.GoToAsync("..");
    }
}