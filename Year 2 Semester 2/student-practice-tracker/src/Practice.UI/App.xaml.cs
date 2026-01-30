using System.Windows;

namespace Practice.UI
{
    /// <summary>
    /// Логіка взаємодії для App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            MainWindow main = new MainWindow();
            main.Show();
        }
    }
}