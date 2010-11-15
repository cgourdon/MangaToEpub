using System.Windows;

namespace EpubManga
{
    public partial class PreviewWindow : Window
    {
        public PreviewWindow(UserInput userInput)
        {
            InitializeComponent();
            DataContext = new Preview(userInput);

            Closing += PreviewWindow_Closing;
        }

        private void PreviewWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ((Preview)DataContext).Dispose();
        }
    }
}
