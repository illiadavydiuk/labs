using DuckNet.Data.Entities;
using DuckNet.Data.Models;
using DuckNet.Repositories.Interfaces;
using DuckNet.Services.Helpers;
using DuckNet.Services.Implementations;
using DuckNet.Services.Interfaces;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

// Аліас для профілю
using AdapterProfile = DuckNet.Data.Entities.AdapterProfile;

namespace DuckNet.UI
{
    public partial class MainWindow : Window
    {
        private readonly INetworkScanner _scanner;
        private readonly DeviceService _deviceService;
        private readonly AdapterService _adapterService;
        private readonly IRepository<AdapterProfile> _profileRepo;
        private readonly SettingsService _settingsService;

        private DispatcherTimer _autoScanTimer;
        private bool _isMenuExpanded = true;
        private bool _isPinging = false;

        private List<NetworkAdapterInfo> _currentSystemAdapters;

        public MainWindow(INetworkScanner scanner, DeviceService deviceService, AdapterService adapterService, IRepository<AdapterProfile> profileRepo)
        {
            InitializeComponent();
            _scanner = scanner;
            _deviceService = deviceService;
            _adapterService = adapterService;
            _profileRepo = profileRepo;
            _settingsService = new SettingsService();

            // Підписка на тривоги (від батч-системи)
            _deviceService.OnSecurityAlert += (message) =>
            {
                Dispatcher.Invoke(() => ToastWindow.Show(message));
            };

            // Таймер
            _autoScanTimer = new DispatcherTimer();
            _autoScanTimer.Tick += async (s, e) => await RunScan();

            // Завантаження налаштувань
            ApplySettings();

            NetworkChange.NetworkAvailabilityChanged += (s, e) => RefreshAdaptersData();

            UpdateDashboard();
            LoadProfilesFromDb();
            RefreshAdaptersData();
        }

        private void ApplySettings()
        {
            var settings = _settingsService.CurrentSettings;
            TxtScanInterval.Text = settings.ScanIntervalSeconds.ToString();
            _autoScanTimer.Interval = TimeSpan.FromSeconds(settings.ScanIntervalSeconds);
            ChkAutoScan.IsChecked = settings.IsAutoScanEnabled;

            if (settings.IsAutoScanEnabled) _autoScanTimer.Start();
        }

        private void SaveAppSettings()
        {
            bool isAuto = ChkAutoScan.IsChecked == true;
            if (int.TryParse(TxtScanInterval.Text, out int seconds))
            {
                _settingsService.SaveSettings(isAuto, seconds);
            }
        }

        // --- DASHBOARD ---
        private void UpdateDashboard()
        {
            var devs = _deviceService.GetAllDevices().ToList();
            DashTotal.Text = devs.Count.ToString();
            DashOnline.Text = devs.Count(x => x.IsOnline).ToString();
            DashOffline.Text = devs.Count(x => !x.IsOnline).ToString();
            DrawGraph();
        }

        private void GraphCanvas_SizeChanged(object sender, SizeChangedEventArgs e) => DrawGraph();

        private void DrawGraph()
        {
            if (GraphCanvas.ActualWidth == 0 || GraphCanvas.ActualHeight == 0) return;
            var history = _deviceService.GetScanHistory().ToList();
            if (history.Count < 2) return;

            double width = GraphCanvas.ActualWidth;
            double height = GraphCanvas.ActualHeight;
            double stepX = width / (history.Count - 1);
            double maxVal = history.Max(h => h.DevicesFound);
            if (maxVal == 0) maxVal = 10;

            PointCollection points = new PointCollection();
            PointCollection fillPoints = new PointCollection();
            fillPoints.Add(new Point(0, height));

            for (int i = 0; i < history.Count; i++)
            {
                double x = i * stepX;
                double y = height - ((double)history[i].DevicesFound / (maxVal * 1.2) * height);
                Point p = new Point(x, y);
                points.Add(p);
                fillPoints.Add(p);
            }
            fillPoints.Add(new Point(width, height));

            ActivityGraphLine.Points = points;
            ActivityGraphFill.Points = fillPoints;
        }

