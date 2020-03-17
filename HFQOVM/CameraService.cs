using AForge.Video;
using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;

namespace HFQOVM
{
  class CameraService : ICameraService
  {
    private int conta = 0;
    private Bitmap lastframe;

    public object BestResCamera
    {
      get
      {
        var webCams = new FilterInfoCollection(FilterCategory.VideoInputDevice);

        if (webCams.Count > 0)
        {
          var cam = new VideoCaptureDevice(webCams[0].MonikerString);
          var Capabilities = cam.VideoCapabilities;

          if (Capabilities != null && Capabilities.Length > 0)
          {
            var BestFrameSize = Capabilities.Max(cap => cap.FrameSize.Width * cap.FrameSize.Height);
            cam.VideoResolution = Capabilities.First(cap => cap.FrameSize.Width * cap.FrameSize.Height == BestFrameSize);
            return cam;
          }
          else
            return null;
        }
        else
          return null;
      }
    }

    public Task<Bitmap> TakeCameraSnapshot()
    {
      var cam = BestResCamera as VideoCaptureDevice;

      if (cam == null)
        return Task.FromResult((Bitmap)null);
      else
      {
        return Task.Run(() =>
        {
          // set NewFrame event handler
          cam.NewFrame += new NewFrameEventHandler(video_NewFrame);

          // start the video source
          cam.Start();

          //wait for the frame to be captured (for 1 second max)
          int counter = 0;
          while (lastframe == null && counter++ < 15)
            Task.Delay(500).Wait();

          //stop cam
          cam.SignalToStop();
          cam.WaitForStop();

          // detach NewFrame event handler
          cam.NewFrame -= new NewFrameEventHandler(video_NewFrame);

          return lastframe;

        }).ContinueWith(t =>
        {
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
      {
        lastframe.Dispose();
        lastframe = null;
      }

      lastframe = (Bitmap)eventArgs.Frame.Clone();
    }
  }
}