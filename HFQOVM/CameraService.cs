using AForge.Video;
using AForge.Video.DirectShow;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace HFQOVM
{
  class CameraService : ICameraService
  {
    private int conta = 0;
    private Bitmap lastframe;
    private VideoCaptureDevice cam;
    private CancellationTokenSource _CancelTokenSrc;
    private CancellationToken _Token;
    private bool _CamInitialized = false;

    /// <summary>
    /// Call this function once at application startup.
    /// </summary>
    /// <returns></returns>
    public bool InitCam()
    {
      var webCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);

      if (webCams.Count > 0)
      {
        cam = new VideoCaptureDevice(webCams[0].MonikerString);
        cam.VideoResolution = cam.VideoCapabilities[0];

        cam.VideoSourceError += cam_VideoSourceError;

        _CancelTokenSrc = new CancellationTokenSource();
        _Token = _CancelTokenSrc.Token;

        _CamInitialized = true;
      }
      else
      {
        _CamInitialized = false;
      }

      return _CamInitialized;
    }

    public Task<Bitmap> TakeCameraSnapshot()
    {
      if (!_CamInitialized)
        return Task.FromResult((Bitmap)null);
      else
      {
        //if (!_CancelTokenSrc.IsCancellationRequested)
        //  _CancelTokenSrc.Cancel();
        
        return Task.Run(() =>
        {
          _Token.ThrowIfCancellationRequested();

          // set NewFrame event handler
          cam.NewFrame += new NewFrameEventHandler(video_NewFrame);

          // start the video source
          cam.Start();

          while (lastframe == null)
          {
            Task.Delay(100).Wait();
            _Token.ThrowIfCancellationRequested();
          }

          return lastframe;
        }, _Token).ContinueWith(t =>
        {
          cam.SignalToStop();
          cam.WaitForStop();
          cam.Stop();

          // detach NewFrame event handler
          cam.NewFrame -= new NewFrameEventHandler(video_NewFrame);

          lastframe = null;

          if (t.IsFaulted || t.IsCanceled)
            return null;
          else
            return t.Result;
        });
      }
    }

    private void video_NewFrame(object sender, NewFrameEventArgs eventArgs)
    {
      // get new frame
      if (conta < 80)// delay to wait that the white balancing stabilizes
      {
        conta++;
        return;
      }

      if (lastframe != null)
        lastframe.Dispose();

      lastframe = (Bitmap)eventArgs.Frame.Clone();

      //since we are intreseted in single snapshots only, we'll detach event handler as soon as first frame is captured
      cam.NewFrame -= new NewFrameEventHandler(video_NewFrame);
    }

    private void cam_VideoSourceError(object sender, VideoSourceErrorEventArgs e)
    {
      ViewModelLocator.Logger.Error(e.Description);
      cam.NewFrame -= new NewFrameEventHandler(video_NewFrame);
      lastframe = null;
    }
  }
}