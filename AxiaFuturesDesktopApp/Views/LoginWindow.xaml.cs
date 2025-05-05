using AxiaFuturesDesktopApp.ViewModels;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AxiaFuturesDesktopApp.Views
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            DataContext = new LoginViewModel(PasswordBox);
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Focus();
                Keyboard.Focus(textBox);
            }
        }
    }
}