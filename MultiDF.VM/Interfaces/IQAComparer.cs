using VMBase;

namespace MultiDF.VM
{
  public interface IQAComparer
  {
    /// <summary>
    /// Weight of the Choices section when computing distance between two QAs. Must return value between 0 and 1.
    /// </summary>
    double ChoiceSectionWeightage { get; }

    /// <summary>
    /// Computes the distance between two QA instances. Returns a value between 0 and 1.
    /// </summary>
    /// <param name="q1"></param>
    /// <param name="q2"></param>
    /// <param name="ignoreCase"></param>
    /// <returns></returns>
    double Distance(QA q1, QA q2, bool ignoreCase);
  }
}