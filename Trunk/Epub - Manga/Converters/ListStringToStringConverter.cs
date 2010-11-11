using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace EpubManga
{
    public class ListStringToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            List<string> strings = value as List<string>;
            if (strings == null) return Binding.DoNothing;

            StringBuilder builder = new StringBuilder();
            bool first = true;

            foreach (string str in strings)
            {
                if (first) first = false;
                else builder.Append(" - ");

                builder.Append("\"");
                builder.Append(str.Substring(str.LastIndexOf('\\') + 1));
                builder.Append("\"");
            }

            return builder.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
