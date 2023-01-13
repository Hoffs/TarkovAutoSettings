using System.Runtime.InteropServices;
using System.Text;

namespace TarkovAutoSettings;

public class WinApi
{
    public enum WinEventFlags : uint
    {
        WINEVENT_OUTOFCONTEXT = 0,
    }

    public enum WinEvents : uint
    {

        // HWND will be window handle
        EVENT_SYSTEM_FOREGROUND = 0x0003,
        EVENT_SYSTEM_MINIMIZEEND = 0x0017,
    }

    // Events
    public delegate void WinEventProc(IntPtr hWinEventHook, uint eventType, IntPtr hwnd, int idObject, int idChild, uint dwEventThread, uint dwmsEventTime);

    [DllImport("user32.dll")]

    public static extern IntPtr SetWinEventHook(WinEvents eventMin, WinEvents eventMax, IntPtr hmodWinEventProc, WinEventProc lpfnWinEventProc, uint idProcess, uint idThread, WinEventFlags dwFlags);

    // Window
    [DllImport("user32.dll")]
    public static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

    // Message loop
    [DllImport("user32.dll", SetLastError = true)]
    public static extern int GetMessage(out Message lpMsg, IntPtr hwnd, int wMsgFilterMin, int wMsgFilterMax);

    [DllImport("user32.dll")]
    public static extern int TranslateMessage(Message lpMsg);

    [DllImport("user32.dll")]
    public static extern int DispatchMessage(Message lpMsg);
}

// From System.Windows.Forms source
public struct Message
{
    IntPtr hWnd;
    int msg;
    IntPtr wparam;
    IntPtr lparam;
    IntPtr result;


    public IntPtr HWnd
    {
        get { return hWnd; }
        set { hWnd = value; }
    }

    public int Msg
    {
        get { return msg; }
        set { msg = value; }
    }

    public IntPtr WParam
    {
        get { return wparam; }
        set { wparam = value; }
    }

    public IntPtr LParam
    {
        get { return lparam; }
        set { lparam = value; }
    }

    public IntPtr Result
    {
        get { return result; }
        set { result = value; }
    }
}