using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using Entrelinhas.ViewModels;

namespace Entrelinhas.Views;

public partial class AddManualPage : ContentPage
{
    private readonly AddManualViewModel _viewModel;
    private int? _statusLeitura;
    private bool _modoEdicao;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    public AddManualPage(AddManualViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;

        btWantRead_Clicked(btWantRead, EventArgs.Empty);
    }

    // Método para receber o livro da página anterior
    public void Inicializar(Livro livro)
    {
        _viewModel.Livro = livro;
        _modoEdicao = livro.Id != 0;

        if (_modoEdicao)
        {
            btListaDesejo.IsVisible = false;
            btSalvar.Text = "Salvar alterações";

            _statusLeitura = (int)livro.Status;
            var botao = _statusLeitura switch
            {
                1 => btReading,
                2 => btRead,
                _ => btWantRead
            };
            
            if (botao == btWantRead) btWantRead_Clicked(botao, EventArgs.Empty);
            else if (botao == btReading) btReading_Clicked(botao, EventArgs.Empty);
            else btRead_Clicked(botao, EventArgs.Empty);
        }
    }

    private async void btCancelar_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    private async void btSalvar_Clicked(object sender, EventArgs e) => await SalvarAsync(false);
    private async void btListaDesejo_Clicked(object sender, EventArgs e) => await SalvarAsync(true);

    private async Task SalvarAsync(bool listaDesejo)      // era: private async void btSalvar_Clicked(...)
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

            _viewModel.Livro.Ano = int.TryParse(txAno.Text, out var ano) ? ano : null;
            _viewModel.Livro.Paginas = int.TryParse(txPaginas.Text, out var paginas) ? paginas : null;

            var resultado = await _viewModel.SalvarLivroAsync(
            listaDesejo ? 0 : (_statusLeitura ?? 0),
            listaDesejo ? true : (bool?)null);

            if (resultado > 0)
            {
                var toast = Toast.Make(
                    listaDesejo ? "Adicionado à lista de desejos!" : "Livro salvo!",   // mudou
                    ToastDuration.Short, 14);
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
            await DisplayAlertAsync("Erro", $"Ocorreu um erro ao salvar o livro: {ex.Message}", "OK");
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