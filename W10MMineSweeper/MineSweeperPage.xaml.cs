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
using Windows.UI.Core;
using System.ComponentModel;

namespace W10MMineSweeper
{
    public sealed partial class MineSweeperPage : Page, INotifyPropertyChanged
    {
    public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _mineCount;
        public int MineCount
        {
            get => _mineCount;
            set
            {
                _mineCount = value;
                OnPropertyChanged(nameof(MineCount));
            }
        }

        public MineSweeperPage()
        {
            this.InitializeComponent();
            _ = GridSizeDialogAsync();
        }

        private async Task GridSizeDialogAsync()
        {
            ContentDialog inputDialog = new ContentDialog()
            {
                Title = "Set Grid Size (Default is 13)",
                PrimaryButtonText = "OK",
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

            ContentDialogResult result = await inputDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                var textBox = (TextBox)((StackPanel)inputDialog.Content).Children[0];
                if (int.TryParse(textBox.Text, out int gridSize))
                {
                    InitializeGrid(gridSize);
                    AddMinesToGrid(gridSize);
                    AddBordersToGrid(gridSize);
                }
                else
                {
                    InitializeGrid(13); // Default if no input is put or if input is invalid
                    AddMinesToGrid(13);
                    AddBordersToGrid(13);
                }
            }
        }

        public class Cell
        {
            public int Row { get; set; }
            public int Column { get; set; }
        }

private List<Cell> cells;

private void InitializeGrid(int gridSize)
{
    SweepGrid.ColumnDefinitions.Clear();
    SweepGrid.RowDefinitions.Clear();
    cells = new List<Cell>();

    for (int i = 0; i < gridSize; i++)
    {
        SweepGrid.ColumnDefinitions.Add(new ColumnDefinition());
        SweepGrid.RowDefinitions.Add(new RowDefinition());
    }

    for (int row = 0; row < gridSize; row++)
    {
        for (int col = 0; col < gridSize; col++)
        {
            cells.Add(new Cell { Row = row, Column = col });
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
                    var border = new Border
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

        private void AddMinesToGrid(int gridSize)
        {
            Random random = new Random();
            int randomNumber = random.Next(0, gridSize);
            int mineCount = (int)(gridSize * gridSize * 0.20);
            MineCount = mineCount;

            if (MineCount > 0)
            {

            }
        }

        private void PlaceMines(int col, int row)
        {
            
        }

        private async void DisplayResetDialog()
        {
            ContentDialog ResetDialog = new ContentDialog
            {
                Title = "Do you want to reset?",
                Content = "Clicking 'Reset' will wipe any progress.",
                PrimaryButtonText = "Reset",
                CloseButtonText = "No"
            };

            ContentDialogResult result = await ResetDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(MainPage));
                Frame.Navigate(typeof(MineSweeperPage));
            }
        }

        private async void DisplayQuitDialog()
        {
            ContentDialog QuitDialog = new ContentDialog
            {
                Title = "Do you want to quit?",
                Content = "Progress will not be saved.",
                PrimaryButtonText = "Yes",
                CloseButtonText = "No"
            };

            ContentDialogResult result = await QuitDialog.ShowAsync();

            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void ReturnButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayQuitDialog();
        }

        private void RestartButton_Click(object sender, RoutedEventArgs e)
        {
            DisplayResetDialog();
        }
    }
}
