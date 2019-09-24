namespace DuplicateFinderMulti.VM
{
  public interface IQAComparer
  {
    double Distance(QA q1, QA q2, bool ignoreCase);
  }
}