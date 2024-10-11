using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace W10MMineSweeper
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MineSweeperPage : Page
    {
        public MineSweeperPage()
        {
            this.InitializeComponent();
            _ = GridSizeDialogAsync();
        }


        private async Task GridSizeDialogAsync()
        {
            // Create the ContentDialog
            ContentDialog inputDialog = new ContentDialog()
            {
                Title = "Set Grid Size",
                PrimaryButtonText = "OK",
                SecondaryButtonText = "Cancel",
                Content = new StackPanel
                {
                    Children =
            {
                new TextBox
                {
                    Name = "gridSizeTextBox",
                    Header = "Grid Size",
                    PlaceholderText = "Enter grid size...",
                    InputScope = new InputScope
                    {
                        Names = { new InputScopeName { NameValue = InputScopeNameValue.Number } }
                    }
                }
            }
                }
            };

            // Show the dialog and wait for the result asynchronously
            ContentDialogResult result = await inputDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var textBox = (TextBox)((StackPanel)inputDialog.Content).Children[0];
                if (int.TryParse(textBox.Text, out int gridSize))
                {
                    InitializeGrid(gridSize);
                    AddBordersToGrid(gridSize);
                }
                else
                {
                    InitializeGrid(10); // Default value if parsing fails
                    AddBordersToGrid(10);
                }
            }
        }



        private void AddBordersToGrid(int gridSize)
        {
            SweepGrid.Children.Clear();
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    Border border = new Border
                    {
                        BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black),
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetRow(border, row);
                    Grid.SetColumn(border, col);
                    SweepGrid.Children.Add(border);
                }
            }
        }
        private void InitializeGrid(int gridSize)
        {
            SweepGrid.ColumnDefinitions.Clear();
            SweepGrid.RowDefinitions.Clear();
            for (int i = 0; i < gridSize; i++)
            {
                SweepGrid.ColumnDefinitions.Add(new ColumnDefinition());
                SweepGrid.RowDefinitions.Add(new RowDefinition());
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            Frame.Navigate(typeof(MainPage));
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}