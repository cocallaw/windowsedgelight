using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Forms;
using System.IO;
using WindowsEdgeLight.Properties;

namespace WindowsEdgeLight;

public partial class MainWindow : Window
{
    private bool isLightOn = true;
    private double currentOpacity = 1.0;  // Full brightness by default
    private int currentTemperature = 6500; // Default cool white (6500K)
    private const double OpacityStep = 0.15;
    private const double MinOpacity = 0.2;
    private const double MaxOpacity = 1.0;
    private const int MinTemperature = 2700; // Warm white
    private const int MaxTemperature = 6500; // Cool white
    private const int TemperatureStep = 100;
    
    private NotifyIcon? notifyIcon;
    private ControlWindow? controlWindow;

    // Monitor management
    private int currentMonitorIndex = 0;
    private Screen[] availableMonitors = Array.Empty<Screen>();

    // Global hotkey IDs
    private const int HOTKEY_TOGGLE = 1;
    private const int HOTKEY_BRIGHTNESS_UP = 2;
    private const int HOTKEY_BRIGHTNESS_DOWN = 3;

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);
    private const int SM_CXSCREEN = 0;
    private const int SM_CYSCREEN = 1;

    [DllImport("user32.dll")]
    private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);
    
    [DllImport("user32.dll")]
    private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

    private const uint MOD_CONTROL = 0x0002;
    private const uint MOD_SHIFT = 0x0004;
    private const uint VK_L = 0x4C;
    private const uint VK_UP = 0x26;
    private const uint VK_DOWN = 0x28;

    public MainWindow()
    {
        InitializeComponent();
        SetupNotifyIcon();
    }

    private void SetupNotifyIcon()
    {
        notifyIcon = new NotifyIcon();
        
        // Load icon from embedded resource or file
        try
        {
            var iconPath = Path.Combine(AppContext.BaseDirectory, "ringlight_cropped.ico");
            if (File.Exists(iconPath))
            {
                notifyIcon.Icon = new System.Drawing.Icon(iconPath);
            }
            else
            {
                // Try application icon from exe
                var appIcon = System.Drawing.Icon.ExtractAssociatedIcon(Environment.ProcessPath ?? System.Reflection.Assembly.GetExecutingAssembly().Location);
                notifyIcon.Icon = appIcon ?? System.Drawing.SystemIcons.Application;
            }
        }
        catch (Exception)
        {
            // Fallback to default icon if loading fails
            notifyIcon.Icon = System.Drawing.SystemIcons.Application;
        }
        
        notifyIcon.Text = "Windows Edge Light - Right-click for options";
        notifyIcon.Visible = true;
        
        var contextMenu = new ContextMenuStrip();
        contextMenu.Items.Add("📋 Keyboard Shortcuts", null, (s, e) => ShowHelp());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("💡 Toggle Light (Ctrl+Shift+L)", null, (s, e) => ToggleLight());
        contextMenu.Items.Add("🔆 Brightness Up (Ctrl+Shift+↑)", null, (s, e) => IncreaseBrightness());
        contextMenu.Items.Add("🔅 Brightness Down (Ctrl+Shift+↓)", null, (s, e) => DecreaseBrightness());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("🌡️ Temperature: Warmer", null, (s, e) => DecreaseTemperature());
        contextMenu.Items.Add("🌡️ Temperature: Cooler", null, (s, e) => IncreaseTemperature());
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("📌 Load Preset 1", null, (s, e) => LoadPreset(1));
        contextMenu.Items.Add("📌 Load Preset 2", null, (s, e) => LoadPreset(2));
        contextMenu.Items.Add("📌 Load Preset 3", null, (s, e) => LoadPreset(3));
        contextMenu.Items.Add(new ToolStripSeparator());
        contextMenu.Items.Add("✖ Exit", null, (s, e) => System.Windows.Application.Current.Shutdown());
        
        notifyIcon.ContextMenuStrip = contextMenu;
        notifyIcon.DoubleClick += (s, e) => ShowHelp();
    }

    private void ShowHelp()
    {
        var version = System.Reflection.Assembly.GetExecutingAssembly()
            .GetName().Version?.ToString() ?? "Unknown";
        
        var helpMessage = $@"Windows Edge Light - Keyboard Shortcuts

💡 Toggle Light:  Ctrl + Shift + L
🔆 Brightness Up:  Ctrl + Shift + ↑
🔅 Brightness Down:  Ctrl + Shift + ↓

🌡️ Temperature Control:
• Use the slider in the control panel
• Or right-click tray icon for quick adjustments

📌 Presets:
• Click preset buttons (1, 2, 3) to load
• Ctrl + Click to save current settings
• Presets save both temperature and brightness

💡 Features:
• Click-through overlay - won't interfere with your work
• Global hotkeys work from any application
• Right-click taskbar icon for menu
• Settings persist between sessions

Created by Scott Hanselman
Version {version}";

        System.Windows.MessageBox.Show(helpMessage, "Windows Edge Light - Help", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SetupWindow()
    {
        // Initialize available monitors on first setup
        if (availableMonitors.Length == 0)
        {
            availableMonitors = Screen.AllScreens;
            
            // Find the primary monitor index
            for (int i = 0; i < availableMonitors.Length; i++)
            {
                if (availableMonitors[i].Primary)
                {
                    currentMonitorIndex = i;
                    break;
                }
            }
        }

        var targetScreen = availableMonitors.Length > 0 ? availableMonitors[currentMonitorIndex] : Screen.PrimaryScreen;
        if (targetScreen == null) return;

        SetupWindowForScreen(targetScreen);
    }

    private void SetupWindowForScreen(Screen screen)
    {
        // Use WorkingArea instead of Bounds to exclude taskbar
        var workingArea = screen.WorkingArea;
        
        // Get DPI scale factor
        var source = PresentationSource.FromVisual(this);
        double dpiScaleX = 1.0;
        double dpiScaleY = 1.0;
        
        if (source != null)
        {
            dpiScaleX = source.CompositionTarget.TransformToDevice.M11;
            dpiScaleY = source.CompositionTarget.TransformToDevice.M22;
        }
        
        // Convert physical pixels to WPF DIPs
        this.Left = workingArea.X / dpiScaleX;
        this.Top = workingArea.Y / dpiScaleY;
        this.Width = workingArea.Width / dpiScaleX;
        this.Height = workingArea.Height / dpiScaleY;
        this.WindowState = System.Windows.WindowState.Normal;
    }

    private void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Load saved settings
        LoadSettings();
        
        SetupWindow();
        CreateFrameGeometry();
        CreateControlWindow();
        
        var hwnd = new WindowInteropHelper(this).Handle;
        int extendedStyle = GetWindowLong(hwnd, GWL_EXSTYLE);
        SetWindowLong(hwnd, GWL_EXSTYLE, extendedStyle | WS_EX_TRANSPARENT | WS_EX_LAYERED);
        
        // Register global hotkeys
        RegisterHotKey(hwnd, HOTKEY_TOGGLE, MOD_CONTROL | MOD_SHIFT, VK_L);
        RegisterHotKey(hwnd, HOTKEY_BRIGHTNESS_UP, MOD_CONTROL | MOD_SHIFT, VK_UP);
        RegisterHotKey(hwnd, HOTKEY_BRIGHTNESS_DOWN, MOD_CONTROL | MOD_SHIFT, VK_DOWN);
        
        // Hook into Windows message processing
        HwndSource source = HwndSource.FromHwnd(hwnd);
        source.AddHook(HwndHook);
        
        // Listen for window size/location changes (docking/undocking)
        this.SizeChanged += Window_SizeChanged;
        this.LocationChanged += Window_LocationChanged;
        
        // Apply initial temperature and brightness
        UpdateLightColor();
        EdgeLightBorder.Opacity = currentOpacity;
    }

    private void CreateControlWindow()
    {
        controlWindow = new ControlWindow(this);
        RepositionControlWindow();
        controlWindow.Show();
    }

    private void CreateFrameGeometry()
    {
        // Get actual dimensions (accounting for margin)
        double width = this.ActualWidth - 40;  // 20px margin on each side
        double height = this.ActualHeight - 40;
        
        const double frameThickness = 80;
        const double outerRadius = 100;  // Extra rounded like macOS
        const double innerRadius = 60;   // Keep proportional
        
        // Outer rounded rectangle
        var outerRect = new RectangleGeometry(new Rect(0, 0, width, height), outerRadius, outerRadius);
        
        // Inner rounded rectangle
        var innerRect = new RectangleGeometry(
            new Rect(frameThickness, frameThickness, 
                    width - (frameThickness * 2), 
                    height - (frameThickness * 2)), 
            innerRadius, innerRadius);
        
        // Combine: outer minus inner = frame
        var frameGeometry = new CombinedGeometry(GeometryCombineMode.Exclude, outerRect, innerRect);
        
        EdgeLightBorder.Data = frameGeometry;
    }

    private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_HOTKEY = 0x0312;
        
        if (msg == WM_HOTKEY)
        {
            int hotkeyId = wParam.ToInt32();
            
            switch (hotkeyId)
            {
                case HOTKEY_TOGGLE:
                    ToggleLight();
                    handled = true;
                    break;
                case HOTKEY_BRIGHTNESS_UP:
                    IncreaseBrightness();
                    handled = true;
                    break;
                case HOTKEY_BRIGHTNESS_DOWN:
                    DecreaseBrightness();
                    handled = true;
                    break;
            }
        }
        
        return IntPtr.Zero;
    }

    protected override void OnClosed(EventArgs e)
    {
        // Save settings before closing
        SaveSettings();
        
        var hwnd = new WindowInteropHelper(this).Handle;
        UnregisterHotKey(hwnd, HOTKEY_TOGGLE);
        UnregisterHotKey(hwnd, HOTKEY_BRIGHTNESS_UP);
        UnregisterHotKey(hwnd, HOTKEY_BRIGHTNESS_DOWN);
        
        if (notifyIcon != null)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
        
        controlWindow?.Close();
        
        base.OnClosed(e);
    }

    private void Window_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
    {
        if (e.Key == Key.L && 
            (Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control && 
            (Keyboard.Modifiers & ModifierKeys.Shift) == ModifierKeys.Shift)
        {
            ToggleLight();
        }
        else if (e.Key == Key.Escape)
        {
            System.Windows.Application.Current.Shutdown();
        }
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        ToggleLight();
    }

    private void ToggleLight()
    {
        isLightOn = !isLightOn;
        EdgeLightBorder.Visibility = isLightOn ? Visibility.Visible : Visibility.Collapsed;
    }

    public void HandleToggle()
    {
        ToggleLight();
    }

    public void IncreaseBrightness()
    {
        currentOpacity = Math.Min(MaxOpacity, currentOpacity + OpacityStep);
        EdgeLightBorder.Opacity = currentOpacity;
        SaveSettings();
    }

    public void DecreaseBrightness()
    {
        currentOpacity = Math.Max(MinOpacity, currentOpacity - OpacityStep);
        EdgeLightBorder.Opacity = currentOpacity;
        SaveSettings();
    }

    public void MoveToNextMonitor()
    {
        // Refresh monitor list in case of hot-plug/unplug
        availableMonitors = Screen.AllScreens;

        if (availableMonitors.Length <= 1)
        {
            // Only one monitor, nothing to do
            return;
        }

        // Bounds check: if current monitor no longer exists, reset to primary
        if (currentMonitorIndex >= availableMonitors.Length)
        {
            // Find primary monitor again
            currentMonitorIndex = 0;
            for (int i = 0; i < availableMonitors.Length; i++)
            {
                if (availableMonitors[i].Primary)
                {
                    currentMonitorIndex = i;
                    break;
                }
            }
        }

        // Cycle to next monitor
        currentMonitorIndex = (currentMonitorIndex + 1) % availableMonitors.Length;
        var targetScreen = availableMonitors[currentMonitorIndex];

        // Reposition main window to new monitor
        SetupWindowForScreen(targetScreen);
        
        // Recreate the frame geometry for new dimensions
        CreateFrameGeometry();
        
        // Reposition control window to follow
        RepositionControlWindow();
    }

    private void RepositionControlWindow()
    {
        if (controlWindow == null) return;

        // Position at bottom center of main window
        controlWindow.Left = this.Left + (this.Width - controlWindow.Width) / 2;
        controlWindow.Top = this.Top + this.Height - controlWindow.Height - 124;
    }

    public bool HasMultipleMonitors()
    {
        // Refresh monitor count to handle hot-plug scenarios
        availableMonitors = Screen.AllScreens;
        return availableMonitors.Length > 1;
    }

    private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // Recreate geometry when window size changes (e.g., different monitor resolution)
        if (EdgeLightBorder != null)
        {
            CreateFrameGeometry();
        }
        
        // Reposition control window
        RepositionControlWindow();
        
        // Update which monitor we're actually on
        UpdateCurrentMonitorIndex();
    }

    private void Window_LocationChanged(object? sender, EventArgs e)
    {
        // Reposition control window when main window moves
        RepositionControlWindow();
        
        // Update which monitor we're actually on
        UpdateCurrentMonitorIndex();
    }

    private void UpdateCurrentMonitorIndex()
    {
        // Refresh monitor list
        availableMonitors = Screen.AllScreens;
        
        // Figure out which monitor we're actually on now
        var windowCenter = new System.Drawing.Point(
            (int)(this.Left + this.Width / 2),
            (int)(this.Top + this.Height / 2)
        );
        
        for (int i = 0; i < availableMonitors.Length; i++)
        {
            if (availableMonitors[i].Bounds.Contains(windowCenter))
            {
                currentMonitorIndex = i;
                break;
            }
        }
    }

    private void BrightnessUp_Click(object sender, RoutedEventArgs e)
    {
        IncreaseBrightness();
    }

    private void BrightnessDown_Click(object sender, RoutedEventArgs e)
    {
        DecreaseBrightness();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private const int GWL_EXSTYLE = -20;
    private const int WS_EX_TRANSPARENT = 0x00000020;
    private const int WS_EX_LAYERED = 0x00080000;

    [DllImport("user32.dll")]
    private static extern int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    private static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

    // Load settings from persistent storage
    private void LoadSettings()
    {
        currentTemperature = Settings.Default.LightTemperature;
        currentOpacity = Settings.Default.LightBrightness;
        
        // Ensure values are in valid range
        currentTemperature = Math.Clamp(currentTemperature, MinTemperature, MaxTemperature);
        currentOpacity = Math.Clamp(currentOpacity, MinOpacity, MaxOpacity);
    }

    // Save settings to persistent storage
    private void SaveSettings()
    {
        Settings.Default.LightTemperature = currentTemperature;
        Settings.Default.LightBrightness = currentOpacity;
        Settings.Default.Save();
    }

    // Convert temperature (Kelvin) to RGB color
    private Color TemperatureToColor(int temperature)
    {
        // Clamp temperature to valid range
        double temp = Math.Clamp(temperature, MinTemperature, MaxTemperature) / 100.0;
        
        double red, green, blue;

        // Calculate red
        if (temp <= 66)
        {
            red = 255;
        }
        else
        {
            red = temp - 60;
            red = 329.698727446 * Math.Pow(red, -0.1332047592);
            red = Math.Clamp(red, 0, 255);
        }

        // Calculate green
        if (temp <= 66)
        {
            green = temp;
            green = 99.4708025861 * Math.Log(green) - 161.1195681661;
            green = Math.Clamp(green, 0, 255);
        }
        else
        {
            green = temp - 60;
            green = 288.1221695283 * Math.Pow(green, -0.0755148492);
            green = Math.Clamp(green, 0, 255);
        }

        // Calculate blue
        if (temp >= 66)
        {
            blue = 255;
        }
        else if (temp <= 19)
        {
            blue = 0;
        }
        else
        {
            blue = temp - 10;
            blue = 138.5177312231 * Math.Log(blue) - 305.0447927307;
            blue = Math.Clamp(blue, 0, 255);
        }

        return Color.FromRgb((byte)red, (byte)green, (byte)blue);
    }

    // Update the light gradient based on current temperature
    private void UpdateLightColor()
    {
        Color baseColor = TemperatureToColor(currentTemperature);
        
        // Create gradient with slight variations for visual interest
        var gradient = new LinearGradientBrush();
        gradient.StartPoint = new Point(0, 0);
        gradient.EndPoint = new Point(1, 1);
        
        // Create variations by adjusting brightness
        Color lighterColor = Color.FromRgb(
            (byte)Math.Min(255, baseColor.R + 15),
            (byte)Math.Min(255, baseColor.G + 15),
            (byte)Math.Min(255, baseColor.B + 15)
        );
        Color darkerColor = Color.FromRgb(
            (byte)Math.Max(0, baseColor.R - 15),
            (byte)Math.Max(0, baseColor.G - 15),
            (byte)Math.Max(0, baseColor.B - 15)
        );
        
        gradient.GradientStops.Add(new GradientStop(baseColor, 0.0));
        gradient.GradientStops.Add(new GradientStop(darkerColor, 0.3));
        gradient.GradientStops.Add(new GradientStop(baseColor, 0.5));
        gradient.GradientStops.Add(new GradientStop(lighterColor, 0.7));
        gradient.GradientStops.Add(new GradientStop(baseColor, 1.0));
        
        EdgeLightBorder.Fill = gradient;
    }

    // Public methods for temperature control
    public void IncreaseTemperature()
    {
        currentTemperature = Math.Min(MaxTemperature, currentTemperature + TemperatureStep);
        UpdateLightColor();
        SaveSettings();
    }

    public void DecreaseTemperature()
    {
        currentTemperature = Math.Max(MinTemperature, currentTemperature - TemperatureStep);
        UpdateLightColor();
        SaveSettings();
    }

    public void SetTemperature(int temperature)
    {
        currentTemperature = Math.Clamp(temperature, MinTemperature, MaxTemperature);
        UpdateLightColor();
        SaveSettings();
    }

    public int GetTemperature()
    {
        return currentTemperature;
    }

    public int GetMinTemperature()
    {
        return MinTemperature;
    }

    public int GetMaxTemperature()
    {
        return MaxTemperature;
    }

    // Preset management methods
    public void SavePreset(int presetNumber)
    {
        switch (presetNumber)
        {
            case 1:
                Settings.Default.Preset1Temperature = currentTemperature;
                Settings.Default.Preset1Brightness = currentOpacity;
                break;
            case 2:
                Settings.Default.Preset2Temperature = currentTemperature;
                Settings.Default.Preset2Brightness = currentOpacity;
                break;
            case 3:
                Settings.Default.Preset3Temperature = currentTemperature;
                Settings.Default.Preset3Brightness = currentOpacity;
                break;
        }
        Settings.Default.Save();
    }

    public void LoadPreset(int presetNumber)
    {
        switch (presetNumber)
        {
            case 1:
                currentTemperature = Settings.Default.Preset1Temperature;
                currentOpacity = Settings.Default.Preset1Brightness;
                break;
            case 2:
                currentTemperature = Settings.Default.Preset2Temperature;
                currentOpacity = Settings.Default.Preset2Brightness;
                break;
            case 3:
                currentTemperature = Settings.Default.Preset3Temperature;
                currentOpacity = Settings.Default.Preset3Brightness;
                break;
        }
        
        // Ensure values are in valid range
        currentTemperature = Math.Clamp(currentTemperature, MinTemperature, MaxTemperature);
        currentOpacity = Math.Clamp(currentOpacity, MinOpacity, MaxOpacity);
        
        UpdateLightColor();
        EdgeLightBorder.Opacity = currentOpacity;
        SaveSettings();
    }
}