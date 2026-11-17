using Entrelinhas.Models;
using Entrelinhas.Services;

namespace Entrelinhas.Views;

public partial class UserPage : ContentPage
{
    private const string ChaveNome = "nome_usuario";
    private const string ChaveMeta = "meta_anual";

    private readonly DatabaseService _db;

    public UserPage(DatabaseService db)
    {
        InitializeComponent();
        _db = db;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        try
        {
            await CarregarAsync();
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro", ex.Message, "OK");
        }
    }

    private async Task CarregarAsync()
    {
        var estante = await _db.ObterTodosAsync();

        PreencherNome();
        PreencherMeta(estante);
        PreencherFavoritos(estante);
    }

    // ---------- NOME E AVATAR ----------

    private void PreencherNome()
    {
        var nome = Preferences.Get(ChaveNome, string.Empty);
        lbNome.Text = string.IsNullOrWhiteSpace(nome) ? "Leitor" : nome;
        lbInicial.Text = lbNome.Text[0].ToString().ToUpperInvariant();
    }

    private async void OnNomeTocado(object? sender, TappedEventArgs e)
    {
        var resposta = await DisplayPromptAsync(
            "Seu nome", "Como você quer ser chamado?", "Salvar", "Cancelar",
            placeholder: "Seu nome",
            maxLength: 20,
            initialValue: Preferences.Get(ChaveNome, string.Empty));

        if (string.IsNullOrWhiteSpace(resposta)) return;

        Preferences.Set(ChaveNome, resposta.Trim());
        PreencherNome();
    }

    // ---------- META DO ANO ----------

    private void PreencherMeta(List<Livro> estante)
    {
        int ano = DateTime.Today.Year;
        int meta = Preferences.Get(ChaveMeta, 0);
        int lidos = estante.Count(l => l.DataConclusao?.Year == ano);

        lbMetaTitulo.Text = $"meta de {ano}";
        lbMetaConcluidos.Text = lidos.ToString();

        if (meta <= 0)
        {
            lbMetaTotal.Text = lidos == 1 ? "livro lido este ano" : "livros lidos este ano";
            lbMetaAcao.Text = "definir meta";
            pbMeta.IsVisible = false;
            return;
        }

        lbMetaTotal.Text = $"de {meta} {(meta == 1 ? "livro" : "livros")}";
        lbMetaAcao.Text = "editar";
        pbMeta.IsVisible = true;
        pbMeta.Progress = Math.Clamp((double)lidos / meta, 0, 1);
    }

    private async void OnMetaTocada(object? sender, TappedEventArgs e)
    {
        int meta = Preferences.Get(ChaveMeta, 0);

        var resposta = await DisplayPromptAsync(
            $"Meta de {DateTime.Today.Year}", "Quantos livros você quer ler este ano?",
            "Salvar", "Cancelar",
            placeholder: "12",
            maxLength: 3,
            keyboard: Keyboard.Numeric,
            initialValue: meta > 0 ? meta.ToString() : string.Empty);

        if (resposta is null) return; // cancelou

        if (!int.TryParse(resposta, out var nova) || nova <= 0)
        {
            await DisplayAlertAsync("Aviso", "Digite um número maior que zero.", "OK");
            return;
        }

        Preferences.Set(ChaveMeta, nova);
        await CarregarAsync();
    }

    // ---------- FAVORITOS ----------

    private void PreencherFavoritos(List<Livro> estante)
    {
        var favoritos = estante.Where(l => l.Favorito).ToList();

        BindableLayout.SetItemsSource(listaFavoritos, favoritos);
        lbSemFavoritos.IsVisible = favoritos.Count == 0;
    }

    private async void OnFavoritoTocado(object? sender, TappedEventArgs e)
    {
        if ((sender as BindableObject)?.BindingContext is not Livro livro) return;

        await Shell.Current.GoToAsync($"detalhelivro?livroId={livro.Id}");
    }

    // ---------- AJUDA ----------

    private async void OnAjudaTocado(object? sender, TappedEventArgs e)
    {
        try
        {
            await Shell.Current.GoToAsync("sobre");
        }
        catch (Exception ex)
        {
            await DisplayAlertAsync("Erro:", ex.Message, "OK");
        }
    }
}