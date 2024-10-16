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
using Windows.UI.Xaml.Media.Imaging;

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
                        },
                        new TextBox
                        {
                            Name = "mineRatioTextBox",
                            Header = "Mine ratio (in % (0.xx or xxx), leave empty for 20%)",
                            PlaceholderText = "Enter mine ratio...",
                            InputScope = new InputScope
                            {
                                Names = { new InputScopeName { NameValue = InputScopeNameValue.Number } }
                            }
                        }
                    }
            }
        };

        ContentDialogResult result = await inputDialog.ShowAsync();
        System.Diagnostics.Debug.WriteLine("Dialog result: " + result);

        if (result == ContentDialogResult.Primary)
        {
            var textBoxSize = (TextBox)((StackPanel)inputDialog.Content).Children[0];
            var textBoxRatio = (TextBox)((StackPanel)inputDialog.Content).Children[1];

            if (int.TryParse(textBoxSize.Text, out int gridSize))
            {
                double mineRatio = 0.20; // Default mine ratio

                if (double.TryParse(textBoxRatio.Text, out double parsedRatio))
                {
                    mineRatio = parsedRatio / 100.0; // Convert percentage to decimal
                }

                System.Diagnostics.Debug.WriteLine("Grid size from user input: " + gridSize);
                System.Diagnostics.Debug.WriteLine("Mine ratio from user input: " + mineRatio);

                InitializeGrid(gridSize);
                AddBordersToGrid(gridSize);
                AddMinesToGrid(gridSize, mineRatio);
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Invalid input; defaulting to grid size 13");
                InitializeGrid(13);
                AddBordersToGrid(13);
                AddMinesToGrid(13, 0.20);
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Dialog dismissed without primary button click");
        }

        System.Diagnostics.Debug.WriteLine("Dialog processing complete");
        SweepGrid.UpdateLayout();
        System.Diagnostics.Debug.WriteLine("SweepGrid layout updated. Visibility: " + SweepGrid.Visibility);
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
                SweepGrid.ColumnDefinitions.Add(new ColumnDefinition() { Width = new GridLength(1, GridUnitType.Star) });
                SweepGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
            }
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    var cellBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Windows.UI.Colors.Gray),
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetRow(cellBorder, row);
                    Grid.SetColumn(cellBorder, col);
                    SweepGrid.Children.Add(cellBorder);
                    cells.Add(new Cell { Row = row, Column = col });
                }
            }
            SweepGrid.UpdateLayout();
            System.Diagnostics.Debug.WriteLine("Grid initialized and layout updated with size: " + gridSize);
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
            System.Diagnostics.Debug.WriteLine("Borders added to grid");
        }

        private void AddMinesToGrid(int gridSize, double mineRatio)
        {
            Random random = new Random();
            int mineCount = (int)(gridSize * gridSize * mineRatio);
            MineCount = mineCount;
            HashSet<(int, int)> minePositions = new HashSet<(int, int)>();

            while (minePositions.Count < MineCount)
            {
                int row = random.Next(0, gridSize);
                int col = random.Next(0, gridSize);

                if (!minePositions.Contains((row, col)))
                {
                    minePositions.Add((row, col));
                    PlaceMines(col, row);
                    System.Diagnostics.Debug.WriteLine($"Mine Button added at: ({row}, {col})");
                }
            }
            SweepGrid.UpdateLayout();
            System.Diagnostics.Debug.WriteLine("Total mines placed: " + MineCount);
        }

        private void PlaceMines(int col, int row)
        {
            var mineImage = new Image
            {
                Source = new BitmapImage(new Uri("ms-appx:///Assets/Mine.png")), // Make sure the image is in the Assets folder and named 'mine.png'
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Visibility = Visibility.Visible // Ensure it is visible
            };

            var mineBorder = new Border
            {
                Background = new SolidColorBrush(Windows.UI.Colors.Transparent),
                Child = mineImage
            };

            Grid.SetRow(mineBorder, row);
            Grid.SetColumn(mineBorder, col);
            SweepGrid.Children.Add(mineBorder);
            System.Diagnostics.Debug.WriteLine($"Mine Image added at: ({row}, {col})");
            SweepGrid.UpdateLayout(); // Force the layout to refresh
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
