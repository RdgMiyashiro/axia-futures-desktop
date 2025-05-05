using AxiaFuturesDesktopApp.ViewModels;
using System.Windows;

namespace AxiaFuturesDesktopApp.Views
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