using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace EpubManga
{
    public static class TextBoxExtension
    {
        public static DependencyProperty RegexpProperty
            = DependencyProperty.RegisterAttached("Regexp", typeof(Regexp), typeof(TextBoxExtension), new PropertyMetadata(OnRegexpChanged));

        public static void SetRegexp(DependencyObject d, Regexp value)
        {
            d.SetValue(RegexpProperty, value);
        }

        public static Regexp GetRegexp(DependencyObject d)
        {
            return (Regexp)d.GetValue(RegexpProperty);
        }

        private static void OnRegexpChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBox textBox = d as TextBox;
            if (textBox == null) return;

            textBox.TextChanged -= textBox_TextChanged;
            textBox.TextChanged += textBox_TextChanged;

            textBox.Unloaded -= textBox_Unloaded;
            textBox.Unloaded += textBox_Unloaded;
        }

        private static void textBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;
            if (string.IsNullOrEmpty(textBox.Text)) return;

            Regexp regexp = GetRegexp(textBox);
            string pattern;

            switch (regexp)
            {
                case Regexp.FileName:
                    pattern = "^[^:\\\\/*?<>\"\\|]*$";
                    break;
                case Regexp.Integer:
                    pattern = "^[0-9]*$";
                    break;
                default:
                    pattern = string.Empty;
                    break;
            }

            if (Regex.Match(textBox.Text, pattern).Success)
            {
                SetPreviousText(textBox, textBox.Text);
                SetPreviousSelectedIndex(textBox, textBox.SelectionStart);
            }
            else
            {
                textBox.TextChanged -= textBox_TextChanged;
                textBox.Text = GetPreviousText(textBox);
                textBox.TextChanged += textBox_TextChanged;
                textBox.SelectionStart = GetPreviousSelectedIndex(textBox);
            }
        }

        private static void textBox_Unloaded(object sender, RoutedEventArgs e)
        {
            TextBox textBox = sender as TextBox;
            if (textBox == null) return;

            textBox.TextChanged -= textBox_TextChanged;
            textBox.Unloaded -= textBox_Unloaded;
        }



        private static DependencyProperty PreviousTextProperty
            = DependencyProperty.RegisterAttached("PreviousText", typeof(string), typeof(TextBoxExtension));

        private static void SetPreviousText(DependencyObject d, string value)
        {
            d.SetValue(PreviousTextProperty, value);
        }

        private static string GetPreviousText(DependencyObject d)
        {
            return (string)d.GetValue(PreviousTextProperty);
        }



        private static DependencyProperty PreviousSelectedIndexProperty
            = DependencyProperty.RegisterAttached("PreviousSelectedIndex", typeof(int), typeof(TextBoxExtension));

        private static void SetPreviousSelectedIndex(DependencyObject d, int value)
        {
            d.SetValue(PreviousSelectedIndexProperty, value);
        }

        private static int GetPreviousSelectedIndex(DependencyObject d)
        {
            return (int)d.GetValue(PreviousSelectedIndexProperty);
        }
    }
}
