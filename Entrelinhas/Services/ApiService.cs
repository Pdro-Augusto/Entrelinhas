using System.Collections.ObjectModel;
using System.Text.Json;

namespace Entrelinhas.Services
{
    public class ApiService
    {
        private const string GOOGLE_API_KEY = "AIzaSyAzAMQaYmO_4XMmexamwO3syWAIj2TxxoY";
        private readonly HttpClient _http;

        public ApiService()
        {
            _http = new HttpClient();
            _http.Timeout = TimeSpan.FromSeconds(10);
        }

        public async Task<Livro?> BuscarPorIsbnAsync(string isbn)
        {
            // Tenta cada API em sequência
            var livro = await BuscarNoGoogleBooksAsync(isbn);
            if (livro != null) return livro;

            livro = await BuscarNaOpenLibraryAsync(isbn);
            if (livro != null) return livro;

            livro = await BuscarNaBrasilApiAsync(isbn);
            if (livro != null) return livro;

            // Se todas falharem
            return null;
        }

        public async Task<ObservableCollection<Livro>?> BuscarPorPalavraChaveAsync(string palavraChave)
        {
            var livros = new ObservableCollection<Livro>();
            await BuscarNoGoogleBooksPorPalavraChaveAsync(palavraChave, livros);
            if (livros.Count > 0) return livros;

            await BuscarNaOpenLibraryPorPalavraChaveAsync(palavraChave, livros);
            if (livros.Count > 0) return livros;

            return null;
        }

        // ===== API 1: Google Books =====
        private async Task<Livro?> BuscarNoGoogleBooksAsync(string isbn)
        {
            try
            {
                var url = $"https://www.googleapis.com/books/v1/volumes?q=isbn:{isbn}&key={GOOGLE_API_KEY}";
                var json = await _http.GetStringAsync(url);
                var data = JsonDocument.Parse(json);

                if (!data.RootElement.TryGetProperty("items", out var items) || items.GetArrayLength() == 0)
                    return null;

                var info = items[0].GetProperty("volumeInfo");

                return new Livro
                {
                    Isbn = isbn,
                    Titulo = info.TryGetProperty("title", out var t) ? t.GetString() : null,
                    Autor = info.TryGetProperty("authors", out var a) ? 
                            string.Join(", ", a.EnumerateArray().Select(x => x.GetString())) : null,
                    Editora = info.TryGetProperty("publisher", out var p) ? p.GetString() : null,
                    Ano = ExtrairAno(info),
                    Paginas = info.TryGetProperty("pageCount", out var pg) ? pg.GetInt32() : null,
                    Sinopse = info.TryGetProperty("description", out var s) ? s.GetString() : null,
                    Genero = info.TryGetProperty("categories", out var c) ? c[0].GetString() : null,
                    CapaUrl = info.TryGetProperty("imageLinks", out var img)
                              && img.TryGetProperty("thumbnail", out var th)
                              ? th.GetString()?.Replace("http://", "https://")
                              : null
                };
            }
            catch
            {
                return null; // Se der erro, retorna null para tentar a próxima API
            }
        }

        private async Task BuscarNoGoogleBooksPorPalavraChaveAsync(string palavraChave, ObservableCollection<Livro> livros)
        {
            try
            {
                var url = $"https://www.googleapis.com/books/v1/volumes?q={Uri.EscapeDataString(palavraChave)}&maxResults=40key={GOOGLE_API_KEY}";
                var json = await _http.GetStringAsync(url);
                var data = JsonDocument.Parse(json);
                if (!data.RootElement.TryGetProperty("items", out var items)) return;
                foreach (var item in items.EnumerateArray())
                {
                    var info = item.GetProperty("volumeInfo");
                    livros.Add(new Livro
                    {
                        Isbn = info.TryGetProperty("industryIdentifiers", out var ids) && ids.GetArrayLength() > 0
                               ? ids[0].GetProperty("identifier").GetString()
                               : null,
                        Titulo = info.TryGetProperty("title", out var t) ? t.GetString() : null,
                        Autor = info.TryGetProperty("authors", out var a) ?
                                string.Join(", ", a.EnumerateArray().Select(x => x.GetString())) : null,
                        Editora = info.TryGetProperty("publisher", out var p) ? p.GetString() : null,
                        Ano = ExtrairAno(info),
                        Paginas = info.TryGetProperty("pageCount", out var pg) ? pg.GetInt32() : null,
                        Sinopse = info.TryGetProperty("description", out var s) ? s.GetString() : null,
                        Genero = info.TryGetProperty("categories", out var c) ? c[0].GetString() : null,
                        CapaUrl = info.TryGetProperty("imageLinks", out var img)
                                  && img.TryGetProperty("thumbnail", out var th)
                                  ? th.GetString()?.Replace("http://", "https://")
                                  : null
                    });
                }
            }
            catch
            {
                // Ignora erros e continua
            }
        }

