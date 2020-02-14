using MultiDF.Views;
using MultiDF.VM;
using GalaSoft.MvvmLight.Ioc;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;

namespace MultiDF.TestingShell
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Fluent.RibbonWindow, IWordService
  {
    public string ActiveDocumentPath => throw new NotImplementedException();

    public MainWindow()
    {
      InitializeComponent();

      SimpleIoc.Default.Register<IDialogService, DialogPresenter>();
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
      ViewModelLocator.DialogServiceMultiDF.OpenAboutWindow();
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
      ViewModelLocator.DialogServiceMultiDF.OpenRegisterWindow();
      ShowHidePaneButton.Visibility = VM.ViewModelLocator.Register.IsRegistered ? Visibility.Visible : Visibility.Collapsed;
    }

    public string GetActiveDocumentText()
    {
      string Res = null;

      GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher.InvokeAsync(() =>
      {
        Res = TXT.Text;
      });

      return Res;
    }

    public List<WordParagraph> GetActiveDocumentParagraphs()
    {
      List<WordParagraph> Res = new List<WordParagraph>();

      string[] Data = null;

      GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher.InvokeAsync(() =>
      {
        Data = Regex.Split(TXT.Text, "(?<=[\r])");
      }).Wait();

      int Start = 0;
      for (int i = 0; i < Data.Length; i++)
      {
        Res.Add(new WordParagraph(Data[i], Start, Start + Data[i].Length, ParagraphType.Text, 0, 0, 0, 0));
        Start += Data[i].Length;
      }

      return Res;
    }

    public void SelectRange(int start, int end)
    {
      TXT.SelectionStart = start;
      TXT.SelectionLength = end - start;
    }

    public string GetRangeText(int start, int end)
    {
      string Res = null;

      GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher.InvokeAsync(() =>
      {
        Res = TXT.Text.Substring(start, end - start);
      }).Wait();

      return Res;
    }

    public int? GetActiveDocumentParagraphsCount()
    {
      int? Res = null;

      GalaSoft.MvvmLight.Threading.DispatcherHelper.UIDispatcher.InvokeAsync(() =>
      {
        Res = TXT.Text.Split("\r".ToCharArray()).Length;
      }).Wait();

      return Res;
    }

    private void DEFTXTButton_Click(object sender, RoutedEventArgs e)
    {
      TXT.Text = "Q1\rA quick brown fox A quick brown fox A quick brown fox A quick brown fox\rB\rC\r\rQ2\rA quick braun fixes A quick brown fox A quick brown fox A quick brown fox\rC\rB";
    }

    public void OpenDocument(string docPath, int? start, int? end)
    {
      throw new NotImplementedException();
    }

    public void ExportDocumentToFixedFormat(ExportFixedFormat format, string docPath, string outputPath, bool closeAfterDone)
    {
      throw new NotImplementedException();
    }

    public List<WordParagraph> GetDocumentParagraphs(string docPath, CancellationToken token, Action<int, int> progressCallback, bool closeAfterDone = true)
    {
      throw new NotImplementedException();
    }

    public void FixQANumbers(string docPath, List<WordParagraph> delimiterParagraphs, bool closeAfterDone)
    {
      throw new NotImplementedException();
    }

    public void CreateMergedDocument(string[] docs, string outputPath, bool closeAfterCreate)
    {
      throw new NotImplementedException();
    }

    private void ShowHidePane_Click(object sender, RoutedEventArgs e)
    {
      if (MV.Visibility == Visibility.Visible)
        MV.Visibility = Visibility.Collapsed;
      else
        MV.Visibility = Visibility.Visible;
    }
  }
}
