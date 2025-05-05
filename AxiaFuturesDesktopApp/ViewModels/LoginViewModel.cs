using AxiaFuturesDesktopApp.Services;
using AxiaFuturesDesktopApp.Views;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace AxiaFuturesDesktopApp.ViewModels
{
    public class LoginViewModel : INotifyPropertyChanged
    {
        private readonly AuthService _authService = new AuthService();
        private readonly PasswordBox _passwordBox;
        private string _username;
        private string _errorMessage;

        public LoginViewModel(PasswordBox passwordBox)
        {
            _passwordBox = passwordBox;
            LoginCommand = new RelayCommand(parameter => ExecuteLogin(), parameter => CanExecuteLogin(parameter));
        }

        public string Username
        {
            get => _username;
            set { _username = value; OnPropertyChanged(nameof(Username)); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(nameof(ErrorMessage)); }
        }

        public ICommand LoginCommand { get; }

        private bool CanExecuteLogin(object parameter)
        {
            return !string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(_passwordBox?.Password);
        }

        private async void ExecuteLogin()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrEmpty(Username) || string.IsNullOrEmpty(_passwordBox.Password))
            {
                ErrorMessage = "Preencha todos os campos.";
                return;
            }

            if (await _authService.LoginAsync(Username, _passwordBox.Password))
            {
                var mainWindow = new MainWindow();
                mainWindow.Show();
                Application.Current.Windows[0]?.Close();
            }
            else
            {
                ErrorMessage = "Credenciais incorretas ou falha na conexão.";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}