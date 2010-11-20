using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace EpubManga
{
    /// <summary>
    /// Extends TextBox framework element by adding a "Regexp" property, allowing to set a specific mask on the textbox text.
    /// </summary>
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

            switch (GetRegexp(textBox))
            {
                case Regexp.FileName:
                    SetPattern(textBox, "^[^:\\\\/*?<>\"\\|]*$");
                    break;
                case Regexp.Integer:
                    SetPattern(textBox, "^[0-9]*$");
                    break;
                case Regexp.NegativeInteger:
                    SetPattern(textBox, "^-?[0-9]*$");
                    break;
                default:
                    SetPattern(textBox, string.Empty);
                    break;
            }

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

            if (Regex.Match(textBox.Text, GetPattern(textBox)).Success)
            {
                SetPreviousText(textBox, textBox.Text);
            }
            else
            {
                int oldSelectionStart = textBox.SelectionStart;
                int oldTextLength = textBox.Text != null ? textBox.Text.Length : 0;

                textBox.TextChanged -= textBox_TextChanged;
                textBox.Text = GetPreviousText(textBox);
                textBox.TextChanged += textBox_TextChanged;

                textBox.SelectionStart = oldSelectionStart - (oldTextLength - (textBox.Text != null ? textBox.Text.Length : 0));
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



        private static DependencyProperty PatternProperty
            = DependencyProperty.RegisterAttached("Pattern", typeof(string), typeof(TextBoxExtension));

        private static void SetPattern(DependencyObject d, string value)
        {
            d.SetValue(PatternProperty, value);
        }

        private static string GetPattern(DependencyObject d)
        {
            return (string)d.GetValue(PatternProperty);
        }
    }
}
