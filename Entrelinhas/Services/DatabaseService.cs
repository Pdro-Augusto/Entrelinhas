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
            return await _db.InsertAsync(livro);
        }

        public async Task<List<Livro>> ObterTodosAsync()
        {
            return await _db.Table<Livro>().ToListAsync();
        }
    }
}