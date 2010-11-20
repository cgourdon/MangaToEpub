using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    /// <summary>
    /// Converts a TrimmingLevel enumeration to a boolean.
    /// </summary>
    public class TrimmingLevelToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TrimmingLevel? doublePage = value as TrimmingLevel?;
            if (!doublePage.HasValue) return Binding.DoNothing;

            string target = parameter as string;
            if (string.IsNullOrEmpty(target)) return Binding.DoNothing;

            if (doublePage.Value.ToString().Equals(target, StringComparison.InvariantCulture)) return true;
            else return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool? isChecked = value as bool?;
            if (!isChecked.HasValue) return Binding.DoNothing;
            if (!isChecked.Value) return Binding.DoNothing;

            string target = parameter as string;
            if (string.IsNullOrEmpty(target)) return Binding.DoNothing;

            TrimmingLevel? doublePage = Enum.Parse(typeof(TrimmingLevel), target) as TrimmingLevel?;
            if (!doublePage.HasValue) return Binding.DoNothing;

            return doublePage.Value;
        }
    }
}
