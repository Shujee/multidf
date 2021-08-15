using System;

namespace VMBase
{
  /// <summary>
  /// Represents a logger that can log errors and information messages.
  /// </summary>
  public interface ILogger
  {
    void Info(string message);
    void Warn(Exception ex, string message);
    void Error(string message);
    void Error(Exception ex, string message);
  }
}