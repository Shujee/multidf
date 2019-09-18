﻿using DuplicateFinderMulti.Views;
using DuplicateFinderMulti.VM;
using GalaSoft.MvvmLight.Ioc;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Windows;

namespace DuplicateFinderMultiTestingShell
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class MainWindow : Window, IWordService
  {
    public MainWindow()
    {
      InitializeComponent();

      if (!GalaSoft.MvvmLight.ViewModelBase.IsInDesignModeStatic)
        SimpleIoc.Default.Register<IWordService>(() => this);

      SimpleIoc.Default.Register<IDialogService, DialogPresenter>();
    }

    private void AboutButton_Click(object sender, RoutedEventArgs e)
    {
      ViewModelLocator.DialogService.OpenDiffWindow("The\nquick\nbrown\nfox\nover", "The\nfox\njumps\nover");

      //ViewModelLocator.DialogService.OpenAboutWindow();
    }

    private void RegisterButton_Click(object sender, RoutedEventArgs e)
    {
      ViewModelLocator.DialogService.OpenRegisterWindow();
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
        Res.Add(new WordParagraph() { Start = Start, End = Start + Data[i].Length, Text = Data[i] });
        Start += Data[i].Length;
      }

      return Res;
    }

    public void SelectRange(int start, int end)
    {
      GalaSoft.MvvmLight.Threading.DispatcherHelper.CheckBeginInvokeOnUI(() =>
      {
        TXT.SelectionStart = start;
        TXT.SelectionLength = end - start;
      });
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

    public List<WordParagraph> GetDocumentParagraphs(string docPath)
    {
      return new List<WordParagraph>();
    }

    public void OpenDocument(string docPath)
    {
      
    }
  }
}