        // --- SCANNER ---
        private async void BtnScan_Click(object sender, RoutedEventArgs e) => await RunScan();

        private async Task RunScan()
        {
            BtnScan.IsEnabled = false;
            this.Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                string ip = TxtIpBase.Text;
                if (!ip.EndsWith(".")) ip += ".";

                var d = await _scanner.ScanNetworkAsync(ip);
                _deviceService.UpdateDevices(d);

                GridScanner.ItemsSource = _deviceService.GetAllDevices();
                UpdateDashboard();
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
            finally { BtnScan.IsEnabled = true; this.Cursor = System.Windows.Input.Cursors.Arrow; }
        }

        // 🔥 Збереження Тексту (Custom Name)
        private void GridScanner_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                Dispatcher.InvokeAsync(() =>
                {
                    var device = e.Row.Item as Device;
                    if (device != null) _deviceService.UpdateDevice(device);
                });
            }
        }

        // 🔥 Збереження Галочки (Trusted) при кліку
        private void ChkTrusted_Click(object sender, RoutedEventArgs e)
        {
            if (sender is CheckBox cb && cb.DataContext is Device device)
            {
                // Примусово оновлюємо значення і зберігаємо
                device.IsTrusted = cb.IsChecked == true;
                _deviceService.UpdateDevice(device);
            }
        }

        // --- EVENTS EXPORT ---
        private void BtnExportEvents_Click(object sender, RoutedEventArgs e)
        {
            if (DateExport.SelectedDate == null) { MessageBox.Show("Виберіть дату."); return; }

            DateTime date = DateExport.SelectedDate.Value;
            var events = _deviceService.GetEventsByDate(date).ToList();

            if (events.Count == 0) { MessageBox.Show("Немає подій."); return; }

            SaveFileDialog sfd = new SaveFileDialog { Filter = "Text file (*.txt)|*.txt", FileName = $"Events_{date:yyyy-MM-dd}.txt" };
            if (sfd.ShowDialog() == true)
            {
                try
                {
                    using (StreamWriter sw = new StreamWriter(sfd.FileName))
                    {
                        sw.WriteLine($"DuckNet Log - {date:dd.MM.yyyy}");
                        sw.WriteLine("--------------------------------");
                        foreach (var ev in events) sw.WriteLine($"[{ev.Timestamp:HH:mm:ss}] {ev.Message}");
                    }
                    MessageBox.Show("Збережено!");
                }
                catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
            }
        }

        // --- SETTINGS ---
        private void SaveScanInterval_Click(object sender, RoutedEventArgs e)
        {
            if (int.TryParse(TxtScanInterval.Text, out int seconds) && seconds > 5)
            {
                _autoScanTimer.Interval = TimeSpan.FromSeconds(seconds);
                SaveAppSettings();
                MessageBox.Show($"Інтервал: {seconds} с");
            }
            else MessageBox.Show("Введіть число > 5");
        }

        private void ChkAutoScan_Checked(object sender, RoutedEventArgs e)
        {
            _autoScanTimer.Start();
            SaveAppSettings();
        }

        private void ChkAutoScan_Unchecked(object sender, RoutedEventArgs e)
        {
            _autoScanTimer.Stop();
            SaveAppSettings();
        }

        private void BtnClearHistory_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Очистити все?", "Увага", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _deviceService.ClearAllHistory();
                UpdateDashboard();
                GridScanner.ItemsSource = null;
                GridAllEvents.ItemsSource = null;
                GridHistory.ItemsSource = null;
            }
        }

        // --- ADAPTERS ---
        private void RefreshAdaptersData()
        {
            Dispatcher.Invoke(() =>
            {
                _currentSystemAdapters = _adapterService.GetAdapters();
                GridAdapters.ItemsSource = _currentSystemAdapters;
            });
        }

        private async void ToggleAdapter_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as CheckBox)?.DataContext is NetworkAdapterInfo adapter)
            {
                bool enable = (sender as CheckBox).IsChecked == true;
                if (string.IsNullOrEmpty(adapter.NetConnectionId)) return;

                this.Cursor = System.Windows.Input.Cursors.Wait;
                await Task.Run(() => _adapterService.ToggleAdapter(adapter.NetConnectionId, enable));
                // Чекаємо поки Windows оновить статус
                await Task.Delay(2000);
                RefreshAdaptersData();
                this.Cursor = System.Windows.Input.Cursors.Arrow;
            }
        }

        // --- PROFILES ---
        private void LoadProfilesFromDb()
        {
            ListProfiles.ItemsSource = _profileRepo.GetAll().ToList();
        }

        private void ListProfiles_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var profile = ListProfiles.SelectedItem as AdapterProfile;
            PanelProfileAdapters.Children.Clear();

            if (profile == null)
            {
                BtnSaveProfile.IsEnabled = false; BtnDeleteProfile.IsEnabled = false; BtnApplyProfile.IsEnabled = false;
                TxtProfName.Text = "";
                return;
            }

            TxtProfName.Text = profile.ProfileName;
            BtnSaveProfile.IsEnabled = true; BtnDeleteProfile.IsEnabled = true; BtnApplyProfile.IsEnabled = true;

            var activeAdapters = profile.ActiveAdaptersData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList();

            if (_currentSystemAdapters != null)
            {
                foreach (var sysAdapter in _currentSystemAdapters)
                {
                    if (string.IsNullOrEmpty(sysAdapter.NetConnectionId)) continue;
                    var cb = new CheckBox
                    {
                        Content = sysAdapter.NetConnectionId,
                        Tag = sysAdapter.NetConnectionId,
                        FontSize = 14,
                        Margin = new Thickness(0, 5, 0, 5),
                        IsChecked = activeAdapters.Contains(sysAdapter.NetConnectionId)
                    };
                    PanelProfileAdapters.Children.Add(cb);
                }
            }
        }

        private void BtnAddProfile_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(TxtProfName.Text))
            {
                _profileRepo.Add(new AdapterProfile { ProfileName = TxtProfName.Text, ActiveAdaptersData = "" });
                _profileRepo.Save(); LoadProfilesFromDb();
            }
        }

        private void BtnSaveProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = ListProfiles.SelectedItem as AdapterProfile;
            if (profile != null)
            {
                var list = new List<string>();
                foreach (var child in PanelProfileAdapters.Children)
                    if (child is CheckBox cb && cb.IsChecked == true) list.Add(cb.Tag.ToString());

                profile.ActiveAdaptersData = string.Join(",", list);
                profile.ProfileName = TxtProfName.Text;
                _profileRepo.Update(profile); _profileRepo.Save(); LoadProfilesFromDb();
                MessageBox.Show("Оновлено!");
            }
        }

        private void BtnDeleteProfile_Click(object sender, RoutedEventArgs e)
        {
            var p = ListProfiles.SelectedItem as AdapterProfile;
            if (p != null && MessageBox.Show("Видалити?", "Питання", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _profileRepo.Delete(p.Id); _profileRepo.Save(); LoadProfilesFromDb();
            }
        }

        private async void BtnApplyProfile_Click(object sender, RoutedEventArgs e)
        {
            var profile = ListProfiles.SelectedItem as AdapterProfile;
            if (profile == null) return;
            if (MessageBox.Show($"Застосувати '{profile.ProfileName}'?", "Увага", MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var active = profile.ActiveAdaptersData.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            this.Cursor = System.Windows.Input.Cursors.Wait;
            try
            {
                await Task.Run(() =>
                {
                    foreach (var a in _adapterService.GetAdapters())
                    {
                        if (string.IsNullOrEmpty(a.NetConnectionId)) continue;
                        _adapterService.ToggleAdapter(a.NetConnectionId, active.Contains(a.NetConnectionId));
                    }
                });
                await Task.Delay(3000); RefreshAdaptersData(); MessageBox.Show("Застосовано!");
            }
            catch (Exception ex) { MessageBox.Show("Помилка: " + ex.Message); }
            finally { this.Cursor = System.Windows.Input.Cursors.Arrow; }
        }

        // --- DIAGNOSTICS ---
        private async void BtnPing_Click(object sender, RoutedEventArgs e)
        {
            if (_isPinging) { _isPinging = false; (sender as Button).Content = "Ping"; return; }
            _isPinging = true; (sender as Button).Content = "Stop";
            string target = TxtPingTarget.Text;
            Ping p = new Ping();

            await Task.Run(async () =>
            {
                while (_isPinging)
                {
                    try
                    {
                        var r = await p.SendPingAsync(target, 2000);
                        Dispatcher.Invoke(() => { TxtPingResult.AppendText($"[{DateTime.Now:T}] {r.Status} ({r.RoundtripTime}ms)\n"); TxtPingResult.ScrollToEnd(); });
                        await Task.Delay(1000);
                    }
                    catch { _isPinging = false; }
                }
            });
            (sender as Button).Content = "Ping";
        }

        private async void BtnTraceroute_Click(object sender, RoutedEventArgs e)
        {
            TxtPingResult.AppendText($"🚀 Trace to {TxtPingTarget.Text}...\n");
            string target = TxtPingTarget.Text;

            await Task.Run(() =>
            {
                using (var pinger = new Ping())
                {
                    PingOptions options = new PingOptions(1, true);
                    Stopwatch sw = new Stopwatch();
                    byte[] buffer = new byte[32];
                    for (int ttl = 1; ttl <= 30; ttl++)
                    {
                        options.Ttl = ttl;
                        sw.Restart();
                        try
                        {
                            PingReply reply = pinger.Send(target, 1000, buffer, options);
                            sw.Stop();
                            long t = sw.ElapsedMilliseconds == 0 ? 1 : sw.ElapsedMilliseconds;
                            Dispatcher.Invoke(() => { TxtPingResult.AppendText($"{ttl}\t{t}ms\t{reply.Status}\t{reply.Address}\n"); TxtPingResult.ScrollToEnd(); });
                            if (reply.Status == IPStatus.Success) break;
                        }
                        catch (Exception ex) { Dispatcher.Invoke(() => TxtPingResult.AppendText($"{ttl}\tError: {ex.Message}\n")); }
                    }
                    Dispatcher.Invoke(() => TxtPingResult.AppendText("🏁 Done.\n"));
                }
            });
        }

        // --- NAVIGATION ---
        private void HideAll()
        {
            if (ViewDashboard == null) return;
            ViewDashboard.Visibility = Visibility.Collapsed;
            ViewScanner.Visibility = Visibility.Collapsed;
            ViewEvents.Visibility = Visibility.Collapsed;
            ViewAdapters.Visibility = Visibility.Collapsed;
            ViewDiagnostics.Visibility = Visibility.Collapsed;
            ViewSettings.Visibility = Visibility.Collapsed;
            ViewHistory.Visibility = Visibility.Collapsed;
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
        private void Nav_History_Click(object sender, RoutedEventArgs e) { HideAll(); ViewHistory.Visibility = Visibility.Visible; GridHistory.ItemsSource = _deviceService.GetScanHistory().OrderByDescending(h => h.ScanTime).ToList(); }
        private void Nav_Adapters_Click(object sender, RoutedEventArgs e) { HideAll(); ViewAdapters.Visibility = Visibility.Visible; RefreshAdaptersData(); }
        private void Nav_Diagnostics_Click(object sender, RoutedEventArgs e) { HideAll(); ViewDiagnostics.Visibility = Visibility.Visible; }
        private void Nav_Settings_Click(object sender, RoutedEventArgs e) { HideAll(); ViewSettings.Visibility = Visibility.Visible; }
    }
}