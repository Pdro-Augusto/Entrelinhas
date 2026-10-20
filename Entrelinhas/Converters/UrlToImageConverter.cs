using System.Globalization;

namespace Entrelinhas.Converters
{
    public class UrlToImageConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // Se a URL for nula ou vazia, retorna o placeholder
            if (value is not string url || string.IsNullOrWhiteSpace(url))
            {
                return "book_placeholder.svg"; // Nome do arquivo na pasta Resources/Images
            }

            // Garante que a URL use HTTPS para não ser bloqueada pelo Android
            var urlSegura = url.Replace("http://", "https://");

            try
            {
                return ImageSource.FromUri(new Uri(urlSegura));
            }
            catch
            {
                // Se a URL for inválida, também retorna o placeholder
                return "book_placeholder.svg";
            }
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}