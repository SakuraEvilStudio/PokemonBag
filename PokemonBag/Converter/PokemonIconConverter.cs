using System;
using System.Globalization;
using System.Windows.Data;

namespace PokemonBag.Converter
{
    public class PokemonIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string url = string.Format("/Resources/Pokemon/{0}.png", value.ToString().ToLower()); ;
            return url;        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
