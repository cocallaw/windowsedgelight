using System.Windows;
using System.Windows.Input;

namespace WindowsEdgeLight;

public partial class ControlWindow : Window
{
    private readonly MainWindow mainWindow;
    private bool isUpdatingSlider = false;

    public ControlWindow(MainWindow main)
    {
        InitializeComponent();
        mainWindow = main;
        
        // Disable switch monitor button if only one monitor
        UpdateMonitorButtonState();
        
        // Initialize temperature slider with current value
        TemperatureSlider.Value = mainWindow.GetTemperature();
        UpdateTemperatureDisplay();
    }

    private void UpdateMonitorButtonState()
    {
        SwitchMonitorButton.IsEnabled = mainWindow.HasMultipleMonitors();
    }

    private void UpdateTemperatureDisplay()
    {
        TemperatureValueText.Text = $"{(int)TemperatureSlider.Value}K";
    }

    private void BrightnessDown_Click(object sender, RoutedEventArgs e)
    {
        mainWindow.DecreaseBrightness();
    }

    private void BrightnessUp_Click(object sender, RoutedEventArgs e)
    {
        mainWindow.IncreaseBrightness();
    }

    private void Toggle_Click(object sender, RoutedEventArgs e)
    {
        mainWindow.HandleToggle();
    }

    private void SwitchMonitor_Click(object sender, RoutedEventArgs e)
    {
        mainWindow.MoveToNextMonitor();
    }

    private void Close_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private void TemperatureSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (isUpdatingSlider) return;
        
        mainWindow.SetTemperature((int)e.NewValue);
        UpdateTemperatureDisplay();
    }

    private void Preset1_Click(object sender, RoutedEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Save current settings to preset 1
            mainWindow.SavePreset(1);
            System.Windows.MessageBox.Show("Preset 1 saved!", "Preset Saved", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Load preset 1
            mainWindow.LoadPreset(1);
            UpdateSliderFromMainWindow();
        }
    }

    private void Preset2_Click(object sender, RoutedEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Save current settings to preset 2
            mainWindow.SavePreset(2);
            System.Windows.MessageBox.Show("Preset 2 saved!", "Preset Saved", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Load preset 2
            mainWindow.LoadPreset(2);
            UpdateSliderFromMainWindow();
        }
    }

    private void Preset3_Click(object sender, RoutedEventArgs e)
    {
        if (Keyboard.Modifiers == ModifierKeys.Control)
        {
            // Save current settings to preset 3
            mainWindow.SavePreset(3);
            System.Windows.MessageBox.Show("Preset 3 saved!", "Preset Saved", 
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        else
        {
            // Load preset 3
            mainWindow.LoadPreset(3);
            UpdateSliderFromMainWindow();
        }
    }

    private void UpdateSliderFromMainWindow()
    {
        isUpdatingSlider = true;
        TemperatureSlider.Value = mainWindow.GetTemperature();
        UpdateTemperatureDisplay();
        isUpdatingSlider = false;
    }
}
