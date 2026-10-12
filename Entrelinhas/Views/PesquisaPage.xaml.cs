using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

public partial class PesquisaPage : ContentPage
{
	private readonly PesquisaViewModel _viewModel;
    CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public PesquisaPage(PesquisaViewModel viewModel)
	{
		InitializeComponent();
		_viewModel = viewModel;
		BindingContext = _viewModel;
    }

    private async void btSearch_Clicked(object sender, EventArgs e)
    {
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
            }
            else if (resultado == -2)
            {
                var toast = Toast.Make("Digite um valor para pesquisar.", ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);
                emptyState.IsVisible = false;
            }
            else if (resultado == 1)
            {
                BookList.ItemsSource = _viewModel.Livros;
                emptyState.IsVisible = false;
            }
        }
    }

    private async void OnLivroTocado(object? sender, TappedEventArgs e)
    {
        //if ((sender as BindableObject)?.BindingContext is not Livro livro) return;

        //await Shell.Current.GoToAsync($"detalhelivro?livroId={livro.Id}");
    }

}