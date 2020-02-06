using MultiDF.VM;
using System.Windows;
using System.Windows.Controls;

namespace MultiDF.Views
{
  public class MyTreeViewItemTemplateSelector : DataTemplateSelector
  {
    public DataTemplate SameDocTemplate { get; set; }
    public DataTemplate DifferentDocsTemplate { get; set; }

    public override DataTemplate SelectTemplate(object item, DependencyObject container)
    {
      if(item is DFResult dfr)
      {
        if (dfr.Doc1 == dfr.Doc2)
          return SameDocTemplate;
        else
          return DifferentDocsTemplate;
      }

      return base.SelectTemplate(item, container);
    }
  }
}
