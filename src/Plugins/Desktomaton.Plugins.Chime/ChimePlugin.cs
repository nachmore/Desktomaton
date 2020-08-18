using Desktomaton.PluginBase;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Desktomaton.Plugins.Chime
{
  public class ChimePlugin : DesktomatonTrigger
  {
    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool EnumThreadWindows(uint dwThreadId, EnumThreadWndProcDelegate lpfn, IntPtr lParam);

    public delegate bool EnumThreadWndProcDelegate(IntPtr hWnd, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    private int _chimeMainThreadId;
    private Dictionary<int, List<string>> _windows;

    // list of window titles that are always present (i.e. not meeting titles)
    private readonly string[] _titlesToIgnore = new string[]
    {
          "Chrome Legacy Window",
          "CiceroUIWndFrame",
          "Amazon Chime",
          "MediaContextNotificationWindow",
          "SystemResourceNotifyWindow",
          "MSCTFIME UI",
          ".NET-BroadcastEventWindow",
          "Default IME",
          "WISPTIS",
          "GDI+ Window",
          "EVRVideoHandler"
    };

    public override string Name => "☎ Chime from Amazon";

    [DesktomatonProperty(PrettyTitle = "Is In Meeting?")]
    public bool? IsInMeeting { get; set; }

#pragma warning disable 1998 // disable async method lacks await
    public override async Task<bool> EvaluateAsync()
    {
      if (IsInMeeting == null)
        throw new ArgumentException("Must set IsInMeeting...");

      _windows = new Dictionary<int, List<string>>();
      _chimeMainThreadId = -1;

      var processes = Process.GetProcessesByName("chime");

      foreach (var process in processes)
      {
        foreach (ProcessThread thread in process.Threads)
        {
          // meeting window will be top level, no need to iterate through children
          EnumThreadWindows((uint)thread.Id, new EnumThreadWndProcDelegate(EnumThreadWndProc), (IntPtr)thread.Id);
        }
      }

      var inMeeting = (_chimeMainThreadId > -1 ? (_windows.ContainsKey(_chimeMainThreadId) && _windows[_chimeMainThreadId].Count > 0) : false);

      return inMeeting == (bool)IsInMeeting;
    }

    /// <summary>
    /// There isn't a great (cheap) way to figure out if Chime is in a meeting, so
    /// we guess based on the windows Chime has. If it only has standard windows,
    /// then it isn't in a meeting - if there are extras... meeting!
    /// </summary>
    /// <param name="hWnd"></param>
    /// <param name="lParam"></param>
    /// <returns></returns>
    private bool EnumThreadWndProc(IntPtr hWnd, IntPtr lParam)
    {
      var threadId = lParam.ToInt32();

      // if we've already found the main thread, and this isn't a window in it
      // then there is no point continuing this particular callback
      if (_chimeMainThreadId > -1 && _chimeMainThreadId != threadId)
      {
        // returning false will terminate the enumeration for this thread
        return false;
      }

      var length = GetWindowTextLength(hWnd);
      var sb = new StringBuilder(length + 1);

      GetWindowText(hWnd, sb, sb.Capacity);

      if (!_windows.ContainsKey(threadId))
      {
        _windows[threadId] = new List<string>();
      }

      var windowTitle = sb.ToString();

      if (!string.IsNullOrEmpty(windowTitle))
      {
        // the thread that has a window with this title is the main thread
        // this will also be the thread that will have the meeting windows
        if (windowTitle.Contains("Amazon Chime"))
        {
          Debug.WriteLine($"*** Main Thread found: {threadId}");
          _chimeMainThreadId = threadId;
        }

        // Chime has a bunch of standard windows, so ignore those
        // Anything left over - is a meeting!
        if (!_titlesToIgnore.Any(s => windowTitle.Contains(s)))
        {
          _windows[threadId].Add(windowTitle);
          Debug.WriteLine($"*** {threadId} title: {windowTitle}");
        }
      }

      // continue the enumaration by returning true
      return true;
    }

  }
}
