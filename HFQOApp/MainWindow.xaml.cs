using GalaSoft.MvvmLight.Ioc;
using MultiDF.VM;
using MultiDF.Views;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;

namespace HFQOApp
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    //Word's Point (1/72 inch) to Point to WPF's Pixel (1/96 inch) ratio.
    const float POINT2PIXEL = 96f / 72f;

    private Rectangle HighlightRect = new Rectangle();

    public MainWindow()
    {
      SimpleIoc.Default.Register<IDialogService, DialogPresenter>();

      InitializeComponent();

      DocumentViewer.FitToWidthCommand.Execute(null, DV);
      
      HighlightRect.Fill = new SolidColorBrush(Colors.SkyBlue);
      HighlightRect.Opacity = 0.5;
      HighlightRect.RenderTransform = new TranslateTransform(0, 0);
      
      Panel.SetZIndex(HighlightRect, 1000);

#if(DEBUG)
      //DV.Document = (FixedDocumentSequence)(new PathToFixedDocumentConverter().Convert(@"F:\Office\Larry Gong\\Analysis\Sample Question for DF Multi.xps", typeof(FixedDocumentSequence), null, null));
      //(this.DataContext as MainVM).XML = System.IO.File.ReadAllText(@"F:\Office\Larry Gong\\Analysis\Sample Question for DF Multi.xml").Deserialize<XMLDoc>();
#endif
    }

    private void HFQPane_QASelected(QA qa)
    {
      var fds = DV.Document as FixedDocumentSequence;
      DocumentReference docReference = fds.References.First();
      FixedDocument fd = docReference.GetDocument(false);
      var HighlightPage = fd.Pages[qa.StartPage - 1].GetPageRoot(false);

      var XPSPageHeight = fds.DocumentPaginator.PageSize.Height;

      //Set width binding to that of the parent FixedPage
      Binding WidthBinding = new Binding("Width");
      WidthBinding.RelativeSource = new RelativeSource(RelativeSourceMode.FindAncestor, typeof(FixedPage), 1);
      WidthBinding.Mode = BindingMode.OneWay;
      HighlightRect.SetBinding(Rectangle.WidthProperty, WidthBinding);

      //Remove rect from previous parent
      if (HighlightRect.Parent is Canvas C)
        C.Children.Remove(HighlightRect);

      //Add highlight rect to the QA page (FixedPage has only one child of type Canvas) and set its top, bottom and height to that of the QA
      (HighlightPage.Children[0] as Canvas).Children.Add(HighlightRect);
      Canvas.SetTop(HighlightRect, qa.StartY);
      Canvas.SetBottom(HighlightRect, qa.EndY);
      HighlightRect.Height = ((qa.EndPage - qa.StartPage) * XPSPageHeight + (qa.EndY - qa.StartY));

      //scroll to the position of QA
      DV.VerticalOffset = ((qa.StartPage - 1) * XPSPageHeight + qa.StartY * POINT2PIXEL) * (DV.Zoom / 100);

      //object findToolbar = DV.Template.FindName("PART_FindToolBarHost", DV);
      //FieldInfo findTextBoxFieldInfo = findToolbar.GetType().GetField("FindTextBox", BindingFlags.NonPublic | BindingFlags.Instance);
      //TextBox findTextBox = (TextBox)findTextBoxFieldInfo.GetValue(findToolbar);
      //findTextBox.Text = "TEXT";
    }
  }
}