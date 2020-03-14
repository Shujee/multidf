using AForge.Video.DirectShow;
using System;
using System.Drawing;
using System.Threading.Tasks;

namespace HFQOVM
{
  public interface ICameraService
  {
    object BestResCamera { get; }
    Task<Bitmap> TakeCameraSnapshot();

    event Action SnapshotCaptured;
  }
}