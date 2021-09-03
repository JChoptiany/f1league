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
        private List<ComboBox> playerSprintComboBoxes;
        private List<Button> fastestLapButtons;

        public MainWindow()
        {
            InitializeComponent();
            CenterWindow();
            AddPlayerComboBoxesToList();
            AddPlayerSprintComboBoxesToList();
            AddFastestLapButtonsToList();
            UpdateRanking();
            InitializeButtonBrushes();
        }

        private void AddPlayerSprintComboBoxesToList()
        {
            playerSprintComboBoxes = new List<ComboBox>();
            playerSprintComboBoxes.Add(Player1SprintComboBox);
            playerSprintComboBoxes.Add(Player2SprintComboBox);
            playerSprintComboBoxes.Add(Player3SprintComboBox);
            playerSprintComboBoxes.Add(Player4SprintComboBox);
            playerSprintComboBoxes.Add(Player5SprintComboBox);
            playerSprintComboBoxes.Add(Player6SprintComboBox);
            playerSprintComboBoxes.Add(Player7SprintComboBox);
            playerSprintComboBoxes.Add(Player8SprintComboBox);
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
                System.Drawing.Color c1 = System.Drawing.Color.FromArgb(254,254,254), c2 = System.Drawing.Color.FromArgb(255, 0, 0), c3 = System.Drawing.Color.FromArgb(147, 147, 147);
                int yValue;

                ReadDataProgressBar.Dispatcher.Invoke(() => ReadDataProgressBar.Value += 10, DispatcherPriority.Background);

                for (int i = 0; i < 8; i++)
                {
                    Input = new OcrInput();
                    Input.ReplaceColor(c1, c2, 100);
                    Input.ReplaceColor(c3, c2, 100);

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

        private bool IsSprintPodiumPlaced()
        {
            bool p1Placed = false;
            bool p2Placed = false;
            bool p3Placed = false;

            foreach (ComboBox comboBox in playerSprintComboBoxes)
            {
                if (comboBox.Text == "P1")
                {
                    if(p1Placed == true)
                    {
                        return false;
                    }
                    else
                    {
                        p1Placed = true;
                    }
                }
                else if (comboBox.Text == "P2")
                {
                    if (p2Placed == true)
                    {
                        return false;
                    }
                    else
                    {
                        p2Placed = true;
                    }
                }
                else if (comboBox.Text == "P3")
                {
                    if (p3Placed == true)
                    {
                        return false;
                    }
                    else
                    {
                        p3Placed = true;
                    }
                }
            }

            return p1Placed == true && p2Placed == true && p3Placed == true;
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

        private int CalculatePoints(int position, bool isTheFastestLap, string sprintPosition = "-")
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

            switch (sprintPosition)
            {
                case "P1":
                    points += 3;
                    break;
                case "P2":
                    points += 2;
                    break;
                case "P3":
                    points += 1;
                    break;
                default:
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
                comboBox.SelectedIndex = 0;
            }
        }

        private void SetAllPlayerSprintComboBoxesToDefault()
        {
            foreach (ComboBox comboBox in playerSprintComboBoxes)
            {
                comboBox.SelectedIndex = 0;
            }
        }

        private void ResetControls()
        {
            SetAllTheFastestLapButtonsToGray();
            SetAllPlayerComboBoxesToDefault();
            SetAllPlayerSprintComboBoxesToDefault();
            TrackComboBox.SelectedIndex = 0;
            RaceDatePicker.SelectedDate = null;
        }

        private bool IsCorrectFormatOfPlayersPlaced()
        {
            int placedPlayers = 0;
            for (int i = 0; i < playerComboBoxes.Count; i++)
            {
                if (playerComboBoxes[i].SelectedIndex > 0)
                {
                    placedPlayers++;
                }
            }

            for (int i = 0; i < placedPlayers; i++)
            {
                if (playerComboBoxes[i].SelectedIndex <= 0)
                {
                    return false;
                }
            }
            return true;
        }

        private int GetNumberOfPlayers()
        {
            int players = 0;
            foreach (ComboBox comboBox in playerComboBoxes)
            {
                if (comboBox.SelectedIndex > 0)
                {
                    players++;
                }
            }
            return players;
        }

        private void SaveNewRaceButton_Click(object sender, RoutedEventArgs e)
        {
            if (IsCorrectFormatOfPlayersPlaced())
            {
                if (!IsEveryPlayerPlaced())
                {
                    Trace.WriteLine("nie wypełniono wszystkich pozycji!");
                    string message = $"Nie wypełniono wszystkich pozycji. Czy chcesz zapisać wyścig z następującą liczbą uczestników: {GetNumberOfPlayers()}?";
                    MessageBoxResult result = MessageBox.Show(message, "Brakujące dane wyścigu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                    if (result == MessageBoxResult.No)
                    {
                        return;
                    }
                }
                if (IsFastestLapChecked())
                {
                    if (!IsSprintPodiumPlaced())
                    {
                        MessageBoxResult result = MessageBox.Show("Nie podano wyników sprintu lub są one nieprawidłowe! Czy chcesz zapisać wyścig z aktualnie podanymi wynikami kwalifikacji?", "Brakujące dane wyścigu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.No)
                        {
                            return;
                        }
                    }
                    if (!IsDatePicked())
                    {
                        Trace.WriteLine("nie podano daty!");
                        MessageBoxResult result = MessageBox.Show("Nie podano daty wyścigu!\nCzy podać dzisiejszą datę?", "Brakujące dane wyścigu", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                        if (result == MessageBoxResult.Yes)
                        {
                            RaceDatePicker.SelectedDate = DateTime.Now;
                        }
                        else
                        {
                            return;
                        }
                    }
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
                                int numberOfPoints;

                                for (int i = 0; i < GetNumberOfPlayers(); i++)
                                {
                                    position = i + 1;

                                    if (playerComboBoxes[i].Text == GetFastestLapPlayerNickname())
                                    {
                                        numberOfPoints = CalculatePoints(position, true, playerSprintComboBoxes[i].Text);
                                        queryString = $"INSERT INTO dbo.Results (player_nickname, track_country, position, is_the_fastest_lap, date, points) VALUES('{playerComboBoxes[i].Text}', '{trackCountry}', {position}, {1}, '{raceDate}', {numberOfPoints});";
                                    }
                                    else
                                    {
                                        numberOfPoints = CalculatePoints(position, false, playerSprintComboBoxes[i].Text);
                                        queryString = $"INSERT INTO dbo.Results (player_nickname, track_country, position, date, points) VALUES('{playerComboBoxes[i].Text}', '{trackCountry}', {position}, '{raceDate}', {numberOfPoints});";
                                    }

                                    command = new SqlCommand(queryString, connection);
                                    command.Connection.Open();
                                    command.ExecuteNonQuery();
                                    command.Connection.Close();
                                }
                            }
                            catch (Exception exception)
                            {
                                MessageBox.Show("Wystąpił błąd podczas zapisywania danych wyścigu. Skontakuj się z administratorem.", "Błąd zapisu", MessageBoxButton.OK, MessageBoxImage.Error);
                                Trace.WriteLine(exception.Message);
                            }
                            MessageBox.Show("Dane wyścigu zostały zapisane pomyślnie i powinny być za chwilę widoczne w klasyfikacji generalnej", "Zapisano dane", MessageBoxButton.OK, MessageBoxImage.Information);
                            ResetControls();
                            Trace.WriteLine("zapis zakonczony!");
                            UpdateRanking();
                        }
                    }
                    else
                    {
                        MessageBox.Show("Nie wybrano toru!", "Brakujące dane wyścigu", MessageBoxButton.OK, MessageBoxImage.Warning);
                        Trace.WriteLine("nie wybrano toru!");
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

        private void SearchPlayerDataButton_Click(object sender, RoutedEventArgs e)
        {
            if(StatisticsPlayerComboBox.SelectedIndex > 0)
            {
                string playerNickname = StatisticsPlayerComboBox.Text;

                int numberOfRaces = 0;
                int numberOfTheFastestLaps = 0;
                int numberOfWins = 0;
                int numberOfPoints = 0;
                int numberOfPodiums = 0;
                double winPercentage = 0d;
                double podiumPercentage = 0d;
                double averagePosition = 0d;
                string firstName = "not found";
                string secondName = "not found";
                string teamName = "not found";
                string teammate = "not found";

                DateTime recentRaceDate;
                string recentRaceDateStr = "none";

                string queryString;
                SqlCommand command;
                object result;

                string statisticsTextLeft;
                string statisticsTextRight;


                using (SqlConnection connection = new SqlConnection(CONNECTION_STRING))
                {
                    try
                    {
                        // number of races
                        queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}';";
                        connection.Open();
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            numberOfRaces = Convert.ToInt32(result);
                        }

                        // number of wins
                        queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}' AND dbo.Results.position = 1;";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            numberOfWins = Convert.ToInt32(result);
                        }

                        // number of podiums
                        queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}' AND dbo.Results.position <= 3;";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            numberOfPodiums = Convert.ToInt32(result);
                        }

                        // number of the fastest laps
                        queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}' AND dbo.Results.is_the_fastest_lap = 1;";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            numberOfTheFastestLaps = Convert.ToInt32(result);
                        }

                        // number of points
                        queryString = $"SELECT SUM(dbo.Results.points) FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}';";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            numberOfPoints = Convert.ToInt32(result);
                        }

                        // first name
                        queryString = $"SELECT dbo.Players.first_name FROM dbo.Players WHERE dbo.Players.nickname = '{playerNickname}';";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            firstName = result.ToString();
                        }

                        // second name
                        queryString = $"SELECT dbo.Players.second_name FROM dbo.Players WHERE dbo.Players.nickname = '{playerNickname}';";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            secondName = result.ToString();
                        }

                        // team name
                        queryString = $"SELECT dbo.Players.team_name FROM dbo.Players WHERE dbo.Players.nickname = '{playerNickname}';";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            teamName = result.ToString();
                        }

                        // teammate
                        queryString = $"SELECT dbo.Players.nickname FROM dbo.Players WHERE dbo.Players.team_name = '{teamName}' AND dbo.Players.nickname != '{playerNickname}';";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            teammate = result.ToString();
                        }

                        // recent race date
                        queryString = $"SELECT TOP 1 dbo.Results.date FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}' ORDER BY dbo.Results.date DESC;";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        { 
                            recentRaceDate = Convert.ToDateTime(result);
                            recentRaceDateStr = recentRaceDate.ToString("dd.MM.yyyy");
                        }

                        // average position
                        queryString = $"SELECT AVG(Cast(dbo.Results.position as Float)) FROM dbo.Results WHERE dbo.Results.player_nickname = '{playerNickname}';";
                        command = new SqlCommand(queryString, connection);
                        result = command.ExecuteScalar();
                        if (result != null)
                        {
                            averagePosition = Convert.ToDouble(result);
                        }

                        connection.Close();

                        // win percentage
                        winPercentage = Convert.ToDouble(numberOfWins) / Convert.ToDouble(numberOfRaces) * 100d;

                        // podium percentage
                        podiumPercentage = Convert.ToDouble(numberOfPodiums) / Convert.ToDouble(numberOfRaces) * 100d;

                        statisticsTextLeft = $"Imię: {firstName}\nNazwisko: {secondName}\nLiczba wyścigów: {numberOfRaces}\nLiczba zwycięstw: {numberOfWins}\nProcent zwycięstw: {Math.Round(winPercentage, 2)}%\nLiczba podiów: {numberOfPodiums}\nProcent podiów: {Math.Round(podiumPercentage, 2)}%";
                        statisticsTextRight = $"Zespół: {teamName}\nKolega z zespołu: {teammate}\nLiczba punktów: {numberOfPoints}\nLiczba naj. okrążeń: {numberOfTheFastestLaps}\nOstatni wyścig: {recentRaceDateStr}\nŚrednia pozycja: {Math.Round(averagePosition, 2)}";

                        PlayerStatisticsLeftTextBox.Text = statisticsTextLeft;
                        PlayerStatisticsRightTextBox.Text = statisticsTextRight;

                    }
                    catch (Exception exception)
                    {
                        Trace.WriteLine(exception.Message);
                    }
                }
            }
            else
            {
                MessageBox.Show("Nie wybrano zawodnika!", "Błąd wyszukiwania", MessageBoxButton.OK, MessageBoxImage.Warning);
                PlayerStatisticsLeftTextBox.Text = "";
                PlayerStatisticsRightTextBox.Text = "";
            }
        }
       

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (IndividualTabItem.IsSelected)
                {
                    OneToFourNumerationPanel.Visibility = Visibility.Visible;
                    FiveToEightNumerationPanel.Visibility = Visibility.Visible;
                }
                else if (TeamTabItem.IsSelected)
                {
                    OneToFourNumerationPanel.Visibility = Visibility.Visible;
                    FiveToEightNumerationPanel.Visibility = Visibility.Hidden;
                }
                else
                {
                    OneToFourNumerationPanel.Visibility = Visibility.Hidden;
                    FiveToEightNumerationPanel.Visibility = Visibility.Hidden;
                }
            }
            catch (Exception exception)
            {
                Trace.WriteLine(exception.Message);
            }
        }
    }
}
