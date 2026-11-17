using Entrelinhas.Models;
using SQLite;

namespace Entrelinhas.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _db;
        private readonly Lazy<Task> _initTask;

        public DatabaseService()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "entrelinhas.db3");
            _db = new SQLiteAsyncConnection(path);
            _initTask = new Lazy<Task>(InicializarAsync);
        }

        private async Task InicializarAsync()
        {
            await _db.CreateTableAsync<Livro>();
            await _db.CreateTableAsync<Anotacao>();
            await _db.CreateTableAsync<RegistroLeitura>();
        }

        public async Task<int> InserirLivroAsync(Livro livro)
        {
            await _initTask.Value;

            if (!string.IsNullOrEmpty(livro.Isbn) &&
                await _db.Table<Livro>().Where(l => l.Isbn == livro.Isbn).FirstOrDefaultAsync() != null)
            {
                return -1; // Livro já existe
            }
            return await _db.InsertAsync(livro);
        }

        public async Task<List<Livro>> ObterTodosAsync()
        {
            await _initTask.Value;
            return await _db.Table<Livro>().Where(l => l.ListaDesejo == false).ToListAsync();
        }

        public async Task<List<Livro>> ObterListaDesejoAsync()
        {
            await _initTask.Value;
            return await _db.Table<Livro>().Where(l => l.ListaDesejo == true).ToListAsync();
        }

        public async Task<Livro?> ObterPorIdAsync(int id)
        {
            await _initTask.Value;
            return await _db.Table<Livro>().FirstOrDefaultAsync(l => l.Id == id);
        }

        // ---------- NOVOS: detalhes do livro e anotações ----------

        public async Task<int> AtualizarLivroAsync(Livro livro)
        {
            await _initTask.Value;
            return await _db.UpdateAsync(livro);
        }

        public async Task<List<Anotacao>> ListarAnotacoesAsync(int livroId)
        {
            await _initTask.Value;
            return await _db.Table<Anotacao>()
                            .Where(a => a.LivroId == livroId)
                            .OrderByDescending(a => a.Data)
                            .ToListAsync();
        }

        public async Task<int> SalvarAnotacaoAsync(Anotacao nota)
        {
            await _initTask.Value;
            return await _db.InsertAsync(nota);
        }

        public async Task ExcluirAnotacaoAsync(int id)
        {
            await _initTask.Value;
            await _db.DeleteAsync<Anotacao>(id);
        }

        public async Task ExcluirLivroAsync(int livroId)
        {
            // Apaga primeiro as anotações do livro, depois o livro em si
            await _db.ExecuteAsync("DELETE FROM Anotacao WHERE LivroId = ?", livroId);
            await _db.DeleteAsync<Livro>(livroId);
        }

        public async Task RegistraLeituraAsync(int livroId, int paginas)
        {
            await _initTask.Value;
            await _db.InsertAsync(new RegistroLeitura
            {
                LivroId = livroId,
                Data = DateTime.Today,
                Paginas = paginas
            });
        }

        public async Task<List<RegistroLeitura>>ObterRegistroDesdeAsync(DateTime desde)
        {
            await _initTask.Value;
            return await _db.Table<RegistroLeitura>().Where(r => r.Data >= desde).ToListAsync();
        }
    }
}