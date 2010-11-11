using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    public class BoolInverterConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bl = value as bool?;
            if (!bl.HasValue) return Binding.DoNothing;

            return !bl.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
