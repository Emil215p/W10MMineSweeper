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
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Input;

namespace W10MMineSweeper
{
    public sealed partial class MineSweeperPage : Page, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        Dictionary<(int, int), int> neighborMineCounts;
        HashSet<(int, int)> minePositions = new HashSet<(int, int)>();
        int gridSize;
        double mineRatio = 0;
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
                            Name = "GridSizeTextBox",
                            Header = "Grid Size",
                            PlaceholderText = "Enter grid size...",
                            InputScope = new InputScope
                            {
                                Names = { new InputScopeName { NameValue = InputScopeNameValue.Number } }
                            },
                        },
                        new TextBox
                        {
                            Name = "MineRatioTextBox",
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

            // Attach event handlers after creating the dialog
            ((TextBox)((StackPanel)inputDialog.Content).Children[0]).TextChanging += GridSizeTextBox_OnTextChanging;

            // Attach event handlers after creating the dialog
            ((TextBox)((StackPanel)inputDialog.Content).Children[1]).TextChanging += MineRatioTextBox_OnTextChanging;

            ContentDialogResult result = await inputDialog.ShowAsync();
        //System.Diagnostics.Debug.WriteLine("Dialog result: " + result);

        if (result == ContentDialogResult.Primary)
        {
            var textBoxSize = (TextBox)((StackPanel)inputDialog.Content).Children[0];
            var textBoxRatio = (TextBox)((StackPanel)inputDialog.Content).Children[1];

            if (int.TryParse(textBoxSize.Text, out int gridSize))
            {

                if (double.TryParse(textBoxRatio.Text, out double parsedRatio))
                {
                    //System.Diagnostics.Debug.WriteLine("Mine ratio submitted: " + parsedRatio);
                    mineRatio = parsedRatio / 100.0; // Convert percentage to decimal
                    //System.Diagnostics.Debug.WriteLine("Parsed mine ratio: " + mineRatio);
                }

                if (mineRatio <= 0 || mineRatio > 1)
                    {
                        mineRatio = 0.20;
                    }
                if (gridSize <= 1)
                    {
                        gridSize = 2;
                        
                        if (mineRatio < 0.25 )
                        {
                            mineRatio = 0.25;
                        }
                    }

                //System.Diagnostics.Debug.WriteLine("Grid size from user input: " + gridSize);
                //System.Diagnostics.Debug.WriteLine("Mine ratio from user input: " + mineRatio);

                InitializeGrid(gridSize);
                AddBordersToGrid(gridSize);
                AddMinesToGrid(gridSize, mineRatio);
                CountNearbyMines(gridSize);
                DisplayNeighborMineCounts();
                }
            else
            {
                    if (double.TryParse(textBoxRatio.Text, out double parsedRatio))
                    {
                        //System.Diagnostics.Debug.WriteLine("Mine ratio submitted: " + parsedRatio);
                        mineRatio = parsedRatio / 100.0; // Convert percentage to decimal
                        //System.Diagnostics.Debug.WriteLine("Parsed mine ratio: " + mineRatio);
                    }

                    if (mineRatio <= 0 || mineRatio > 1)
                    {
                        mineRatio = 0.20;
                    }

                //System.Diagnostics.Debug.WriteLine("Invalid input; defaulting.");
                InitializeGrid(13);
                AddBordersToGrid(13);
                AddMinesToGrid(13, mineRatio);
                CountNearbyMines(13);
                DisplayNeighborMineCounts();
            }
        }
        else
        {
            System.Diagnostics.Debug.WriteLine("Dialog dismissed without primary button click");
        }

        //System.Diagnostics.Debug.WriteLine("Dialog processing complete");
        SweepGrid.UpdateLayout();
        //System.Diagnostics.Debug.WriteLine("SweepGrid layout updated. Visibility: " + SweepGrid.Visibility);
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
            //System.Diagnostics.Debug.WriteLine("Grid initialized and layout updated with size: " + gridSize);
        }

        private void AddBordersToGrid(int gridSize)
        {
            for (int row = 0; row < gridSize; row++)
            {
                for (int col = 0; col < gridSize; col++)
                {
                    // Add cover dynamically
                    var cellCover = new Border
                    {
                        Background = new SolidColorBrush(Windows.UI.Colors.LightGray),
                        Opacity = 1,
                        Tag = (row, col) // Tag for identification
                    };
                    Canvas.SetZIndex(cellCover, 2); // Ensure the cover is on top of the border
                    cellCover.Tapped += CellCover_Tapped;
                    cellCover.RightTapped += CellCover_RightTapped;
                    Grid.SetRow(cellCover, row);
                    Grid.SetColumn(cellCover, col);
                    SweepGrid.Children.Add(cellCover);

                    // Border for cover elements.
                    var coverBorder = new Border
                    {
                        BorderBrush = new SolidColorBrush(Windows.UI.Colors.Black),
                        BorderThickness = new Thickness(1)
                    };
                    Grid.SetRow(coverBorder, row);
                    Grid.SetColumn(coverBorder, col);
                    Canvas.SetZIndex(coverBorder, 3); // Ensure the border is on top of the cover
                    SweepGrid.Children.Add(coverBorder);
                }
            }
            //System.Diagnostics.Debug.WriteLine("Borders and covers added to grid");
        }

        private async void CellCover_Tapped(object sender, TappedRoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                var position = (ValueTuple<int, int>)border.Tag;
                if (minePositions.Contains(position))
                {
                    border.Visibility = Visibility.Collapsed;
                    await ShowResetDialog();
                }
                else
                {
                    border.Visibility = Visibility.Collapsed;
                }
            }
        }

        private void CellCover_RightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            var border = sender as Border;
            if (border != null)
            {
                var position = (ValueTuple<int, int>)border.Tag; // Retrieve position from Tag
                var existingFlag = border.Child as TextBlock;

                if (existingFlag == null)
                {
                    // Right clicking on PC adds a flag to a unrevealed tile, long tap for mobile.
                    var flag = new TextBlock
                    {
                        Text = "🚩",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center,
                        Foreground = new SolidColorBrush(Windows.UI.Colors.Red) // Red font color
                    };
                    border.Child = flag;

                    MineCount--;
                }
                else
                {
                    // Remove flag
                    border.Child = null;
                    MineCount++;
                }
            }
        }



        private async Task ShowResetDialog()
        {
            ContentDialog resetDialog = new ContentDialog
            {
                Title = "Game Over",
                Content = "You blew up.",
                PrimaryButtonText = "Restart",
                SecondaryButtonText = "Exit"
            };

            ContentDialogResult result = await resetDialog.ShowAsync();
            if (result == ContentDialogResult.Primary)
            {
                Frame.Navigate(typeof(MainPage));
                Frame.Navigate(typeof(MineSweeperPage));
            }

            if (result == ContentDialogResult.Secondary)
            {
                Frame.Navigate(typeof(MainPage));
            }
        }

        private void AddMinesToGrid(int gridSize, double mineRatio)
        {
            Random random = new Random();
            int mineCount = (int)(gridSize * gridSize * mineRatio);
            MineCount = mineCount;
            minePositions.Clear(); // Ensure it's clear before adding new mines

            while (minePositions.Count < MineCount)
            {
                int row = random.Next(0, gridSize);
                int col = random.Next(0, gridSize);
                if (!minePositions.Contains((row, col)))
                {
                    minePositions.Add((row, col));
                    PlaceMines(col, row);
                    //System.Diagnostics.Debug.WriteLine($"Mine placed at: ({row}, {col})");
                }
            }

            //System.Diagnostics.Debug.WriteLine("Total mines placed: " + MineCount);
            //System.Diagnostics.Debug.WriteLine("Mine locations:");
            //foreach (var position in minePositions)
            //{
                //System.Diagnostics.Debug.WriteLine($"Row: {position.Item1}, Column: {position.Item2}");
            //}
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
            //System.Diagnostics.Debug.WriteLine($"Mine Image added at: ({row}, {col})");
            SweepGrid.UpdateLayout(); // Force the layout to refresh
        }

        private void CountNearbyMines(int gridSize)
        {
            //System.Diagnostics.Debug.WriteLine("Counting nearby mines... " + gridSize);
            neighborMineCounts = new Dictionary<(int, int), int>();

            //System.Diagnostics.Debug.WriteLine("Initialized neighborMineCounts");

            int[] rowOffsets = { -1, -1, -1, 0, 0, 1, 1, 1 };
            int[] colOffsets = { -1, 0, 1, -1, 1, -1, 0, 1 };

            //System.Diagnostics.Debug.WriteLine($"Grid size: {gridSize}");
            //System.Diagnostics.Debug.WriteLine("Mine positions:");
            foreach (var pos in minePositions)
            {
                //System.Diagnostics.Debug.WriteLine($"Mine at: ({pos.Item1}, {pos.Item2})");
            }

            foreach (var cell in cells)
            {
                if (cell.Row >= gridSize || cell.Column >= gridSize)
                {
                    //System.Diagnostics.Debug.WriteLine($"Invalid cell detected: ({cell.Row}, {cell.Column})");
                    continue;
                }

                int neighborMineCount = 0;
                //System.Diagnostics.Debug.WriteLine($"Checking cell at ({cell.Row}, {cell.Column}):");

                for (int i = 0; i < 8; i++)
                {
                    int newRow = cell.Row + rowOffsets[i];
                    int newCol = cell.Column + colOffsets[i];
                    if (newRow < 0 || newRow >= gridSize || newCol < 0 || newCol >= gridSize)
                    {
                        //System.Diagnostics.Debug.WriteLine($"Skipping neighbor at ({newRow}, {newCol}) - out of bounds");
                        continue;
                    }
                    if (minePositions.Contains((newRow, newCol)))
                    {
                        neighborMineCount++;
                        //System.Diagnostics.Debug.WriteLine($"Mine found at: ({newRow}, {newCol}) for cell ({cell.Row}, {cell.Column})");
                    }
                }
                neighborMineCounts[(cell.Row, cell.Column)] = neighborMineCount;
            }
            //System.Diagnostics.Debug.WriteLine("Finished counting nearby mines.");
            //System.Diagnostics.Debug.WriteLine("Neighbor Mine Counts:");
            //for (int row = 0; row < gridSize; row++)
            //{
            //    for (int col = 0; col < gridSize; col++)
            //    {
            //        System.Diagnostics.Debug.Write(neighborMineCounts.ContainsKey((row, col)) ? neighborMineCounts[(row, col)] + " " : "0 ");
            //    }
            //    System.Diagnostics.Debug.WriteLine("");
            //}
        }

        private void DisplayNeighborMineCounts()
        {
            if (neighborMineCounts == null)
            {
                System.Diagnostics.Debug.WriteLine("neighborMineCounts is null!");
                return;
            }

            foreach (var cell in cells)
            {
                int mineCount = neighborMineCounts.ContainsKey((cell.Row, cell.Column)) ? neighborMineCounts[(cell.Row, cell.Column)] : 0;
                var cellTextBlock = new TextBlock
                {
                    Text = mineCount > 0 ? mineCount.ToString() : "",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    Foreground = new SolidColorBrush(Windows.UI.Colors.Red), // Red font color
                };

                // Check if each cell is valid
                //System.Diagnostics.Debug.WriteLine($"Processing cell at ({cell.Row}, {cell.Column})");

                // Locate the corresponding cell border and update it
                var cellBorder = SweepGrid.Children
                    .Cast<FrameworkElement>()
                    .FirstOrDefault(e => Grid.GetRow(e) == cell.Row && Grid.GetColumn(e) == cell.Column);

                if (cellBorder == null)
                {
                    //System.Diagnostics.Debug.WriteLine($"No cellBorder found for cell at ({cell.Row}, {cell.Column})");
                    continue;
                }

                if (cellBorder is Border border)
                {
                    border.Child = cellTextBlock;
                    //System.Diagnostics.Debug.WriteLine($"Updated cell at ({cell.Row}, {cell.Column}) with count: {mineCount}");
                }
                else
                {
                    //System.Diagnostics.Debug.WriteLine($"Element at ({cell.Row}, {cell.Column}) is not a Border");
                }
            }
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

        private void GridSizeTextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            // Validate input: only allow digits
            string newText = sender.Text;
            int position = sender.SelectionStart;

            // Ensure valid characters only
            if (!Regex.IsMatch(newText, @"^\d*$"))
            {
                // Remove invalid characters
                var invalidChars = newText.Where(c => !char.IsDigit(c)).ToArray();
                foreach (var ch in invalidChars)
                {
                    position = newText.IndexOf(ch);
                    if (position >= 0 && position < newText.Length)
                    {
                        newText = newText.Remove(position, 1);
                        sender.Text = newText;
                        sender.SelectionStart = position;
                    }
                }
            }
        }

        private void MineRatioTextBox_OnTextChanging(TextBox sender, TextBoxTextChangingEventArgs args)
        {
            // Validate input: only allow digits and max length of 3
            string newText = sender.Text;
            int position = sender.SelectionStart;

            // Ensure valid characters and length
            if (!Regex.IsMatch(newText, @"^\d*$") || newText.Length > 3)
            {
                // Remove invalid characters or trim the length
                if (newText.Length > 0)
                {
                    if (newText.Length > 3)
                    {
                        // Trim to the first 3 characters
                        newText = newText.Substring(0, 3);
                        sender.Text = newText;
                        sender.SelectionStart = position > 3 ? 3 : position; // Adjust cursor position
                    }
                    else
                    {
                        // Remove invalid character
                        newText = newText.Remove(position - 1, 1);
                        sender.Text = newText;
                        sender.SelectionStart = Math.Max(0, position - 1);
                    }
                }
            }
        }
    }
}
