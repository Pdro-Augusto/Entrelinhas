using SQLite;

namespace Entrelinhas.Services
{
    public class DatabaseService
    {
        private readonly SQLiteAsyncConnection _db;

        public DatabaseService()
        {
            var path = Path.Combine(FileSystem.AppDataDirectory, "entrelinhas.db3");
            _db = new SQLiteAsyncConnection(path);
            _db.CreateTableAsync<Livro>().Wait();
        }

        public async Task<int> InserirLivroAsync(Livro livro)
        {
            if (await _db.Table<Livro>().Where(l => l.Isbn == livro.Isbn).FirstOrDefaultAsync() != null)
            {
                return -1; // Livro já existe
            }
            return await _db.InsertAsync(livro);
        }

        public async Task<List<Livro>> ObterTodosAsync()
        {
            return await _db.Table<Livro>().Where(l => l.ListaDesejo == false).ToListAsync();
        }

        public async Task<List<Livro>> ObterListaDesejoAsync()
        {
            return await _db.Table<Livro>().Where(l => l.ListaDesejo == true).ToListAsync();
        }

        public async Task<Livro?> ObterPorIdAsync(int id)
        {
            return await _db.Table<Livro>().FirstOrDefaultAsync(l => l.Id == id);
        }
    }
}