using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace DuckNet.UI
{
    public partial class ToastWindow : Window
    {
        public ToastWindow(string message)
        {
            InitializeComponent();
            TxtMessage.Text = message;

            // Розміщення в правому нижньому куті
            var desktopWorkingArea = SystemParameters.WorkArea;
            this.Left = desktopWorkingArea.Right - this.Width - 10;
            this.Top = desktopWorkingArea.Bottom - this.Height - 10;

            // Автозакриття через 5 секунд
            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(5) };
            timer.Tick += (s, e) => { timer.Stop(); this.Close(); };
            timer.Start();
        }

        // Статичний метод для виклику
        public static void Show(string msg)
        {
            Application.Current.Dispatcher.Invoke(() =>
            {
                var toast = new ToastWindow(msg);
                toast.Show();
            });
        }
    }
}