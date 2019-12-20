using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace DuplicateFinderMulti.Views
{
  /// <summary>
  /// This static class adds extension method to Window class to hide Minimize, Maximize and Close buttons
  /// </summary>
  internal static class WindowExtensions
  {
    private const int GWL_STYLE = -16,
                      WS_MAXIMIZEBOX = 0x10000,
                      WS_MINIMIZEBOX = 0x20000,
                      WS_SYSMENU = 0x80000;

    [DllImport("user32.dll")]
    extern private static int GetWindowLong(IntPtr hwnd, int index);

    [DllImport("user32.dll")]
    extern private static int SetWindowLong(IntPtr hwnd, int index, int value);

    internal static void HideMinimizeAndMaximizeButtons(this Window window)
    {
      IntPtr hwnd = new System.Windows.Interop.WindowInteropHelper(window).Handle;
      var currentStyle = GetWindowLong(hwnd, GWL_STYLE);

      SetWindowLong(hwnd, GWL_STYLE, (currentStyle & ~WS_MAXIMIZEBOX & ~WS_MINIMIZEBOX & ~WS_SYSMENU));
    }
  }
}
