using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    public class TrimmingMethodToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TrimmingMethod? doublePage = value as TrimmingMethod?;
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

            TrimmingMethod? doublePage = Enum.Parse(typeof(TrimmingMethod), target) as TrimmingMethod?;
            if (!doublePage.HasValue) return Binding.DoNothing;

            return doublePage.Value;
        }
    }
}
