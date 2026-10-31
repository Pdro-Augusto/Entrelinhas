using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

public partial class AddManualPage : ContentPage
{
    private readonly AddManualViewModel _viewModel;
    private int? _statusLeitura;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public AddManualPage(AddManualViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    // Método para receber o livro
    public void Inicializar(Livro livro)
    {
        _viewModel.Livro = livro;
    }

    private async void btCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void btSalvar_Clicked(object sender, EventArgs e)
    {
        try
        {
            if (string.IsNullOrEmpty(txTitulo.Text))
            {
                var toast = Toast.Make("Título é obrigatório.", ToastDuration.Short, 14);
                await toast.Show(cancellationTokenSource.Token);
                return;
            }

            _viewModel.Livro ??= new Livro();

            //_viewModel.Livro.CapaUrl = imgCapa.Source?.ToString();
            _viewModel.Livro.Ano = int.TryParse(txAno.Text, out var ano) ? ano : null;
            _viewModel.Livro.Paginas = int.TryParse(txPaginas.Text, out var paginas) ? paginas : null;

            // Coloque um breakpoint aqui e confira os valores de livro.
            var resultado = await _viewModel.SalvarLivroAsync(_statusLeitura ?? 0);

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
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine(ex.ToString());

            var toast = Toast.Make(
                $"Erro ao salvar: {ex.Message}",
                ToastDuration.Long,
                14);

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

    private async void OnImageTapped(object sender, TappedEventArgs e)
    {
        try
        {
            // 1. Usa o novo método (PickPhotosAsync) em vez do antigo
            var fotos = await MediaPicker.Default.PickPhotosAsync(new MediaPickerOptions
            {
                SelectionLimit = 1, // Garante que só uma foto seja selecionada
                Title = "Escolha a capa do livro"
            });

            // 2. Verifica se o usuário não cancelou (lista vazia)
            if (fotos == null || fotos.Count == 0)
                return;

            var resultado = fotos.First(); // Pega a única foto selecionada

            // 3. O resto do código continua igual
            var nomeArquivo = $"{Guid.NewGuid()}.jpg";
            var caminhoLocal = Path.Combine(FileSystem.AppDataDirectory, nomeArquivo);

            using (var streamOrigem = await resultado.OpenReadAsync())
            using (var streamDestino = File.Create(caminhoLocal))
            {
                await streamOrigem.CopyToAsync(streamDestino);
            }

            _viewModel.Livro ??= new Livro();

            imgCapa.Source = ImageSource.FromFile(caminhoLocal);
            _viewModel.Livro.CapaUrl = caminhoLocal;
        }
        catch (Exception ex)
        {
            var toast = Toast.Make("Erro ao salvar a imagem.", ToastDuration.Short, 14);
            await toast.Show(cancellationTokenSource.Token);
        }
    }
}