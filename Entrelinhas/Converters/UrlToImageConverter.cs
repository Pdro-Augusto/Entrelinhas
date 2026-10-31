using System.Globalization;

namespace Entrelinhas.Converters
{
    public class UrlToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is not string caminho || string.IsNullOrWhiteSpace(caminho))
            {
                return "book_placeholder.svg";
            }

            // 1. Verifica se é um arquivo local (não começa com http)
            if (!caminho.StartsWith("http", StringComparison.OrdinalIgnoreCase))
            {
                if(caminho.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
                {
                    caminho = caminho.Substring(8);
                }
                return caminho;
            }

            // 2. Se for URL, trata como antes
            var urlSegura = caminho.Replace("http://", "https://");

            try
            {
                return ImageSource.FromUri(new Uri(urlSegura));
            }
            catch
            {
                return "book_placeholder.svg";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}