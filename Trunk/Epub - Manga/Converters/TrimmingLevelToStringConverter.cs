using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    /// <summary>
    /// Converts a TrimmingLevel enumeration to a string.
    /// </summary>
    public class TrimmingLevelToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            TrimmingLevel? trimmingLevel = value as TrimmingLevel?;
            if (!trimmingLevel.HasValue) return Binding.DoNothing;

            return trimmingLevel.Value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string trimmingLevel = value as string;
            if (string.IsNullOrEmpty(trimmingLevel)) return Binding.DoNothing;

            return (TrimmingLevel)Enum.Parse(typeof(TrimmingLevel), trimmingLevel);
        }
    }
}
