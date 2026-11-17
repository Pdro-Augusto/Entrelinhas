using System.Text.RegularExpressions;
using Entrelinhas.Models;
using Entrelinhas.ViewModels;
using CommunityToolkit.Maui.Core.Platform;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Maui.Alerts;
using System.Data;

namespace Entrelinhas.Views;

[QueryProperty(nameof(LivroId), "livroId")]
public partial class BookPage : ContentPage
{
    private readonly BookViewModel _viewModel;
    private readonly IServiceProvider _serviceProvider;
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
    private const int TotalCoracoes = 5;
    private const double TamanhoCoracao = 40;
    private readonly Grid[] _preenchimentos = new Grid[TotalCoracoes];
    public BookPage(BookViewModel bookViewModel, IServiceProvider serviceProvider)
    {
        InitializeComponent();
        CriarCoracoes();
        _viewModel = bookViewModel;
        _serviceProvider = serviceProvider;
        BindingContext = _viewModel;

        BindableLayout.SetItemsSource(listaAnotacoes, _viewModel.Anotacoes);
        _viewModel.Anotacoes.CollectionChanged += (_, _) =>
            lbSemAnotacoes.IsVisible = _viewModel.Anotacoes.Count == 0;

        MostrarAba(0);
    }

    public Livro? livro
    {
        get => _viewModel.Livro;
        set => _viewModel.Livro = value; // Repassa o ID do livro para o ViewModel
    }

    public int? LivroId
    {
        get => _viewModel.LivroId;
        set => _viewModel.LivroId = value; // Repassa o ID do livro para o ViewModel
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (!_viewModel.LivroId.HasValue) return;

        await _viewModel.BuscarLivroAsync();

        if (_viewModel.Livro is null)
        {
            await DisplayAlertAsync("Aviso", "Livro não encontrado.", "OK");
            return;
        }

        AtualizarProgresso();
        AtualizarStatus();
        AtualizarFavorito();
        AtualizarAvaliacao();
        AtualizarListaDesejo();
    }

    // ---------- ABAS ----------

    private static Color Cor(string chave, string fallback) =>
        Application.Current?.Resources.TryGetValue(chave, out var v) == true && v is Color c
            ? c
            : Color.FromArgb(fallback);

    private void MostrarAba(int indice)
    {
        vsProgresso.IsVisible = indice == 0;
        vsAnotacoes.IsVisible = indice == 1;
        vsSobre.IsVisible = indice == 2;

        EstilizarAba(lbTabProgresso, ulProgresso, indice == 0);
        EstilizarAba(lbTabAnotacoes, ulAnotacoes, indice == 1);
        EstilizarAba(lbTabSobre, ulSobre, indice == 2);
    }

    private void EstilizarAba(Label rotulo, BoxView sublinhado, bool ativa)
    {
        rotulo.TextColor = ativa ? Cor("MainColor", "#D85A30") : Cor("SubFontAlpha", "#A08C82");
        rotulo.FontAttributes = ativa ? FontAttributes.Bold : FontAttributes.None;
        sublinhado.Color = ativa ? Cor("MainColor", "#D85A30") : Colors.Transparent;
    }

    private void OnAbaProgressoClicado(object? sender, TappedEventArgs e) => MostrarAba(0);
    private void OnAbaAnotacoesClicado(object? sender, TappedEventArgs e) => MostrarAba(1);
    private void OnAbaSobreClicado(object? sender, TappedEventArgs e) => MostrarAba(2);

    // ---------- PROGRESSO E STATUS ----------

    private void AtualizarProgresso()
    {
        if (livro is null) return;

        int total = livro.Paginas ?? 0;

        if (total <= 0)
        {
            pbProgresso.Progress = 0;
            lbPorcentagem.Text = "—";
            lbProgresso.Text = "Número de páginas não informado";
            return;
        }

        pbProgresso.Progress = Math.Clamp((double)livro.PaginasLidas / total, 0, 1);

        int percentual = Math.Clamp(livro.PaginasLidas * 100 / total, 0, 100);
        lbPorcentagem.Text = $"{percentual}%";

        lbProgresso.Text = $"{livro.PaginasLidas} de {total} páginas";
    }

    private void AtualizarStatus()
    {
        if (livro is null) return;
        lbStatus.Text = NomeStatus(livro.Status);
    }

