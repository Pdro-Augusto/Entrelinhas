using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

public partial class PesquisaPage : ContentPage
{
	private readonly PesquisaViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public PesquisaPage(PesquisaViewModel viewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _serviceProvider = serviceProvider;
        BindingContext = _viewModel;
    }

    private async void btSearch_Clicked(object sender, EventArgs e)
    {
        await searchBook.HideKeyboardAsync(CancellationToken.None);

        var palavraChave = searchBook.Text;

		if (string.IsNullOrWhiteSpace(palavraChave))
		{
            var toast = Toast.Make("Digite um valor para pesquisar.", ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
            return;
		}
		else 
		{
			var resultado = await _viewModel.PesquisarLivrosAsync(palavraChave);

			if (resultado == -1)
			{
                var toast = Toast.Make("Nenhum livro encontrado.", ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);
                emptyState.IsVisible = true;
                lbNaoEncontrou.IsVisible = false;

            }
            else if (resultado == -2)
            {
                var toast = Toast.Make("Digite um valor para pesquisar.", ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);
                emptyState.IsVisible = false;
                lbNaoEncontrou.IsVisible = true;
            }
            else if (resultado == 1)
            {
                BookList.ItemsSource = _viewModel.Livros;
                emptyState.IsVisible = false;
                lbNaoEncontrou.IsVisible = true;
            }
        }
    }

    private async void OnLivroTocado(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Livro livro) return;

        try
        {
            _viewModel.IsLoading = true;

            // 1. Pede ao MAUI para criar a AddLivroPage (ele já injeta as ViewModels sozinho)
            var addLivroPage = _serviceProvider.GetRequiredService<AddLivroPage>();

            // 2. Passa o livro para a página recém-criada
            addLivroPage.InicializarComLivro(livro);

            // 3. Navega
            await Shell.Current.Navigation.PushAsync(addLivroPage);
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }

    private async void btCadastrarManualmente_Clicked(object sender, EventArgs e)
    {
        try
        {
            _viewModel.IsLoading = true;
            var addManualPage = _serviceProvider.GetRequiredService<AddManualPage>();
            await Shell.Current.Navigation.PushAsync(addManualPage);
        }
        finally
        {
            _viewModel.IsLoading = false;
        }
    }
}