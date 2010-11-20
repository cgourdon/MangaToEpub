using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace EpubManga
{
    /// <summary>
    /// Returns Visible if True is given.
    /// If the parameter is empty or False, it returns Collapsed if bound to a False boolean, Hidden if the parameter is True.
    /// </summary>
    public class BoolToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? bl = value as bool?;
            if (!bl.HasValue) return Binding.DoNothing;

            bool hiddenInsteadOfCollapsed = false;
            Boolean.TryParse(parameter == null ? string.Empty : parameter.ToString(), out hiddenInsteadOfCollapsed);

            return bl == true ? Visibility.Visible : (hiddenInsteadOfCollapsed ? Visibility.Hidden : Visibility.Collapsed);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
