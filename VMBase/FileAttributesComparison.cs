using System;

namespace VMBase
{
  public class FileAttributesComparison
  {
    public long Size1 { get; set; }
    public string Size1KB => Math.Ceiling(Size1 / 1024f).ToString() + "KB";

    public long Size2 { get; set; }
    public string Size2KB => Math.Ceiling(Size2 / 1024f).ToString() + "KB";

    public DateTime LastModified1 { get; set; }
    public DateTime LastModified2 { get; set; }
  }
}