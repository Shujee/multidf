using System;
using System.Runtime.InteropServices;

namespace MultiDF
{
  internal class NativeMethods
  {
    [DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
    internal static extern IntPtr GetForegroundWindow();
  }
}
