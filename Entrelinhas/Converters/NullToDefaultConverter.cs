using System.Globalization;

namespace Entrelinhas.Converters
{
    public class NullToDefaultConverter : IValueConverter
    {
        public string DefaultValue { get; set; } = "Não informado";

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value == null || string.IsNullOrWhiteSpace(value.ToString()))
                return DefaultValue;

            return value;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}