using System;
using System.Management;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace HFQOVM
{
  public class HardwareHelper : IHardwareHelper
  {
    [DllImport("PowrProf.dll")]
    private static extern bool GetPwrCapabilities(out SYSTEM_POWER_CAPABILITIES lpSystemPowerCapabilities);

    [Serializable]
    private struct SYSTEM_POWER_CAPABILITIES
    {
      [MarshalAs(UnmanagedType.I1)]
      public bool PowerButtonPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool SleepButtonPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool LidPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS1;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS2;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS3;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS4;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemS5;
      [MarshalAs(UnmanagedType.I1)]
      public bool HiberFilePresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool FullWake;
      [MarshalAs(UnmanagedType.I1)]
      public bool VideoDimPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool ApmPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool UpsPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool ThermalControl;
      [MarshalAs(UnmanagedType.I1)]
      public bool ProcessorThrottle;
      public byte ProcessorMinThrottle;
      public byte ProcessorMaxThrottle;
      [MarshalAs(UnmanagedType.I1)]
      public bool FastSystemS4;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public byte[] spare2;
      [MarshalAs(UnmanagedType.I1)]
      public bool DiskSpinDown;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
      public byte[] spare3;
      [MarshalAs(UnmanagedType.I1)]
      public bool SystemBatteriesPresent;
      [MarshalAs(UnmanagedType.I1)]
      public bool BatteriesAreShortTerm;
      [MarshalAs(UnmanagedType.ByValArray, SizeConst = 3)]
      public BATTERY_REPORTING_SCALE[] BatteryScale;
      public SYSTEM_POWER_STATE AcOnLineWake;
      public SYSTEM_POWER_STATE SoftLidWake;
      public SYSTEM_POWER_STATE RtcWake;
      public SYSTEM_POWER_STATE MinDeviceWakeState;
      public SYSTEM_POWER_STATE DefaultLowLatencyWake;
    }

    private struct BATTERY_REPORTING_SCALE
    {
      public UInt32 Granularity;
      public UInt32 Capacity;
    }

    private enum SYSTEM_POWER_STATE
    {
      PowerSystemUnspecified = 0,
      PowerSystemWorking = 1,
      PowerSystemSleeping1 = 2,
      PowerSystemSleeping2 = 3,
      PowerSystemSleeping3 = 4,
      PowerSystemHibernate = 5,
      PowerSystemShutdown = 6,
      PowerSystemMaximum = 7
    }

    public bool IsLaptop()
    {
      return SystemInformation.PowerStatus.BatteryChargeStatus != BatteryChargeStatus.NoSystemBattery || LidExists();
    }

    public bool IsVM()
    {
      using (var searcher = new System.Management.ManagementObjectSearcher("Select * from Win32_ComputerSystem"))
      {
        using (var items = searcher.Get())
        {
          foreach (var item in items)
          {
            string manufacturer = item["Manufacturer"].ToString().ToLower();
            if ((manufacturer == "microsoft corporation" && item["Model"].ToString().ToUpperInvariant().Contains("VIRTUAL"))
                || manufacturer.Contains("vmware")
                || item["Model"].ToString() == "VirtualBox")
            {
              return true;
            }
          }
        }
      }
      return false;
    }

    public bool HasMultipleScreens()
    {
      ManagementObjectSearcher searcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity where service =\"monitor\"");
      return searcher.Get().Count > 1;
    }

    private static bool LidExists()
    {
      SYSTEM_POWER_CAPABILITIES spc;
      _ = GetPwrCapabilities(out spc);
      return spc.LidPresent;
    }

    public bool IsWindows10()
    {
      return Environment.OSVersion.Version >= new Version(10, 0);
    }
  }
}