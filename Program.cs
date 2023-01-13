using System.ComponentModel;
using System.Text;
using System.Threading.Channels;
using TarkovAutoSettings;

using var cts = new CancellationTokenSource();

var settingsPath = args.Length >= 2 ? args[1] : "settings.json";
var settings = await Settings.LoadAsync(settingsPath, cts.Token);
Console.WriteLine(settings);

using var switcher = new DisplaySettingsSwitcher(
    settings.VibranceOn,
    settings.VibranceOff,
    settings.BrightnessOn,
    settings.ContrastOn,
    settings.GammaOn,
    settings.BrightnessOff,
    settings.ContrastOff,
    settings.GammaOff
);

var channel = Channel.CreateBounded<(string, uint)>(new BoundedChannelOptions(1000) { FullMode = BoundedChannelFullMode.DropWrite });

void HandleWindowEvent(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime)
{
    if (eventType != 3)
    {
        throw new ArgumentException($"Received unexpected event: {eventType}");
    }

#if DEBUG
    Console.WriteLine($"Received event {eventType} for hwnd {hwnd}");
#endif

    const uint nChars = 2048;
    var windowText = new StringBuilder((int)nChars);
    var moduleWindowText = WinApi.GetWindowText(hwnd, windowText, (int)nChars);

    var windowName = windowText.ToString();
    channel.Writer.TryWrite((windowName, dwmsEventTime));

#if DEBUG
    Console.WriteLine($"Foreground window name: '{windowText}'");
#endif
}

var handlerDelegate = new WinApi.WinEventProc(HandleWindowEvent);
var hookThread = new Thread(() =>
{
#if DEBUG
    Console.WriteLine("Setting windows event hook");
#endif

    var response = WinApi.SetWinEventHook(WinApi.WinEvents.EVENT_SYSTEM_FOREGROUND, WinApi.WinEvents.EVENT_SYSTEM_FOREGROUND, IntPtr.Zero, handlerDelegate, 0, 0, WinApi.WinEventFlags.WINEVENT_OUTOFCONTEXT);
    if (response == 0)
    {
        throw new InvalidOperationException("Failed to set win hook");
    }

#if DEBUG
    Console.WriteLine("Starting winapi message loop");
#endif

    while (!cts.IsCancellationRequested)
    {
        var result = WinApi.GetMessage(out var msg, IntPtr.Zero, 0, 0);
        if (result == 0) break;
        if (result == -1) throw new Win32Exception();
        WinApi.TranslateMessage(msg);
        WinApi.DispatchMessage(msg);
    }
});

hookThread.Start();

Console.CancelKeyPress += (sender, evt) =>
{
    cts.Cancel();
    switcher.Disable();
};

Console.WriteLine("Starting to read foreground window events");

while (!cts.IsCancellationRequested)
{
    uint lastWindowChangeTime = 0;
    await foreach (var windowChange in channel.Reader.ReadAllAsync(cts.Token))
    {
        var (windowName, atTime) = windowChange;
        if (string.IsNullOrWhiteSpace(windowName)
            || windowName == "Task Switching")
        {
            // When alt-tabbing, task switching is a window that appears
            continue;
        }

#if DEBUG
        Console.WriteLine($"Read from channel: {windowName}/{atTime}");
#endif

        var tarkovActive = windowName == "EscapeFromTarkov";
        if (tarkovActive && atTime > lastWindowChangeTime)
        {
#if DEBUG
            Console.WriteLine("Enabling");
#endif
            switcher.Enable();
        }
        else
        {
#if DEBUG
            Console.WriteLine("Disabling");
#endif
            switcher.Disable();
        }
    }
}