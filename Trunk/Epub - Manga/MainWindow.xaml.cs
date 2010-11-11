using System.Windows;

namespace EpubManga
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new DataContext();
        }
    }

}
