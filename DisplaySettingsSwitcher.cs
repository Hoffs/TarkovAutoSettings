
using WindowsDisplayAPI;

namespace TarkovAutoSettings;

public class DisplaySettingsSwitcher : IDisposable
{

    private bool _isEnabled;
    private readonly Display _winDisplay;
    private readonly NvAPIWrapper.Display.DisplayDevice _nvDisplay;
    private readonly NvAPIWrapper.Display.DVCInformation _nvVibrance;

    private readonly int _vibranceOn;
    private readonly int _vibranceOff;
    private readonly DisplayGammaRamp _gammaRampOn;
    private readonly DisplayGammaRamp _gammaRampOff;

    public DisplaySettingsSwitcher(
        int vibranceOn,
        int vibranceOff,
        double brightnessOn,
        double contrastOn,
        double gammaOn,
        double brightnessOff,
        double contrastOff,
        double gammaOff)
    {
        _vibranceOn = vibranceOn;
        _vibranceOff = vibranceOff;
        _gammaRampOn = new DisplayGammaRamp(brightnessOn, contrastOn, gammaOn);
        _gammaRampOff = new DisplayGammaRamp(brightnessOff, contrastOff, gammaOff);

        _winDisplay = Display.GetDisplays().Single(d => d.DisplayScreen.IsPrimary);
        _nvDisplay = NvAPIWrapper.Display.DisplayDevice.GetGDIPrimaryDisplayDevice();
        _nvVibrance = _nvDisplay.Output.DigitalVibranceControl;

        Console.WriteLine($"Primary Windows Display: {_winDisplay.DeviceName}");
        Console.WriteLine($"Primary Nv Display: {_nvDisplay.Output}");
        Console.WriteLine($"Primary Nv Display Vibrance: {_nvDisplay.Output.DigitalVibranceControl}");
        Disable(true);
    }
    
    public void Set(bool isEnabled)
    {
        if (isEnabled)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }

    public void Enable(bool force = false)
    {
        if (_isEnabled && !force)
        {
            return;
        }

        _isEnabled = true;
        _winDisplay.GammaRamp = _gammaRampOn;
        _nvVibrance.CurrentLevel = _vibranceOn;

#if DEBUG
        LogDisplayStatus();
#endif
    }

    public void Disable(bool force = false)
    {
        if (!_isEnabled && !force)
        {
            return;
        }

        _isEnabled = false;
        _winDisplay.GammaRamp = _gammaRampOff;
        _nvVibrance.CurrentLevel = _vibranceOff;

#if DEBUG
        LogDisplayStatus();
#endif
    }
    
#if DEBUG
    private void LogDisplayStatus()
    {
        Console.WriteLine($"[{nameof(DisplaySettingsSwitcher)}] Display Status:");
        Console.WriteLine($"[{nameof(DisplaySettingsSwitcher)}] Enabled: {_isEnabled}");
        Console.WriteLine($"[{nameof(DisplaySettingsSwitcher)}] Vibrance: {_nvVibrance}");
    }
#endif

    public void Dispose()
    {
        Disable();
    }
}