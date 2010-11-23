using System;
using System.Globalization;
using System.Windows.Data;

namespace EpubManga
{
    /// <summary>
    /// Converts a DoublePage enumeration to a string.
    /// </summary>
    public class DoublePageToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            DoublePage? doublePage = value as DoublePage?;
            if (!doublePage.HasValue) return Binding.DoNothing;

            string result = doublePage.ToString();
            for (int i = 1; i < result.Length; i++)
            {
                if (Char.IsUpper(result[i]))
                {
                    result = result.Insert(i, " ");
                    i++;
                }
            }

            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string doublePage = value as string;
            if (string.IsNullOrEmpty(doublePage)) return Binding.DoNothing;

            return (DoublePage)Enum.Parse(typeof(DoublePage), doublePage.Replace(" ", ""));
        }
    }
}
