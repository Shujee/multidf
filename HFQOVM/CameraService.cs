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

    public bool InitCam()
    {
      webCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);

      if (webCams.Count > 0)
      {
        cam = new VideoCaptureDevice(webCams[0].MonikerString);
        cam.VideoResolution = cam.VideoCapabilities[0];

        cam.NewFrame += cam_NewFrame;
        cam.VideoSourceError += cam_VideoSourceError;

        _CamInitialized = true;
      }
      else
      {
        _CamInitialized = false;
      }

      return _CamInitialized;
    }

    public void StopCam()
    {
      cam.NewFrame -= cam_NewFrame;
      cam.VideoSourceError -= cam_VideoSourceError;
    }

    private bool _IsBusy = false;
    public Task<Bitmap> TakeCameraSnapshot()
    {
      if (_IsBusy)
      {
        cam.SignalToStop();        
        cam.WaitForStop();
      }

      _IsBusy = true;

      if (!_CamInitialized)
        InitCam();

      if (!_CamInitialized)
        return null;

      lock (padlock)
      {
        return Task.Run(() =>
        {
          cam.Start();

          int attempts = 0;
          while (cameraWorks == null && attempts++ < 6)
          {
            Task.Delay(200 * attempts).Wait();
          }

          cam.SignalToStop();
          cam.WaitForStop();

          if (!(cameraWorks ?? false))
          {
            cameraWorks = null;
            return null;
          }
          else
          {
            cameraWorks = true;
            return bmap;
          }
        }).ContinueWith(t =>
        {
          _IsBusy = false;
          return t.Result;
        });
      }
    }

    private void cam_NewFrame(object sender, NewFrameEventArgs e)
    {
      cameraWorks = true;
      bmap = e.Frame.Clone() as Bitmap;
    }

    private void cam_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
    {
      ViewModelLocator.Logger.Error(e.Description);
      cameraWorks = false;
    }
  }
}