using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  internal class CameraService : ICameraService
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
  }
}