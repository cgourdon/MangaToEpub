using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    /// <summary>
    /// Converts a DoublePage enumeration to a boolean.
    /// </summary>
    public class DoublePageToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DoublePage? doublePage = value as DoublePage?;
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

            DoublePage? doublePage = Enum.Parse(typeof(DoublePage), target) as DoublePage?;
            if (!doublePage.HasValue) return Binding.DoNothing;

            return doublePage.Value;
        }
    }
}
