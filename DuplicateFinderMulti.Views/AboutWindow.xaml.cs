﻿using System.Windows;

namespace DuplicateFinderMulti.Views
{
  /// <summary>
  /// Interaction logic for MainWindow.xaml
  /// </summary>
  public partial class AboutWindow : Window
  {
    public AboutWindow()
    {
      InitializeComponent();
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
      this.DialogResult = true;
      this.Close();
    }
  }
}