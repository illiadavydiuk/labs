using DuckNet.Data.Models;
using DuckNet.Services.Implementations;
using DuckNet.Services.Interfaces;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace DuckNet.UI
{
    public partial class MainWindow : Window
    {
        private readonly INetworkScanner _scanner;
        private readonly DeviceService _deviceService;
        private readonly AdapterService _adapterService;

        public MainWindow(INetworkScanner scanner, DeviceService deviceService, AdapterService adapterService)
        {
            InitializeComponent();
            _scanner = scanner;
            _deviceService = deviceService;
            _adapterService = adapterService;

            // Завантаження стартових даних
            UpdateDashboard();
        }

        // --- NAVIGATION ---
        private void HideAllViews()
        {
            ViewDashboard.Visibility = Visibility.Collapsed;
            ViewScanner.Visibility = Visibility.Collapsed;
            ViewEvents.Visibility = Visibility.Collapsed;
            ViewAdapters.Visibility = Visibility.Collapsed;
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e) { HideAllViews(); ViewDashboard.Visibility = Visibility.Visible; UpdateDashboard(); }
        private void Nav_Scanner_Click(object sender, RoutedEventArgs e) { HideAllViews(); ViewScanner.Visibility = Visibility.Visible; RefreshScannerGrid(); }
        private void Nav_Events_Click(object sender, RoutedEventArgs e) { HideAllViews(); ViewEvents.Visibility = Visibility.Visible; LoadEvents(); }
        private void Nav_Adapters_Click(object sender, RoutedEventArgs e) { HideAllViews(); ViewAdapters.Visibility = Visibility.Visible; BtnLoadAdapters_Click(null, null); }


        // --- DASHBOARD LOGIC ---
        private void UpdateDashboard()
        {
            var devices = _deviceService.GetAllDevices().ToList();
            var events = _deviceService.GetRecentEvents().ToList();

            DashTotalDevices.Text = devices.Count.ToString();
            DashOnlineDevices.Text = devices.Count(d => d.IsOnline).ToString();

            // Подій за сьогодні
            DashEventsCount.Text = events.Count(ev => ev.Timestamp.Date == DateTime.Today).ToString();

            GridDashEvents.ItemsSource = events.Take(10); // Топ 10 для головної
        }

        // --- SCANNER LOGIC ---
        private async void BtnScan_Click(object sender, RoutedEventArgs e)
        {
            string ipBase = TxtIpBase.Text.Trim();
            if (!ipBase.EndsWith(".")) ipBase += ".";

            BtnScan.IsEnabled = false;
            TxtScanStatus.Text = "⏳ Сканування...";
            this.Cursor = Cursors.Wait;

            try
            {
                var foundDevices = await _scanner.ScanNetworkAsync(ipBase);

                // Це також оновить журнал подій
                _deviceService.UpdateDevices(foundDevices);

                RefreshScannerGrid();
                TxtScanStatus.Text = $"✅ Завершено. Активних: {foundDevices.Count}";

                MessageBox.Show("Сканування завершено! Перевірте журнал подій.", "DuckNet");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка: " + ex.Message);
                TxtScanStatus.Text = "❌ Помилка";
            }
            finally
            {
                BtnScan.IsEnabled = true;
                this.Cursor = Cursors.Arrow;
            }
        }

        private void RefreshScannerGrid()
        {
            GridScanner.ItemsSource = _deviceService.GetAllDevices()
                                           .OrderByDescending(d => d.IsOnline)
                                           .ThenBy(d => d.IpAddress)
                                           .ToList();
        }

        // --- EVENTS LOGIC ---
        private void LoadEvents()
        {
            GridAllEvents.ItemsSource = _deviceService.GetRecentEvents();
        }

        // --- ADAPTERS LOGIC ---
        private void BtnLoadAdapters_Click(object sender, RoutedEventArgs e)
        {
            try { GridAdapters.ItemsSource = _adapterService.GetAdapters(); }
            catch { }
        }

        private async void BtnToggleAdapter_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is NetworkAdapterInfo adapter)
            {
                try
                {
                    bool newState = !adapter.IsEnabled;
                    this.Cursor = Cursors.Wait;
                    await Task.Run(() => _adapterService.ToggleAdapter(adapter.DeviceId, newState));
                    await Task.Delay(2000);
                    BtnLoadAdapters_Click(null, null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Запустіть програму від імені Адміністратора!\n" + ex.Message);
                }
                finally { this.Cursor = Cursors.Arrow; }
            }
        }
    }
}