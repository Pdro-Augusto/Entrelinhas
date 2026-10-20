using System.Collections.ObjectModel;
using Entrelinhas.Models;
using Entrelinhas.Services;

namespace Entrelinhas.ViewModels;

public partial class BookViewModel : BaseViewModel
{
    private readonly DatabaseService _databaseService;

    public BookViewModel(DatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    private int? _livroId;
    public int? LivroId
    {
        get => _livroId;
        set => SetProperty(ref _livroId, value);
    }

    private Livro? _livro;
    public Livro? Livro // Propriedade que representa o livro encontrado
    {
        get => _livro;
        set => SetProperty(ref _livro, value);
    }

    // Anotações do livro aberto (a página usa essa lista direto)
    public ObservableCollection<Anotacao> Anotacoes { get; } = new();

    public async Task BuscarLivroAsync()
    {
        if (!LivroId.HasValue) return;
        IsLoading = true;
        try
        {
            Livro = await _databaseService.ObterPorIdAsync(LivroId.Value);

            Anotacoes.Clear();
            if (Livro is not null)
            {
                var notas = await _databaseService.ListarAnotacoesAsync(LivroId.Value);
                foreach (var nota in notas)
                    Anotacoes.Add(nota);
            }
        }
        finally
        {
            IsLoading = false;
        }
    }

    // Grava no banco o que mudou no livro (progresso, status)
    public async Task AtualizarLivroAsync()
    {
        if (Livro is null) return;
        try
        {
            IsLoading = true;
            await _databaseService.AtualizarLivroAsync(Livro);
        }
        finally
        {
            IsLoading = false;
        }
    }

    public async Task AdicionarAnotacaoAsync(Anotacao nota)
    {
        await _databaseService.SalvarAnotacaoAsync(nota);
        Anotacoes.Insert(0, nota); // a mais nova aparece primeiro
    }

    public async Task ExcluirAnotacaoAsync(Anotacao nota)
    {
        await _databaseService.ExcluirAnotacaoAsync(nota.Id);
        Anotacoes.Remove(nota);
    }

    public async Task ExcluirLivroAsync()
    {
        if (!LivroId.HasValue) return;
        await _databaseService.ExcluirLivroAsync(LivroId.Value);
    }
}