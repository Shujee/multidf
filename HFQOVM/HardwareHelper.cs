using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace HFQOVM
{
  public class HardwareHelper : IHardwareHelper
  {
    private FilterInfoCollection webCams;
    private VideoCaptureDevice cam;
    private bool? cameraWorks = null;
    private Bitmap bmap;
    private static readonly object padlock = new object();
    private bool _CamInitialized = false;

    private void InitCam()
    {
      webCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);
      cam = new VideoCaptureDevice(webCams[0].MonikerString);
      cam.VideoResolution = cam.VideoCapabilities[0];

      _CamInitialized = true;
    }

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

    public Bitmap TakeCameraSnapshot()
    {
      if (!_CamInitialized)
        InitCam();

      lock (padlock)
      {
        cam.NewFrame += NewFrameTest;
        cam.VideoSourceError += ErrorTest;
        cam.Start();
        int attempts = 0;
        while (cameraWorks == null && attempts++ < 6)
        {
          Task.Delay(500 * attempts).Wait();
        }
        cam.NewFrame -= NewFrameTest;
        cam.VideoSourceError -= ErrorTest;
        cam.SignalToStop();
        cam.WaitForStop();
        bool retValue = cameraWorks ?? false;
        if (!(cameraWorks ?? false))
        {
          cameraWorks = null;
          return null;
        }
        else
        {
          cameraWorks = null;
        }
        return bmap;
      }
    }

    private void NewFrameTest(object sender, NewFrameEventArgs e)
    {
      bmap = e.Frame.Clone() as Bitmap;
      cameraWorks = true;
    }

    private void ErrorTest(object sender, VideoSourceErrorEventArgs e)
    {
      ViewModelLocator.Logger.Error(e.Description);
      cameraWorks = false;
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
