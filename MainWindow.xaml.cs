using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.IO;
using Microsoft.Win32;
using IronOcr;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Data;

namespace formula1
{
    public partial class MainWindow : Window
    { 
        private const int NUMBER_OF_PLAYERS = 8;
        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\jakub\source\repos\formula1\Database.mdf;Integrated Security=True";
        private List<ComboBox> playerComboBoxes;
        private List<RadioButton> fastestLapRadioButtons;

        public MainWindow()
        {
            InitializeComponent();
            AddPlayerComboBoxesToList();
            AddFastestLapRadioButtonsToList();
            UpdateRanking();
        }
        private void AddFastestLapRadioButtonsToList()
        {
            fastestLapRadioButtons = new List<RadioButton>();
            fastestLapRadioButtons.Add(FastestLap1RadioButton);
            fastestLapRadioButtons.Add(FastestLap2RadioButton);
            fastestLapRadioButtons.Add(FastestLap3RadioButton);
            fastestLapRadioButtons.Add(FastestLap4RadioButton);
            fastestLapRadioButtons.Add(FastestLap5RadioButton);
            fastestLapRadioButtons.Add(FastestLap6RadioButton);
            fastestLapRadioButtons.Add(FastestLap7RadioButton);
            fastestLapRadioButtons.Add(FastestLap8RadioButton);
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

        private void IndividualRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TableTitleLabel.Content = "Klasyfikacja indywidualna";
        }

        private void TeamRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TableTitleLabel.Content = "Klasyfikacja drużynowa";
        }

        private void AddResultsButton_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;

                string[] players = new string[NUMBER_OF_PLAYERS];

                var Ocr = new IronTesseract();
                Ocr.Configuration.EngineMode = TesseractEngineMode.TesseractAndLstm;
                Ocr.Configuration.WhiteListCharacters = "jqobTOXAdamndPcynLeikpsiuWO";

                System.Drawing.Rectangle ContentArea;

                OcrInput Input;
                int yValue;

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
                    else if(Result.Text == "W"  || Result.Text == "WOW")
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
                }

                Player1ComboBox.Text = players[0];
                Player2ComboBox.Text = players[1]; 
                Player3ComboBox.Text = players[2]; 
                Player4ComboBox.Text = players[3];
                Player5ComboBox.Text = players[4];
                Player6ComboBox.Text = players[5];
                Player7ComboBox.Text = players[6];
                Player8ComboBox.Text = players[7];

                Trace.WriteLine("done");
            }
        }

        private bool IsEveryPlayerPlaced()
        {
            foreach(ComboBox comboBox in playerComboBoxes)
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
            foreach (RadioButton radioButton in fastestLapRadioButtons)
            {
                if (radioButton.IsChecked == true)
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
            for(int i = 0; i < 8; i++)
            {
                if(fastestLapRadioButtons[i].IsChecked == true)
                {
                    return playerComboBoxes[i].Text;
                }
            }
            throw new Exception("Fastest lap driver not found!");
        }

        private void UpdateRanking()
        {
            string queryString = "SELECT player_nickname AS 'Zawodnik', SUM(points) AS 'Punkty' FROM dbo.Results GROUP BY player_nickname ORDER BY SUM(points) DESC;";

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(queryString, connection);
                    command.ExecuteNonQuery();

                    SqlDataAdapter dataAdapter = new SqlDataAdapter(command);
                    DataTable dataTable = new DataTable("Results");
                    dataAdapter.Fill(dataTable);

                    RankingDataGrid.ItemsSource = dataTable.DefaultView;
                    dataAdapter.Update(dataTable);

                    connection.Close();
                }
                catch (Exception exception)
                {
                    Console.WriteLine(exception.Message);
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

        private void SaveNewRaceButton_Click(object sender, RoutedEventArgs e)
        {
            if(IsEveryPlayerPlaced())     
            {
                if(IsFastestLapChecked())
                {
                    if(IsDatePicked())
                    {
                        if(IsTrackChecked())
                        {
                            Trace.WriteLine("rozpoczynam zapis!");

                            using (SqlConnection connection = new SqlConnection(connectionString))
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
                                        position = i+1;
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
                                }
                                catch (Exception exception)
                                {
                                    Trace.WriteLine(exception.Message);
                                }
                            }
                            Trace.WriteLine("zapis zakonczony!");
                            UpdateRanking();
                        }
                        else
                        {
                            Trace.WriteLine("nie wybrano toru!");
                        }
                    }
                    else
                    {
                        Trace.WriteLine("nie podano daty!");
                    }
                }
                else
                {
                    Trace.WriteLine("nie zaznaczono najszybszego okrazenia");
                }
            }
            else
            {
                Trace.WriteLine("nie zaznaczono wszystkich pozycji!");
            }
        }
    }
}
