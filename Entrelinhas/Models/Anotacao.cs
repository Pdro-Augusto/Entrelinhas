using System;
using System.Collections.Generic;
using System.Text;

namespace Entrelinhas.Models
{
    public class Anotacao
    {
        public int Id { get; set; }
        public int LivroId { get; set; }
        public string Texto { get; set; } = string.Empty;
        public int? Pagina { get; set; }
        public DateTime Data { get; set; } = DateTime.Now;

        public string Rodape =>
            Pagina is null
                ? Data.ToString("dd/MM")
                : $"pág. {Pagina} · {Data:dd/MM}";
    }
}