    // Transforma o nome do enum em texto legível: "QueroLer" -> "Quero ler"
    private static string NomeStatus(StatusLeitura status)
    {
        var nome = Regex.Replace(status.ToString(), "(?<=[a-z])(?=[A-Z])", " ").ToLowerInvariant();
        return char.ToUpperInvariant(nome[0]) + nome[1..];
    }

    private async void OnAtualizarProgressoClicado(object? sender, EventArgs e)
    {
        await entryPaginasLidas.HideKeyboardAsync(CancellationToken.None);
        if (livro is null) return;

        if (!int.TryParse(entryPaginasLidas.Text, out var paginas) || paginas < 0)
        {
            await DisplayAlertAsync("Aviso", "Digite um número de página válido.", "OK");
            return;
        }

        int total = livro.Paginas ?? 0;
        if (total > 0 && paginas > total)
        {
            await DisplayAlertAsync("Aviso", $"O livro tem só {total} páginas.", "OK");
            return;
        }

        await _viewModel.SalvaProgressoAsync(paginas);
        var toast = Toast.Make("Páginas atualizadas!", ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);

        AtualizarProgresso();
        entryPaginasLidas.Text = string.Empty;
    }

    private async void OnStatusTocado(object? sender, TappedEventArgs e)
    {
        if (livro is null) return;

        var valores = Enum.GetValues<StatusLeitura>();
        var opcoes = valores.Select(NomeStatus).ToArray();

        var escolha = await DisplayActionSheetAsync(
            "Status de leitura", "Cancelar", null, opcoes);

        if (escolha is null or "Cancelar") return;

        foreach (var valor in valores)
        {
            if (NomeStatus(valor) == escolha)
            {
                livro.Status = valor;
                livro.DataConclusao = valor == StatusLeitura.Lido
                    ? livro.DataConclusao ?? DateTime.Now
                    : null;
                break;
            }
        }

        await _viewModel.AtualizarLivroAsync();
        var toast = Toast.Make("Status atualizado!", ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);

        AtualizarStatus();
    }

    // ---------- ANOTAÇÕES ----------

    private async void OnSalvarAnotacaoClicado(object? sender, EventArgs e)
    {
        await edtAnotacao.HideKeyboardAsync(CancellationToken.None);
        if (livro is null) return;

        var texto = edtAnotacao.Text?.Trim();
        if (string.IsNullOrWhiteSpace(texto))
        {
            await DisplayAlertAsync("Aviso", "Escreva alguma coisa antes de salvar.", "OK");
            return;
        }

        int? pagina = int.TryParse(entryPaginaNota.Text, out var p) ? p : null;

        var nota = new Anotacao
        {
            LivroId = livro.Id,
            Texto = texto,
            Pagina = pagina,
            Data = DateTime.Now
        };

        await _viewModel.AdicionarAnotacaoAsync(nota);
        var toast = Toast.Make("Anotação adicionada!", ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);

        edtAnotacao.Text = string.Empty;
        entryPaginaNota.Text = string.Empty;
    }

    private async void OnExcluirAnotacaoTocado(object? sender, TappedEventArgs e)
    {
        if ((sender as Label)?.BindingContext is not Anotacao nota) return;

        bool confirmou = await DisplayAlertAsync(
            "Excluir anotação", "Quer apagar essa anotação?", "Excluir", "Cancelar");
        if (!confirmou) return;

        await _viewModel.ExcluirAnotacaoAsync(nota);
        var toast = Toast.Make("Anotação excluída!", ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);
    }

    // ------------- FAVORITO -------------
    private void AtualizarFavorito()
    {
        if (livro is null) return;
        lbFavorito.Text = livro.Favorito ? "★" : "☆";
        lbFavorito.TextColor = livro.Favorito
            ? Cor("MainColor", "#D85A30")
            : Cor("SubFontAlpha", "#A08C82");
    }

    private async void OnFavoritoTocado(object? sender, TappedEventArgs e)
    {
        if (livro is null) return;

        livro.Favorito = !livro.Favorito;
        await _viewModel.AtualizarLivroAsync();
        AtualizarFavorito();

        var toast = Toast.Make(
            livro.Favorito ? "Adicionado aos favoritos!" : "Removido dos favoritos!",
            ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);
    }

