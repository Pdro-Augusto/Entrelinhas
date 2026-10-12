using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

[QueryProperty(nameof(LivroId), "livroId")]

public partial class BookPage : ContentPage
{
    private readonly BookViewModel _viewModel;
    

    public BookPage(BookViewModel bookViewModel)
	{
		InitializeComponent();
        _viewModel = bookViewModel;
        BindingContext = _viewModel;
    }

    public int? LivroId
    {
        get => _viewModel.LivroId;
        set => _viewModel.LivroId = value; // Repassa o ID do livro para o ViewModel
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_viewModel.LivroId.HasValue)
        {
            await _viewModel.BuscarLivroAsync();
        }
    }

    private void OnVoltarClicado(object sender, TappedEventArgs e)
    {

    }
    private void OnMenuClicado(object sender, TappedEventArgs e)
    {

    }
    private void OnAbaProgressoClicado(object sender, TappedEventArgs e)
    {

    }
    private void OnAbaAnotacoesClicado(object sender, TappedEventArgs e)
    {

    }
    private void OnAbaSobreClicado(object sender, TappedEventArgs e)
    {

    }
    private void OnStatusTocado(object sender, TappedEventArgs e)
    {

    }
    private void OnAtualizarProgressoClicado(object sender, EventArgs e)
    {

    }

    private void OnSalvarAnotacaoClicado(object sender, EventArgs e)
    {

    }

    private void OnExcluirAnotacaoTocado(object sender, TappedEventArgs e)
    {

    }
}