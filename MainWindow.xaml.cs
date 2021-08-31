using System;
using System.Collections.Generic;
using System.Windows;
using System.Data.SqlClient;
using System.Diagnostics;
using Microsoft.Win32;
using IronOcr;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace formula1
{
    public partial class MainWindow : Window
    {
        private const int NUMBER_OF_PLAYERS = 8;
        private const string CONNECTION_STRING = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\jakub\source\repos\formula1\Database.mdf;Integrated Security=True";
        private ImageBrush purpleButtonBrush;
        private ImageBrush grayButtonBrush;
        private List<ComboBox> playerComboBoxes;
        private List<Button> fastestLapButtons;

        public MainWindow()
        {
            InitializeComponent();
            CenterWindow();
            AddPlayerComboBoxesToList();
            AddFastestLapButtonsToList();
            UpdateRanking();
            InitializeButtonBrushes();
        }

        private void CenterWindow()
        {
            double screenWidth = SystemParameters.PrimaryScreenWidth;
            double screenHeight = SystemParameters.PrimaryScreenHeight;
            double windowWidth = this.Width;
            double windowHeight = this.Height;
            this.Left = (screenWidth / 2) - (windowWidth / 2);
            this.Top = (screenHeight / 2) - (windowHeight / 2);
        }

        private void InitializeButtonBrushes()
        {
            purpleButtonBrush = new ImageBrush();
            grayButtonBrush = new ImageBrush();

            purpleButtonBrush.ImageSource = new BitmapImage(new Uri("../../../Images/fastestLapImage.jpg", UriKind.Relative));
            grayButtonBrush.ImageSource = new BitmapImage(new Uri("../../../Images/fastestLapImageGray.jpg", UriKind.Relative));
        }

        private void AddFastestLapButtonsToList()
        {
            fastestLapButtons = new List<Button>();
            fastestLapButtons.Add(FastestLap1Button);
            fastestLapButtons.Add(FastestLap2Button);
            fastestLapButtons.Add(FastestLap3Button);
            fastestLapButtons.Add(FastestLap4Button);
            fastestLapButtons.Add(FastestLap5Button);
            fastestLapButtons.Add(FastestLap6Button);
            fastestLapButtons.Add(FastestLap7Button);
            fastestLapButtons.Add(FastestLap8Button);
        }

        private void AddPlayerComboBoxesToList()
        {
            playerComboBoxes = new List<ComboBox>();
            playerComboBoxes.Add(Player1ComboBox);
            playerComboBoxes.Add(Player2ComboBox);
            playerComboBoxes.Add(Player3ComboBox);
            playerComboBoxes.Add(Player4ComboBox);
            playerComboBoxes.Add(Player5ComboBox);
            playerComboBoxes.Add(Player6ComboBox);
            playerComboBoxes.Add(Player7ComboBox);
            playerComboBoxes.Add(Player8ComboBox);
        }

        private void AddResultsButton_Click(object sender, RoutedEventArgs e)
        {
            ReadDataProgressBar.Value = 0;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                string[] players = new string[NUMBER_OF_PLAYERS];

                ReadDataProgressBar.Dispatcher.Invoke(() => ReadDataProgressBar.Value += 5, DispatcherPriority.Background);

                var Ocr = new IronTesseract();

                Ocr.Configuration.EngineMode = TesseractEngineMode.TesseractAndLstm;

                Ocr.Configuration.WhiteListCharacters = "jqobTOXAdamndPcynLeikpsiuWO";

                System.Drawing.Rectangle ContentArea;

                OcrInput Input;
                int yValue;

                ReadDataProgressBar.Dispatcher.Invoke(() => ReadDataProgressBar.Value += 10, DispatcherPriority.Background);

                for (int i = 0; i < 8; i++)
                { 
                    Input = new OcrInput();
                    yValue = 366 + 40 * i;
                    ContentArea = new System.Drawing.Rectangle() { X = 646, Y = yValue, Height = 38, Width = 284 };
                    Input.AddImage(filePath, ContentArea);
                    var Result = Ocr.Read(Input);

                    if (Result.Text == string.Empty)
                    {
                        players[i] = "Wybierz zawodnika";
                    }
                    else if (Regex.IsMatch(Result.Text, @"(j|i)qob", RegexOptions.IgnoreCase))
                    {
                        players[i] = "jqob";
                    }
                    else if (Result.Text == "W" || Result.Text == "WOW")
                    {
                        players[i] = "WOW";
                    }
                    else if (Result.Text == "XT" || Result.Text == "Lemi")
                    {
                        players[i] = "Lemi";
                    }
                    else
                    {
                        players[i] = Result.Text;
                    }
                    ReadDataProgressBar.Dispatcher.Invoke(() => ReadDataProgressBar.Value += 10, DispatcherPriority.Background);
                }

                ResetControls();

                Player1ComboBox.Text = players[0];
                Player2ComboBox.Text = players[1];
                Player3ComboBox.Text = players[2];
                Player4ComboBox.Text = players[3];
                Player5ComboBox.Text = players[4];
                Player6ComboBox.Text = players[5];
                Player7ComboBox.Text = players[6];
                Player8ComboBox.Text = players[7];

                ReadDataProgressBar.Dispatcher.Invoke(() => ReadDataProgressBar.Value += 5, DispatcherPriority.Background);


                Trace.WriteLine("done");
            }
        }

        private bool IsEveryPlayerPlaced()
        {
            foreach (ComboBox comboBox in playerComboBoxes)
            {
                if (comboBox.Text == string.Empty || comboBox.Text == "Wybierz zawodnika")
                {
                    return false;
                }
            }
            return true;
        }

        private bool IsFastestLapChecked()
        {
            foreach (Button button in fastestLapButtons)
            {
                if (button.Background == purpleButtonBrush)
                {
                    return true;
                }
            }
            return false;
        }

        private bool IsDatePicked()
        {
            return RaceDatePicker.SelectedDate != null;
        }

        private bool IsTrackChecked()
        {
            return TrackComboBox.Text != string.Empty && TrackComboBox.Text != "Wybierz tor";
        }

        private string GetFastestLapPlayerNickname()
        {
            for (int i = 0; i < 8; i++)
            {
                if (fastestLapButtons[i].Background == purpleButtonBrush)
                {
                    return playerComboBoxes[i].Text;
                }
            }
            throw new Exception("Fastest lap driver not found!");
        }

        private void UpdateRanking()
        {
            string queryString;
            SqlCommand command;
            SqlDataAdapter dataAdapter;
            DataTable dataTable;

            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
            {
                try
                {
                    // Individual table
                    queryString = "SELECT dbo.Results.player_nickname AS 'Zawodnik', SUM(dbo.Results.points) AS 'Punkty', Zespół = (SELECT dbo.Players.team_name FROM dbo.Players WHERE nickname = dbo.Results.player_nickname) FROM dbo.Results GROUP BY dbo.Results.player_nickname ORDER BY Punkty DESC;";

                    connection.Open();
                    command = new SqlCommand(queryString, connection);
                    command.ExecuteNonQuery();
                    connection.Close();

                    dataAdapter = new SqlDataAdapter(command);
                    dataTable = new DataTable("Results");
                    dataAdapter.Fill(dataTable);

                    IndividualRankingDataGrid.ItemsSource = dataTable.DefaultView;
                    dataAdapter.Update(dataTable);

                    // Team Table
                    queryString = "SELECT Players.team_name AS 'Zespół', Sum(Results.points) AS 'Punkty' FROM Players INNER JOIN Results ON Players.nickname = Results.player_nickname GROUP BY Players.team_name ORDER BY Sum(Results.points) DESC;";
                    connection.Open();
                    command = new SqlCommand(queryString, connection);
                    command.ExecuteNonQuery();
                    connection.Close();

                    dataAdapter = new SqlDataAdapter(command);
                    dataTable = new DataTable("Results");
                    dataAdapter.Fill(dataTable);

                    TeamRankingDataGrid.ItemsSource = dataTable.DefaultView;
                    dataAdapter.Update(dataTable);
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception.Message);
                }
            }
        }

        private int CalculatePoints(int position, bool isTheFastestLap)
        {
            int points;
            switch (position)
            {
                case 1:
                    points = 12;
                    break;
                case 2:
                    points = 8;
                    break;
                case 3:
                    points = 5;
                    break;
                case 4:
                    points = 3;
                    break;
                case 5:
                    points = 2;
                    break;
                case 6:
                    points = 1;
                    break;
                default:
                    points = 0;
                    break;
            }
            if (isTheFastestLap)
            {
                points++;
            }
            return points;
        }

        private void SetAllPlayerComboBoxesToDefault()
        {
            foreach (ComboBox comboBox in playerComboBoxes)
            {
                comboBox.SelectedItem = 0;
            }
        }

        private void ResetControls()
        {
            SetAllTheFastestLapButtonsToGray();
            SetAllPlayerComboBoxesToDefault();
            TrackComboBox.SelectedIndex = 0;
            RaceDatePicker.SelectedDate = null;
        }

        private void SaveNewRaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsEveryPlayerPlaced())
            {
                if (IsFastestLapChecked())
                {
                    if (IsDatePicked())
                    {
                        if (IsTrackChecked())
                        {
                            Trace.WriteLine("rozpoczynam zapis!");

                            using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
                            {
                                try
                                {
                                    string trackCountry = TrackComboBox.Text.ToUpper();
                                    string raceDate = Convert.ToDateTime(RaceDatePicker.SelectedDate).ToString("yyyy-MM-dd");

                                    string queryString;
                                    SqlCommand command;
                                    int position;

                                    for (int i = 0; i < 8; i++)
                                    {
                                        position = i + 1;
                                        if (playerComboBoxes[i].Text == GetFastestLapPlayerNickname())
                                        {
                                            queryString = string.Format("INSERT INTO dbo.Results (player_nickname, track_country, position, is_the_fastest_lap, date, points) VALUES('{0}', '{1}', {2}, {3}, '{4}', {5});", playerComboBoxes[i].Text, trackCountry, position, 1, raceDate, CalculatePoints(position, true));
                                        }
                                        else
                                        {
                                            queryString = string.Format("INSERT INTO dbo.Results (player_nickname, track_country, position, date, points) VALUES('{0}', '{1}', {2}, '{3}', {4});", playerComboBoxes[i].Text, trackCountry, position, raceDate, CalculatePoints(position, false));
                                        }

                                        command = new SqlCommand(queryString, connection);
                                        command.Connection.Open();
                                        command.ExecuteNonQuery();
                                        command.Connection.Close();
                                    }
                                    MessageBox.Show("Dane wyścigu zostały zapisane pomyślnie i powinny być za chwilę widoczne w klasyfikacji generalnej", "Zapisano dane", MessageBoxButton.OK, MessageBoxImage.Information);
                                    ResetControls();
                                }
                                catch (Exception exception)
                                {
                                    MessageBox.Show("Wystąpił błąd podczas zapisywania danych wyścigu. Skontakuj się z administratorem.", "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Trace.WriteLine(exception.Message);
                                }
                            }
                            Trace.WriteLine("zapis zakonczony!");
                            UpdateRanking();
                        }
                        else
                        {
                            MessageBox.Show("Nie wybrano toru!", "Brakujące dane wyścigu", MessageBoxButton.OK, MessageBoxImage.Warning);
                            Trace.WriteLine("nie wybrano toru!");
                        }
                    }
                    else
                    {
                        MessageBoxResult result = MessageBox.Show("Nie podano daty wyścigu!\nCzy podać dzisiejszą datę?", "Brakujące dane wyścigu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            RaceDatePicker.SelectedDate = DateTime.Now;
                            SaveNewRaceButton_Click(sender, e);
                        }
                        Trace.WriteLine("nie podano daty!");
                    }
                }
                else
                {
                    MessageBox.Show("Nie zaznaczono najszybszego okrążenia!", "Brakujące dane wyścigu", MessageBoxButton.OK, MessageBoxImage.Warning);
                    Trace.WriteLine("nie zaznaczono najszybszego okrazenia");
                }
            }
            else
            {
                MessageBox.Show("Nie zaznaczono wszystkich pozycji!", "Brakujące dane wyścigu", MessageBoxButton.OK, MessageBoxImage.Warning);
                Trace.WriteLine("nie zaznaczono wszystkich pozycji!");
            }
        }

        private void SetAllTheFastestLapButtonsToGray()
        {
            foreach (Button button in fastestLapButtons)
            {
                button.Background = grayButtonBrush;
            }
        }

        private void FastestLapButton_Click(object sender, RoutedEventArgs e)
        {
            SetAllTheFastestLapButtonsToGray();
            (sender as Button).Background = purpleButtonBrush;
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void Grid_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            {
                DragMove();
            }
        }
    }
}
