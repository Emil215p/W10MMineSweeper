using System;
using Windows.ApplicationModel.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace W10MMineSweeper
{

    public sealed partial class MainPage : Page
    {
        public MainPage()
        {

            Uri imageUri = new Uri("ms-appx:///Assets/background.png");
            BitmapImage bitmapImage = new BitmapImage(imageUri);
            ImageBrush imageBrush = new ImageBrush
            {
                ImageSource = bitmapImage
            };

            Application.Current.Resources["ApplicationPageBackgroundThemeBrush"] = imageBrush;

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
            Frame.Navigate(typeof(SettingsPage));
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
                Content = "Click 'Start' and then define the grid you want. Left click or tap to reveal a field. Long press or if on PC right click to mark a field if you think it contains a mine, repeat to unmark. You win if all fields are revealed and all mines are marked.",
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
