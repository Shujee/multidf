using System.Linq;
using System.Windows;

namespace DuplicateFinderMultiKeyGen
{
  /// <summary>
  /// Interaction logic for App.xaml
  /// </summary>
  public partial class App : Application
  {
    private string _ProductName;
    public string ProductName => _ProductName;

    public App()
    {
      //We'll fetch product name from the Title attribute of the running assembly. (note this doesn't work correctly when called in VS Extension's context, so VS Extension
      //sets it directly by calling the setter of this property).
      var ExecutingAssembly = System.Reflection.Assembly.GetExecutingAssembly();

      if (ExecutingAssembly != null)
      {
        //The following statement returns the Title attribute of the entry assembly, as defined in project properties (Assembly Information dialog).
        var ProductNameAttrib = ExecutingAssembly.GetCustomAttributesData().First(x => x.AttributeType.Name == "AssemblyProductAttribute");

        if (ProductNameAttrib != null)
          _ProductName = ProductNameAttrib.ConstructorArguments[0].Value.ToString();
        else
          _ProductName = "DuplicateFinderMultiKeyGen";
      }
    }
  }
}
