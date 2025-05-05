using AxiaFuturesDesktopApp.Views;
using System.Windows;

namespace AxiaFuturesDesktopApp
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginWindow = new LoginWindow();
            loginWindow.Show();
        }
    }
}