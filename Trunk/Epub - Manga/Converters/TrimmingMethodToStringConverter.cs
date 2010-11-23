using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    /// <summary>
    /// Converts a TrimmingMethod enumeration to a string.
    /// </summary>
    public class TrimmingMethodToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TrimmingMethod? trimmingMethod = value as TrimmingMethod?;
            if (!trimmingMethod.HasValue) return Binding.DoNothing;

            return trimmingMethod.Value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string trimmingMethod = value as string;
            if (string.IsNullOrEmpty(trimmingMethod)) return Binding.DoNothing;

            return (TrimmingMethod)Enum.Parse(typeof(TrimmingMethod), trimmingMethod);
        }
    }
}
