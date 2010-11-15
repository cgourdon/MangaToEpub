using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EpubManga
{
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bl = value as bool?;
            if (!bl.HasValue) return Binding.DoNothing;

            return bl == true ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