        // ===== API 2: Open Library =====
        private async Task<Livro?> BuscarNaOpenLibraryAsync(string isbn)
        {
            try
            {
                var url = $"https://openlibrary.org/api/books?bibkeys=ISBN:{isbn}&format=json&jscmd=data";
                var json = await _http.GetStringAsync(url);
                var data = JsonDocument.Parse(json);

                // A Open Library retorna um objeto vazio {} se não encontrar
                if (!data.RootElement.TryGetProperty($"ISBN:{isbn}", out var livroData))
                    return null;

                return new Livro
                {
                    Isbn = isbn,
                    Titulo = livroData.TryGetProperty("title", out var t) ? t.GetString() : null,
                    Autor = livroData.TryGetProperty("authors", out var a) && a.GetArrayLength() > 0
                        ? string.Join(", ", a.EnumerateArray().Select(x => x.GetString())) : null,
                    Editora = livroData.TryGetProperty("publishers", out var p) && p.GetArrayLength() > 0
                        ? p[0].GetProperty("name").GetString() : null,
                    Ano = ExtrairAnoOpenLibrary(livroData),
                    Paginas = livroData.TryGetProperty("number_of_pages", out var pg) ? pg.GetInt32() : null,
                    CapaUrl = livroData.TryGetProperty("cover", out var cover) && cover.TryGetProperty("medium", out var m)
                        ? m.GetString() : null
                };
            }
            catch
            {
                return null;
            }
        }

        private async Task BuscarNaOpenLibraryPorPalavraChaveAsync(string palavraChave, ObservableCollection<Livro> livros)
        {
            try
            {
                var url = $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(palavraChave)}&limit=40&fields=key,title,author_name,first_publish_year,cover_i,isbn,number_of_pages_median"; ;
                var json = await _http.GetStringAsync(url);
                var data = JsonDocument.Parse(json);
                if (!data.RootElement.TryGetProperty("docs", out var docs)) return;
                foreach (var doc in docs.EnumerateArray())
                {
                    livros.Add(new Livro
                    {
                        Isbn = doc.TryGetProperty("isbn", out var isbns) && isbns.GetArrayLength() > 0
                               ? isbns[0].GetString() : null,
                        Titulo = doc.TryGetProperty("title", out var t) ? t.GetString() : null,
                        Autor = doc.TryGetProperty("author_name", out var a) && a.GetArrayLength() > 0
                                ? string.Join(", ", a.EnumerateArray().Select(x => x.GetString())) : null,
                        Editora = doc.TryGetProperty("publisher", out var p) && p.GetArrayLength() > 0
                                ? p[0].GetString() : null,
                        Ano = doc.TryGetProperty("first_publish_year", out var y) ? y.GetInt32() : null,
                        Paginas = null, // A Open Library não fornece número de páginas na busca
                        CapaUrl = doc.TryGetProperty("cover_i", out var coverId) && coverId.ValueKind == JsonValueKind.Number
                                   ? $"https://covers.openlibrary.org/b/id/{coverId.GetInt32()}-M.jpg"
                                   : null
                    });
                }
            }
            catch
            {
                // Ignora erros e continua
            }
        }

        // ===== API 3: BrasilAPI =====
        private async Task<Livro?> BuscarNaBrasilApiAsync(string isbn)
        {
            try
            {
                var url = $"https://brasilapi.com.br/api/isbn/v1/{isbn}";
                var json = await _http.GetStringAsync(url);
                var data = JsonDocument.Parse(json);
                var root = data.RootElement;

                return new Livro
                {
                    Isbn = root.TryGetProperty("isbn", out var i) ? i.GetString() : isbn,
                    Titulo = root.TryGetProperty("title", out var t) ? t.GetString() : null,
                    Autor = root.TryGetProperty("authors", out var a) && a.GetArrayLength() > 0
                        ? string.Join(", ", a.EnumerateArray().Select(x => x.GetString()))
                        : null,
                    Editora = root.TryGetProperty("publisher", out var p) ? p.GetString() : null,
                    Ano = root.TryGetProperty("year", out var y) && y.ValueKind == JsonValueKind.Number
                        ? y.GetInt32() : null,
                    Paginas = root.TryGetProperty("page_count", out var pg) && pg.ValueKind == JsonValueKind.Number
                        ? pg.GetInt32() : null,
                    Sinopse = root.TryGetProperty("synopsis", out var s) ? s.GetString() : null,
                    Genero = root.TryGetProperty("subjects", out var sub) && sub.GetArrayLength() > 0
                        ? string.Join(", ", sub.EnumerateArray().Select(x => x.GetString()))
                        : null,
                    CapaUrl = root.TryGetProperty("cover_url", out var c) && c.ValueKind != JsonValueKind.Null
                        ? c.GetString() : null
                };
            }
            catch
            {
                return null;
            }
        }


        // ===== Helpers =====
        private int? ExtrairAno(JsonElement info)
        {
            if (info.TryGetProperty("publishedDate", out var d))
            {
                var dataStr = d.GetString();
                if (!string.IsNullOrEmpty(dataStr) && dataStr.Length >= 4)
                    return int.Parse(dataStr.Substring(0, 4));
            }
            return null;
        }

        private int? ExtrairAnoOpenLibrary(JsonElement livroData)
        {
            if (livroData.TryGetProperty("publish_date", out var d))
            {
                var dataStr = d.GetString();
                if (!string.IsNullOrEmpty(dataStr))
                {
                    // A Open Library retorna datas em formatos variados
                    var match = System.Text.RegularExpressions.Regex.Match(dataStr, @"\d{4}");
                    if (match.Success)
                        return int.Parse(match.Value);
                }
            }
            return null;
        }
    }
}