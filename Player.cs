using System;
using System.Data.SqlClient;
using System.Diagnostics;

namespace formula1
{
    class Player
    {
        public string Nickname { get; private set; }
        public string FirstName { get; private set; }
        public string LastName { get; private set; }
        public string TeamName { get; private set; }
        public string Teammate { get; private set; }
        public string RecentRaceTrackName { get; private set; }

        public int NumberOfPoints { get; private set; }
        public int NumberOfRaces { get; private set; }
        public int NumberOfWins { get; private set; }
        public int NumberOfPodiums { get; private set; }
        public int NumberOfTheFastestsLaps { get; private set; }
        public int TheBestPosition { get; private set; }
        public int RecentRacePosition { get; private set; }

        public double AveragePosition { get; private set; }
        public double WinPercentage { get; private set; }
        public double PodiumPercentage { get; private set; }
        public DateTime? RecentRaceDate { get; private set; }

        public Player(string playerNickname)
        {
            Nickname = playerNickname;
        }

        public string GetPlayerStatistics(int page)
        {
            switch (page)
            {
                case 1:
                    return $"Imię: {FirstName}\nNazwisko: {LastName}\nLiczba wyścigów: {NumberOfRaces}\nLiczba zwycięstw: {NumberOfWins}\nProcent zwycięstw: {Math.Round(WinPercentage, 2)}%\nLiczba podiów: {NumberOfPodiums}\nProcent podiów: {Math.Round(PodiumPercentage, 2)}%";
                case 2:
                    return $"Zespół: {TeamName}\nKolega z zespołu: {Teammate}\nLiczba punktów: {NumberOfPoints}\nLiczba naj. okrążeń: {NumberOfTheFastestsLaps}\nNajlepsza pozycja: {TheBestPosition}\nŚrednia pozycja: {Math.Round(AveragePosition, 2)}";
                case 3:
                    if (RecentRaceDate.HasValue)
                    {
                        DateTime date = Convert.ToDateTime(RecentRaceDate);
                        return $"Ostatni wyścig: {date.ToString("dd.MM.yyyy")}, {RecentRaceTrackName}, poz. {RecentRacePosition}";
                    }
                    else
                    {
                        return "";
                    }
                default:
                    return "Data not found!";
            }
        }

        public void LoadPlayerDataFromDataBase(string connectionString)
        {
            NumberOfRaces = 0;
            NumberOfTheFastestsLaps = 0;
            NumberOfWins = 0;
            NumberOfPoints = 0;
            NumberOfPodiums = 0;
            TheBestPosition = 0;
            RecentRacePosition = 0;
            WinPercentage = 0d;
            PodiumPercentage = 0d;
            AveragePosition = 0d;
            FirstName = "not found";
            LastName = "not found";
            TeamName = "not found";
            Teammate = "not found";
            RecentRaceTrackName = "not found";
            RecentRaceDate = null;

            string queryString;
            SqlCommand command;
            object result;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                try
                {
                    // number of races
                    queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}';";
                    connection.Open();
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        NumberOfRaces = Convert.ToInt32(result);
                    }

                    // number of wins
                    queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}' AND dbo.Results.position = 1;";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        NumberOfWins = Convert.ToInt32(result);
                    }

                    // number of podiums
                    queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}' AND dbo.Results.position <= 3;";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        NumberOfPodiums = Convert.ToInt32(result);
                    }

                    // number of the fastest laps
                    queryString = $"SELECT COUNT(dbo.Results.result_id) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}' AND dbo.Results.is_the_fastest_lap = 1;";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        NumberOfTheFastestsLaps = Convert.ToInt32(result);
                    }

                    // number of points
                    queryString = $"SELECT SUM(dbo.Results.points) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        NumberOfPoints = Convert.ToInt32(result);
                    }

                    // first name
                    queryString = $"SELECT dbo.Players.first_name FROM dbo.Players WHERE dbo.Players.nickname = '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        FirstName = result.ToString();
                    }

                    // last name
                    queryString = $"SELECT dbo.Players.second_name FROM dbo.Players WHERE dbo.Players.nickname = '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        LastName = result.ToString();
                    }

                    // team name
                    queryString = $"SELECT dbo.Players.team_name FROM dbo.Players WHERE dbo.Players.nickname = '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        TeamName = result.ToString();
                    }

                    // teammate
                    queryString = $"SELECT dbo.Players.nickname FROM dbo.Players WHERE dbo.Players.team_name = '{TeamName}' AND dbo.Players.nickname != '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        Teammate = result.ToString();
                    }

                    // recent race date
                    queryString = $"SELECT TOP 1 dbo.Results.date FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}' ORDER BY dbo.Results.date DESC;";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        RecentRaceDate = Convert.ToDateTime(result);
                    }

                    // recent race position
                    queryString = $"SELECT TOP 1 dbo.Results.position FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}' ORDER BY dbo.Results.result_id DESC;";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        RecentRacePosition = Convert.ToInt32(result);
                    }

                    // recent race track name
                    queryString = $"SELECT dbo.Tracks.name FROM dbo.Tracks WHERE dbo.Tracks.country = (SELECT TOP 1 dbo.Results.track_country FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}' ORDER BY dbo.Results.result_id DESC);";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        RecentRaceTrackName = result.ToString();
                    }

                    // average position
                    queryString = $"SELECT AVG(Cast(dbo.Results.position as Float)) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        AveragePosition = Convert.ToDouble(result);
                    }

                    // the best position
                    queryString = $"SELECT MIN(dbo.Results.position) FROM dbo.Results WHERE dbo.Results.player_nickname = '{Nickname}';";
                    command = new SqlCommand(queryString, connection);
                    result = command.ExecuteScalar();
                    if (result != null)
                    {
                        TheBestPosition = Convert.ToInt32(result);
                    }

                    connection.Close();

                    // win percentage
                    WinPercentage = Convert.ToDouble(NumberOfWins) / Convert.ToDouble(NumberOfRaces) * 100d;

                    // podium percentage
                    PodiumPercentage = Convert.ToDouble(NumberOfPodiums) / Convert.ToDouble(NumberOfRaces) * 100d;
                }
                catch (Exception exception)
                {
                    Trace.WriteLine(exception.Message);
                }
            }
        }
    }
}