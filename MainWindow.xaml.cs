using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Data;
using System.Data.SqlClient;

namespace formula1
{
    public partial class MainWindow : Window
    {
        private static string connectionString = @"Data Source=(LocalDB)\MSSQLLocalDB;AttachDbFilename=C:\Users\jakub\source\repos\formula1\Database.mdf;Integrated Security=True";
        
        public MainWindow()
        {
            InitializeComponent();
        }

        private void IndividualRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TableTitleLabel.Content = "Klasyfikacja indywidualna";
        }

        private void TeamRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            TableTitleLabel.Content = "Klasyfikacja drużynowa";
        }
    }
}
