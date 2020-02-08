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
      //scroll to the position of QA
      var fds = DV.Document as FixedDocumentSequence;
      var XPSPageHeight = fds.DocumentPaginator.PageSize.Height;
      DV.VerticalOffset = ((qa.StartPage - 1) * XPSPageHeight + qa.StartY * POINT2PIXEL) * (DV.Zoom / 100);
    }
  }
}