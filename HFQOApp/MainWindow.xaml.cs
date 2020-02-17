using GalaSoft.MvvmLight.Ioc;
using HFQOViews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Shapes;
using VMBase;

namespace HFQOApp
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window
  {
    //Word's Point (1/72 inch) to Point to WPF's Pixel (1/96 inch) ratio.
    const float POINT2PIXEL = 96f / 72f;

    private readonly DialogPresenter DLG = new DialogPresenter("HFQ Client App");

    //This list keeps track of FixedPage objects. Otherwise, they're (probably) collected by GC nad 
    //Loaded event used in the loop below is never fired.
    List<FixedPage> Pages = new List<FixedPage>();

    public MainWindow()
    {
      SimpleIoc.Default.Register<Common.IDialogService>(() => DLG);
      SimpleIoc.Default.Register<HFQOVM.IDialogService>(() => DLG);

      InitializeComponent();

      this.Title = $"HFQApp (ver: { System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString()})";

      DV.FitToWidth();

      var VM = this.DataContext as HFQOVM.HFQVM;
      VM.PropertyChanged += (sender2, e2) =>
      {
        if (e2.PropertyName == nameof(HFQOVM.HFQVM.XPSPath) && !string.IsNullOrEmpty(VM.XPSPath))
        {
          AddWatermark(new string[] { ViewModelLocatorBase.Auth.User.name, ViewModelLocatorBase.Auth.Email } );
        }
      };    

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

    private void AddWatermark(string[] text)
    {
      var fds = DV.Document as FixedDocumentSequence;
      DocumentReference docReference = fds.References.First();
      FixedDocument fd = docReference.GetDocument(false);

      Pages.Clear();
      for (int i = 0; i < fd.Pages.Count; i++)
      {
        var HighlightPage = fd.Pages[i].GetPageRoot(false);
        Pages.Add(HighlightPage);

        HighlightPage.Loaded += (sender, e) =>
        {
          TextBlock TB = new TextBlock();

          foreach (var L in text)
          {
            TB.Inlines.Add(new Run(L));
            TB.Inlines.Add(new LineBreak());
          }

          var HighlightPage2 = sender as FixedPage;

          TB.RenderTransformOrigin = new Point(.5, .5);
          TB.Opacity = .3;
          TB.Foreground = Brushes.LightGray;
          TB.FontFamily = new FontFamily("Arial Black");
          TB.TextAlignment = TextAlignment.Center;
          TB.RenderTransform = new RotateTransform(-Math.Atan(HighlightPage2.ActualWidth / HighlightPage2.ActualHeight) * (180.0 / Math.PI));

          Rectangle WatermarkRect = new Rectangle();
          WatermarkRect.Fill = new VisualBrush(TB)
          {
            TileMode = TileMode.Tile,
            Stretch = Stretch.Uniform,
            Viewport = new Rect(0, 0, 120, 120),
            Viewbox = new Rect(-0.1, -0.1, 1.2, 1.2),
            ViewportUnits = BrushMappingMode.Absolute
          };

          WatermarkRect.Width = HighlightPage2.Width * (1 / POINT2PIXEL);
          WatermarkRect.Height = HighlightPage2.Height * (1 / POINT2PIXEL);

          (HighlightPage2.Children[0] as Canvas).Children.Add(WatermarkRect);
        };
      }
    }
  }
}