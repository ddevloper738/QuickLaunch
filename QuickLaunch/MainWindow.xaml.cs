using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Input;
using System.Windows.Media;
using Forms = System.Windows.Forms;

namespace QuickLaunch
{
    public partial class MainWindow : Window
    {
        private const int HotkeyId = 9001;
        private const uint ModControl = 0x0002;
        private const uint WmHotkey = 0x0312;
        private readonly List<LauncherItem> allItems = new List<LauncherItem>();
        private HwndSource source;
        private Forms.NotifyIcon trayIcon;
        private bool allowClose;

        public ObservableCollection<LauncherItem> Results { get; } = new ObservableCollection<LauncherItem>();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint modifiers, uint virtualKey);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            source = PresentationSource.FromVisual(this) as HwndSource;
            if (source != null)
            {
                source.AddHook(WindowMessageHook);
                RegisterHotKey(source.Handle, HotkeyId, ModControl, (uint)KeyInterop.VirtualKeyFromKey(Key.Space));
            }

            CreateTrayIcon();
            LoadItems();
            SearchBox.Focus();
        }

        private void LoadItems()
        {
            allItems.Clear();
            allItems.Add(new LauncherItem("Calculator", "Open the Windows calculator", "calc.exe", "=", "#FF4C9AFF"));
            allItems.Add(new LauncherItem("Notepad", "Open a plain text editor", "notepad.exe", "N", "#FF5EC6A8"));
            allItems.Add(new LauncherItem("Settings", "Open Windows settings", "ms-settings:", "⚙", "#FFFFA45B"));
            allItems.Add(new LauncherItem("File Explorer", "Browse files and folders", "explorer.exe", "▣", "#FF54B8E8"));
            allItems.Add(new LauncherItem("Desktop", "Open your desktop folder", Environment.GetFolderPath(Environment.SpecialFolder.Desktop), "D", "#FFB47CFF"));
            allItems.Add(new LauncherItem("Downloads", "Open your downloads folder", Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads"), "↓", "#FFFF6F91"));

            AddStartMenuItems(Environment.GetFolderPath(Environment.SpecialFolder.StartMenu));
            AddStartMenuItems(Environment.GetFolderPath(Environment.SpecialFolder.CommonStartMenu));
            ShowResults(string.Empty);
        }

        private void AddStartMenuItems(string root)
        {
            if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
                return;

            try
            {
                foreach (string shortcut in Directory.EnumerateFiles(root, "*.lnk", SearchOption.AllDirectories))
                {
                    string name = Path.GetFileNameWithoutExtension(shortcut);
                    if (!string.IsNullOrWhiteSpace(name) && allItems.All(item => !string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase)))
                        allItems.Add(new LauncherItem(name, "Application shortcut", shortcut, "A", "#FF7785FF"));
                    if (allItems.Count >= 120)
                        break;
                }
            }
            catch (UnauthorizedAccessException)
            {
                // Some Start Menu folders are protected; the built-in actions remain available.
            }
            catch (IOException)
            {
                // A disconnected or unavailable profile should not stop the launcher starting.
            }
        }

        private void ShowResults(string query)
        {
            Results.Clear();
            IEnumerable<LauncherItem> matches = allItems;
            if (!string.IsNullOrWhiteSpace(query))
            {
                string[] terms = query.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                matches = allItems.Where(item => terms.All(term =>
                    item.Name.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Description.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0))
                    .OrderBy(item => item.Name.IndexOf(query.Trim(), StringComparison.OrdinalIgnoreCase) == 0 ? 0 : 1)
                    .ThenBy(item => item.Name);
            }

            foreach (LauncherItem item in matches.Take(12))
                Results.Add(item);

            for (int i = 0; i < Results.Count; i++)
                Results[i].IsFirst = i == 0;

            EmptyState.Visibility = Results.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            StatusText.Text = Results.Count == 0 ? "Nothing found" : Results.Count + (Results.Count == 1 ? " result" : " results");
            if (Results.Count > 0)
                ResultsList.SelectedIndex = 0;
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            ShowResults(SearchBox.Text);
        }

        private void ResultsList_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            LauncherItem item = ResultsList.SelectedItem as LauncherItem;
            if (item != null)
                StatusText.Text = item.Description;
        }

        private void ResultsList_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LaunchSelected();
                e.Handled = true;
            }
        }

        private void ResultsList_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            LaunchSelected();
        }

        private void Window_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                HideLauncher();
                e.Handled = true;
            }
            else if (e.Key == Key.Enter && SearchBox.IsKeyboardFocusWithin)
            {
                LaunchSelected();
                e.Handled = true;
            }
        }

        private void LaunchSelected()
        {
            LauncherItem item = ResultsList.SelectedItem as LauncherItem;
            if (item == null)
                return;

            try
            {
                Process.Start(new ProcessStartInfo(item.Path) { UseShellExecute = true });
                HideLauncher();
            }
            catch (Exception ex) when (ex is InvalidOperationException || ex is System.ComponentModel.Win32Exception || ex is FileNotFoundException)
            {
                StatusText.Text = "Could not open " + item.Name;
            }
        }

        private IntPtr WindowMessageHook(IntPtr handle, int message, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (message == WmHotkey && wParam.ToInt32() == HotkeyId)
            {
                ToggleLauncher();
                handled = true;
            }
            return IntPtr.Zero;
        }

        private void ToggleLauncher()
        {
            if (IsVisible && IsActive)
                HideLauncher();
            else
            {
                Show();
                Activate();
                SearchBox.Focus();
                SearchBox.SelectAll();
            }
        }

        private void HideLauncher()
        {
            Hide();
            SearchBox.Clear();
        }

        private void CreateTrayIcon()
        {
            trayIcon = new Forms.NotifyIcon
            {
                Icon = System.Drawing.SystemIcons.Application,
                Text = "QuickLaunch",
                Visible = true
            };
            trayIcon.DoubleClick += delegate { ToggleLauncher(); };
            Forms.ContextMenuStrip menu = new Forms.ContextMenuStrip();
            menu.Items.Add("Show QuickLaunch", null, delegate { Show(); Activate(); });
            menu.Items.Add(new Forms.ToolStripSeparator());
            menu.Items.Add("Exit", null, delegate { allowClose = true; Close(); });
            trayIcon.ContextMenuStrip = menu;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            HideLauncher();
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            if (IsVisible)
                HideLauncher();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (!allowClose)
            {
                e.Cancel = true;
                HideLauncher();
                return;
            }

            if (source != null)
            {
                UnregisterHotKey(source.Handle, HotkeyId);
                source.RemoveHook(WindowMessageHook);
            }
            if (trayIcon != null)
            {
                trayIcon.Visible = false;
                trayIcon.Dispose();
                trayIcon = null;
            }
        }
    }

    public sealed class LauncherItem
    {
        public LauncherItem(string name, string description, string path, string glyph, string accentColor)
        {
            Name = name;
            Description = description;
            Path = path;
            Glyph = glyph;
            AccentColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString(accentColor));
        }

        public string Name { get; private set; }
        public string Description { get; private set; }
        public string Path { get; private set; }
        public string Glyph { get; private set; }
        public Brush AccentColor { get; private set; }
        public bool IsFirst { get; set; }
    }
}
