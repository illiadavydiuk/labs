using DuckNet.Data.Models;
using DuckNet.Services.Helpers; // Traceroute & ProfileManager
using DuckNet.Services.Implementations;
using DuckNet.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace DuckNet.UI
{
    public partial class MainWindow : Window
    {
        private readonly INetworkScanner _scanner;
        private readonly DeviceService _deviceService;
        private readonly AdapterService _adapterService;

        private DispatcherTimer _autoScanTimer;
        private bool _isMenuExpanded = true;

        // Список профілів з файлу
        private List<AdapterProfile> _profiles;
        // Список реальних адаптерів з системи
        private List<NetworkAdapterInfo> _systemAdapters;

        public MainWindow(INetworkScanner scanner, DeviceService deviceService, AdapterService adapterService)
        {
            InitializeComponent();
            _scanner = scanner;
            _deviceService = deviceService;
            _adapterService = adapterService;

            _autoScanTimer = new DispatcherTimer();
            _autoScanTimer.Interval = TimeSpan.FromMinutes(5);
            _autoScanTimer.Tick += async (s, e) => await RunScan();

            NetworkChange.NetworkAvailabilityChanged += (s, e) => RefreshAdaptersUI();
            NetworkChange.NetworkAddressChanged += (s, e) => RefreshAdaptersUI();

            UpdateDashboard();
            LoadProfilesUI(); // Завантажуємо профілі
        }

        // --- PROFILE MANAGER LOGIC (NEW) ---
        private void LoadProfilesUI()
        {
            _profiles = ProfileManager.LoadProfiles();
            ListProfiles.ItemsSource = _profiles.Select(p => p.Name).ToList();

            // Отримуємо реальні адаптери для відображення в списку
            _systemAdapters = _adapterService.GetAdapters();
        }

        private void ListProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (ListProfiles.SelectedIndex == -1) return;

            string selectedName = ListProfiles.SelectedItem.ToString();
            var profile = _profiles.FirstOrDefault(p => p.Name == selectedName);
            if (profile == null) return;

            PanelProfileAdapters.Children.Clear();
            PanelProfileAdapters.Children.Add(new TextBlock { Text = $"Адаптери для '{profile.Name}':", FontWeight = FontWeights.Bold, Margin = new Thickness(0, 0, 0, 10) });

            foreach (var sysAdapter in _systemAdapters)
            {
                if (string.IsNullOrEmpty(sysAdapter.NetConnectionId)) continue;

                var cb = new CheckBox
                {
                    Content = sysAdapter.NetConnectionId, // Назва (Wi-Fi, Ethernet)
                    Tag = sysAdapter.NetConnectionId,
                    Margin = new Thickness(0, 5, 0, 5),
                    FontSize = 14
                };

                // Якщо адаптер є в списку активних цього профілю - ставимо галочку
                if (profile.ActiveAdapters.Contains(sysAdapter.NetConnectionId))
                {
                    cb.IsChecked = true;
                }

                PanelProfileAdapters.Children.Add(cb);
            }

            BtnSaveProfile.IsEnabled = true;
            BtnApplyProfile.IsEnabled = true;
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ListProfiles.SelectedIndex == -1) return;
            string selectedName = ListProfiles.SelectedItem.ToString();
            var profile = _profiles.FirstOrDefault(p => p.Name == selectedName);

            // Оновлюємо список активних адаптерів на основі галочок
            profile.ActiveAdapters.Clear();
            foreach (var child in PanelProfileAdapters.Children)
            {
                if (child is CheckBox cb && cb.IsChecked == true)
                {
                    profile.ActiveAdapters.Add(cb.Tag.ToString());
                }
            }

            ProfileManager.SaveProfiles(_profiles);
            MessageBox.Show("Профіль збережено!");
        }

        private async void BtnApplyProfile_Click(object sender, RoutedEventArgs e)
        {
            if (ListProfiles.SelectedIndex == -1) return;
            string selectedName = ListProfiles.SelectedItem.ToString();
            var profile = _profiles.FirstOrDefault(p => p.Name == selectedName);

            if (MessageBox.Show($"Застосувати профіль '{selectedName}'?\nЦе увімкне вибрані адаптери та ВИМКНЕ всі інші.", "Підтвердження", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            this.Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                await Task.Run(() =>
                {
                    // Проходимо по всіх реальних адаптерах
                    foreach (var sysAdapter in _systemAdapters)
                    {
                        if (string.IsNullOrEmpty(sysAdapter.NetConnectionId)) continue;

                        bool shouldBeOn = profile.ActiveAdapters.Contains(sysAdapter.NetConnectionId);

                        // Якщо поточний стан відрізняється від бажаного - перемикаємо
                        // (Але краще просто форсувати, щоб напевно)
                        _adapterService.ToggleAdapter(sysAdapter.NetConnectionId, shouldBeOn);
                    }
                });

                await Task.Delay(3000); // Чекаємо
                RefreshAdaptersUI();
                MessageBox.Show("Профіль застосовано успішно!");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Помилка (потрібен Адмін): " + ex.Message);
            }
            finally { this.Cursor = System.Windows.Input.Cursors.Arrow; }
        }

        // --- CLEAR HISTORY ---
        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Ви впевнені? Це видалить ВСІ пристрої та події з бази.", "Очищення", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                _deviceService.ClearAllHistory();
                UpdateDashboard();
                GridScanner.ItemsSource = null;
                GridAllEvents.ItemsSource = null;
                MessageBox.Show("Базу очищено.");
            }
        }

        // --- DASHBOARD & GRAPH ---
        private void UpdateDashboard()
        {
            var devs = _deviceService.GetAllDevices();
            var events = _deviceService.GetRecentEvents().OrderByDescending(e => e.Timestamp).ToList();

            DashTotal.Text = devs.Count().ToString();
            DashOnline.Text = devs.Count(x => x.IsOnline).ToString();
            DashOffline.Text = devs.Count(x => !x.IsOnline).ToString();

            // Тепер на дашборді немає таблиці подій, бо вона в окремій вкладці. 
            // Але графік лишається.
            DrawGraph();
        }

        private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawGraph();

        private void DrawGraph()
        {
            if (GraphCanvas.ActualWidth == 0 || GraphCanvas.ActualHeight == 0) return;
            var events = _deviceService.GetRecentEvents().OrderBy(e => e.Timestamp).ToList();
            if (events.Count < 2) return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            double stepX = width / (events.Count - 1);
            PointCollection points = new PointCollection();
            PointCollection fillPoints = new PointCollection();
            fillPoints.Add(new Point(0, height));
            Random r = new Random();
            for (int i = 0; i < events.Count; i++)
            {
                double x = i * stepX;
                double yValue = r.Next(20, 80);
                double y = height - (yValue / 100 * height);
                Point p = new Point(x, y);
                points.Add(p);
                fillPoints.Add(p);
            }
            fillPoints.Add(new Point(width, height));
            ActivityGraphLine.Points = points;
            ActivityGraphFill.Points = fillPoints;
        }

        // --- NAVIGATION ---
        private void HideAll()
        {
            if (ViewDashboard == null) return;
            ViewDashboard.Visibility = Visibility.Collapsed;
            ViewScanner.Visibility = Visibility.Collapsed;
            ViewEvents.Visibility = Visibility.Collapsed; // Нова вкладка
            ViewAdapters.Visibility = Visibility.Collapsed;
            ViewDiagnostics.Visibility = Visibility.Collapsed;
            ViewSettings.Visibility = Visibility.Collapsed;
        }

        private void BtnCollapseMenu_Click(object sender, RoutedEventArgs e)
        {
            if (_isMenuExpanded) { ColMenu.Width = new GridLength(60); LogoPanel.Visibility = Visibility.Collapsed; }
            else { ColMenu.Width = new GridLength(260); LogoPanel.Visibility = Visibility.Visible; }
            _isMenuExpanded = !_isMenuExpanded;
        }

        private void Nav_Dashboard_Click(object sender, RoutedEventArgs e) { HideAll(); ViewDashboard.Visibility = Visibility.Visible; UpdateDashboard(); }
        private void Nav_Scanner_Click(object sender, RoutedEventArgs e) { HideAll(); ViewScanner.Visibility = Visibility.Visible; GridScanner.ItemsSource = _deviceService.GetAllDevices(); }
        private void Nav_Events_Click(object sender, RoutedEventArgs e) { HideAll(); ViewEvents.Visibility = Visibility.Visible; GridAllEvents.ItemsSource = _deviceService.GetRecentEvents(); }
        private void Nav_Adapters_Click(object sender, RoutedEventArgs e) { HideAll(); ViewAdapters.Visibility = Visibility.Visible; GridAdapters.ItemsSource = _adapterService.GetAdapters(); }
        private void Nav_Diagnostics_Click(object sender, RoutedEventArgs e) { HideAll(); ViewDiagnostics.Visibility = Visibility.Visible; }
        private void Nav_Settings_Click(object sender, RoutedEventArgs e) { HideAll(); ViewSettings.Visibility = Visibility.Visible; }

        // --- OTHERS ---
        private void RefreshAdaptersUI() { Dispatcher.Invoke(() => { if (ViewAdapters.Visibility == Visibility.Visible) GridAdapters.ItemsSource = _adapterService.GetAdapters(); }); }

        private async void ToggleAdapter_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is NetworkAdapterInfo adapter)
            {
                try
                {
                    bool enable = (sender as CheckBox).IsChecked == true;
                    if (string.IsNullOrEmpty(adapter.NetConnectionId)) return;
                    this.Cursor = System.Windows.Input.Cursors.Wait;
                    await Task.Run(() => _adapterService.ToggleAdapter(adapter.NetConnectionId, enable));
                    await Task.Delay(2000);
                    GridAdapters.ItemsSource = _adapterService.GetAdapters();
                }
                catch (Exception ex) { MessageBox.Show("Потрібен Адмін: " + ex.Message); (sender as CheckBox).IsChecked = !(sender as CheckBox).IsChecked; }
                finally { this.Cursor = System.Windows.Input.Cursors.Arrow; }
            }
        }

        private async void BtnScan_Click(object sender, RoutedEventArgs e) => await RunScan();
        private async Task RunScan()
        {
            BtnScan.IsEnabled = false; this.Cursor = System.Windows.Input.Cursors.Wait;
            try { string ip = TxtIpBase.Text; if (!ip.EndsWith(".")) ip += "."; var d = await _scanner.ScanNetworkAsync(ip); _deviceService.UpdateDevices(d); GridScanner.ItemsSource = _deviceService.GetAllDevices(); UpdateDashboard(); } catch { } finally { BtnScan.IsEnabled = true; this.Cursor = System.Windows.Input.Cursors.Arrow; }
        }
        private void ChkAutoScan_Checked(object sender, RoutedEventArgs e) => _autoScanTimer.Start();
        private void ChkAutoScan_Unchecked(object sender, RoutedEventArgs e) => _autoScanTimer.Stop();
        private void BtnSaveSettings_Click(object sender, RoutedEventArgs e) { if (int.TryParse(TxtScanInterval.Text, out int min)) { _autoScanTimer.Interval = TimeSpan.FromMinutes(min); MessageBox.Show("Збережено!"); } }

        private async void BtnPing_Click(object sender, RoutedEventArgs e) { try { Ping p = new Ping(); var r = await p.SendPingAsync(TxtPingTarget.Text); TxtPingResult.Text = $"Status: {r.Status}\nTime: {r.RoundtripTime}ms"; } catch (Exception ex) { TxtPingResult.Text = ex.Message; } }
        private async void BtnTraceroute_Click(object sender, RoutedEventArgs e) { TxtPingResult.Text = "⏳ Trace..."; try { var r = await TracerouteHelper.TraceRoute(TxtPingTarget.Text); foreach (var l in r) TxtPingResult.Text += l + "\n"; } catch (Exception ex) { TxtPingResult.Text += ex.Message; } }
    }
}