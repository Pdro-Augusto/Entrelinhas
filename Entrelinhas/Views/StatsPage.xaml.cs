using Entrelinhas.Models;
using Entrelinhas.Services;

namespace Entrelinhas.Views;

public class DiaGrafico
{
    public string Rotulo { get; set; } = string.Empty;
    public string Valor { get; set; } = string.Empty;
    public double Altura { get; set; }
    public Color Cor { get; set; } = Colors.Transparent;
    public Color CorTexto { get; set; } = Colors.Gray;
}

public partial class StatsPage : ContentPage
{
    private static readonly string[] Dias = { "dom", "seg", "ter", "qua", "qui", "sex", "sáb" };

    private static readonly string[] Meses =
    {
        "janeiro", "fevereiro", "março", "abril", "maio", "junho",
        "julho", "agosto", "setembro", "outubro", "novembro", "dezembro"
    };

    private readonly DatabaseService _db;

    public StatsPage(DatabaseService db)
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
        var desejos = await _db.ObterListaDesejoAsync();

        PreencherContagens(estante, desejos);
        PreencherMes(estante);
        await PreencherDiasAsync();
    }

    private void PreencherContagens(List<Livro> estante, List<Livro> desejos)
    {
        lbLidos.Text = estante.Count(l => l.Status == StatusLeitura.Lido).ToString();
        lbLendo.Text = estante.Count(l => l.Status == StatusLeitura.Lendo).ToString();

        // "Quero ler" = livros da estante com esse status + a lista de desejos
        lbQueroLer.Text = (estante.Count(l => l.Status == StatusLeitura.QueroLer) + desejos.Count).ToString();

        // Livro marcado como lido sem atualizar o progresso conta com o total de páginas dele
        int totalPaginas = estante.Sum(l =>
            l.Status == StatusLeitura.Lido && l.PaginasLidas == 0
                ? l.Paginas ?? 0
                : l.PaginasLidas);

        lbPaginasTotal.Text = totalPaginas.ToString("N0");
    }

    private void PreencherMes(List<Livro> estante)
    {
        var hoje = DateTime.Today;
        var inicioMes = new DateTime(hoje.Year, hoje.Month, 1);
        var inicioMesAnterior = inicioMes.AddMonths(-1);

        int noMes = estante.Count(l => l.DataConclusao >= inicioMes);
        int noMesAnterior = estante.Count(l =>
            l.DataConclusao >= inicioMesAnterior && l.DataConclusao < inicioMes);

        lbLivrosMes.Text = noMes.ToString();
        lbMesTitulo.Text = $"livros lidos em {Meses[inicioMes.Month - 1]}";
        lbMesAnterior.Text =
            $"{Meses[inicioMesAnterior.Month - 1]}: {noMesAnterior} {(noMesAnterior == 1 ? "livro" : "livros")}";
    }

    private async Task PreencherDiasAsync()
    {
        var hoje = DateTime.Today;
        var inicio = hoje.AddDays(-6);

        var registros = await _db.ObterRegistroDesdeAsync(inicio);
        var porDia = registros
            .GroupBy(r => r.Data.Date)
            .ToDictionary(g => g.Key, g => g.Sum(r => r.Paginas));

        var dias = Enumerable.Range(0, 7)
            .Select(i => inicio.AddDays(i))
            .Select(d => (Dia: d, Paginas: porDia.GetValueOrDefault(d, 0)))
            .ToList();

        int maximo = Math.Max(dias.Max(d => d.Paginas), 1);
        var corPrincipal = Cor("MainColor", "#D85A30");
        var corTexto = Cor("SubFontColor", "#7A5C50");

        var itens = dias.Select(d =>
        {
            bool ehHoje = d.Dia == hoje;
            return new DiaGrafico
            {
                Rotulo = ehHoje ? "hoje" : Dias[(int)d.Dia.DayOfWeek],
                Valor = d.Paginas.ToString(),
                Altura = Math.Max(3, 64.0 * d.Paginas / maximo),
                Cor = ehHoje ? corPrincipal : corPrincipal.WithAlpha(0.45f),
                CorTexto = ehHoje ? corPrincipal : corTexto
            };
        }).ToList();

        BindableLayout.SetItemsSource(graficoBarra, itens);
        BindableLayout.SetItemsSource(graficoRotulos, itens);

        int media = (int)Math.Round(dias.Sum(d => d.Paginas) / 7.0);
        lbMedia.Text = $"média de {media} por dia";
    }

    private static Color Cor(string chave, string fallback) =>
        Application.Current?.Resources.TryGetValue(chave, out var v) == true && v is Color c
            ? c
            : Color.FromArgb(fallback);
}