    // ---------- MENU E NAVEGAÇÃO ----------
    private async void OnEditarClicado(object? sender, EventArgs e)
    {
        if (livro is null) return;

        var page = _serviceProvider.GetRequiredService<AddManualPage>();
        page.Inicializar(livro);
        await Shell.Current.Navigation.PushAsync(page);
    }

    private async void OnExcluirLivroClicado(object? sender, EventArgs e)
    {
        bool confirmou = await DisplayAlertAsync(
            "Excluir livro",
            "Tem certeza que quer tirar este livro da estante? Isso também apaga as anotações.",
            "Excluir", "Cancelar");
        if (!confirmou) return;

        await _viewModel.ExcluirLivroAsync();
        var toast = Toast.Make("Livro excluído!", ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);
        await Shell.Current.GoToAsync("..");
    }

    public async void btVolta_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }

    // --------------- AVALIAÇÃO ---------------------
    private void CriarCoracoes()
    {
        for (int i = 0; i < TotalCoracoes; i++)
        {
            int indice = i;

            var contorno = new Label
            {
                Text = "☆",
                FontSize = 34,
                TextColor = Cor("SubFontAlpha", "#A08C82"),
                WidthRequest = TamanhoCoracao,
                HeightRequest = TamanhoCoracao,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            var cheio = new Label
            {
                Text = "★",
                FontSize = 34,
                TextColor = Cor("MainColor", "#D85A30"),
                WidthRequest = TamanhoCoracao,
                HeightRequest = TamanhoCoracao,
                HorizontalOptions = LayoutOptions.Start,
                HorizontalTextAlignment = TextAlignment.Center,
                VerticalTextAlignment = TextAlignment.Center
            };

            // "janela" que mostra 0%, 50% ou 100% do coração cheio
            var corte = new Grid
            {
                WidthRequest = 0,
                HorizontalOptions = LayoutOptions.Start,
                IsClippedToBounds = true
            };
            corte.Add(cheio);
            _preenchimentos[i] = corte;

            var celula = new Grid { WidthRequest = TamanhoCoracao, HeightRequest = TamanhoCoracao };
            celula.Add(contorno);
            celula.Add(corte);

            var toque = new TapGestureRecognizer();
            toque.Tapped += async (s, e) =>
            {
                double x = e.GetPosition((View)s!)?.X ?? TamanhoCoracao;
                double nota = indice + (x < TamanhoCoracao / 2 ? 0.5 : 1.0);
                await DefinirAvaliacaoAsync(nota);
            };
            celula.GestureRecognizers.Add(toque);

            stCoracoes.Add(celula);
        }
    }

    private void AtualizarAvaliacao()
    {
        if (livro is null) return;

        for (int i = 0; i < TotalCoracoes; i++)
        {
            double parte = Math.Clamp(livro.Avaliacao - i, 0, 1);   // 0, 0.5 ou 1
            _preenchimentos[i].WidthRequest = TamanhoCoracao * parte;
        }

        lbAvaliacao.Text = livro.Avaliacao > 0
            ? $"{livro.Avaliacao:0.0} de 5"
            : "Sem avaliação";
    }

    private async Task DefinirAvaliacaoAsync(double nota)
    {
        if (livro is null) return;

        // tocar na mesma nota de novo limpa a avaliação
        livro.Avaliacao = livro.Avaliacao == nota ? 0 : nota;

        await _viewModel.AtualizarLivroAsync();
        AtualizarAvaliacao();

        var toast = Toast.Make(
            livro.Avaliacao > 0 ? $"Avaliação: {livro.Avaliacao:0.0}" : "Avaliação removida",
            ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);
    }

    // ------------ LISTA DE DESEJOS ----------------
    private void AtualizarListaDesejo()
    {
        if (livro is null) return;
        bool desejo = livro.ListaDesejo;

        // só na lista de desejos
        btMoverEstante.IsVisible = desejo;

        // só quando já está na estante
        lbProgresso.IsVisible = !desejo;
        pbProgresso.IsVisible = !desejo;
        gridAtualizar.IsVisible = !desejo;
        stAvaliacao.IsVisible = !desejo;
    }

    private async void OnMoverEstanteClicado(object? sender, EventArgs e)
    {
        if (livro is null) return;

        livro.ListaDesejo = false;
        await _viewModel.AtualizarLivroAsync();
        AtualizarListaDesejo();

        var toast = Toast.Make("Livro movido para a estante!", ToastDuration.Short, 14);
        await toast.Show(cancellationTokenSource.Token);
    }
}