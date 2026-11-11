using Entrelinhas.Services;

namespace Entrelinhas.ViewModels
{
    public partial class AddManualViewModel : BaseViewModel
    {
        private readonly DatabaseService _databaseService;
        private Livro _livro = new Livro();
        public AddManualViewModel(DatabaseService databaseService)
        {
            _databaseService = databaseService;
        }

        public Livro Livro
        {
            get => _livro;
            set => SetProperty(ref _livro, value);
        }

        public async Task<int> SalvarLivroAsync(int? statusLeitura, bool? listaDesejo = null)
        {
            if (Livro == null) return 0;

            IsLoading = true;
            try
            {
                if (listaDesejo.HasValue)
                    Livro.ListaDesejo = listaDesejo.Value;
                if (statusLeitura == null)
                    statusLeitura = 0; // Default para QueroLer
                Livro.Status = (StatusLeitura)statusLeitura;
                if (Livro.Id != 0)
                    return await _databaseService.AtualizarLivroAsync(Livro);   // use o método de update que o BookViewModel já usa
                return await _databaseService.InserirLivroAsync(Livro);
            }
            finally
            {
                IsLoading = false;
            }
        }

    }
}
