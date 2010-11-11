using System;
using System.Globalization;
using System.Windows.Data;


namespace EpubManga
{
    public class BoolLogicMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values == null) return Binding.DoNothing;
            if (values.Length != 2) return Binding.DoNothing;

            bool? bl1 = values[0] as bool?;
            if (!bl1.HasValue) return Binding.DoNothing;

            bool? bl2 = values[1] as bool?;
            if (!bl2.HasValue) return Binding.DoNothing;

            return bl1.Value & bl2.Value;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
