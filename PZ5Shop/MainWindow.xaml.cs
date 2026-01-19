using System.Windows;
using PZ5Shop.ViewModels;

namespace PZ5Shop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }
    }
}
