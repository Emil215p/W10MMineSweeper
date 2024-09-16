using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace W10MMineSweeper
{

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MineSweeperPage));
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            CoreApplication.Exit();
        }

        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void HelpButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayHelpDialog();
        }
        private async void DisplayHelpDialog()
        {
            ContentDialog HelpDialog = new ContentDialog
            {
                Title = "To play",
                Content = "Click start and then click on a field, to mark a mine long click, if unsure if its a mine long click again.",
                CloseButtonText = "OK"
            };

            ContentDialogResult result = await HelpDialog.ShowAsync();
        }

        private void CreditsButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(CreditsPage));
        }
    }
}
