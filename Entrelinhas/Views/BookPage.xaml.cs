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
    private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

    public BookPage(BookViewModel bookViewModel)
    {
        InitializeComponent();
        _viewModel = bookViewModel;
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
            lbProgresso.Text = "Número de páginas não informado";
            return;
        }

        pbProgresso.Progress = Math.Clamp((double)livro.PaginasLidas / total, 0, 1);
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

        livro.PaginasLidas = paginas;
        await _viewModel.AtualizarLivroAsync();
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

    // ---------- MENU E NAVEGAÇÃO ----------

    private async void OnMenuClicado(object? sender, TappedEventArgs e)
    {
        var acao = await DisplayActionSheetAsync(
            "Opções", "Cancelar", "Remover da estante");

        if (acao != "Remover da estante") return;

        bool confirmou = await DisplayAlertAsync(
            "Remover livro", "Tem certeza que quer tirar este livro da estante?",
            "Remover", "Cancelar");
        if (!confirmou) return;

        await _viewModel.ExcluirLivroAsync();
        await Shell.Current.GoToAsync("..");
    }

    public async void btVolta_Clicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync("..");
    }
}