using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.OleDb;
using System.Media;
using System.Windows;
using System.Windows.Controls;

namespace COMP609_Assessment_2_LMS_GUI_App
{
    public partial class MainWindow : Window
    {
        private readonly string connectionString;
        int checker;
        public MainWindow()
        {
            connectionString = App.DatabaseConnectionString;
            InitializeComponent();
            livestockdataviewer();
            commoditydataviewer();
            PopulateComboBoxWithDbIds();
            PopulateComboBoxWithDbColours();
        }
        private void PopulateComboBoxWithDbIds()
        {
            List<string> dbIds = RetrieveIdsFromDatabase();

            txtLivestockID.ItemsSource = dbIds;
            txtLivestockSearch.ItemsSource = dbIds;
        }
        private List<string> RetrieveIdsFromDatabase()
        {
            List<string> dbIds = new List<string>();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT ID FROM Cow UNION ALL SELECT ID FROM Goat UNION ALL SELECT ID FROM Sheep";
                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dbIds.Add(reader["ID"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving IDs from the database: " + ex.Message);
            }
            return dbIds;
        }
        private void PopulateComboBoxWithDbColours()
        {
            List<string> dbColours = RetrieveColoursFromDatabase();

            txtLivestockColour.ItemsSource = dbColours;
            txtStatisticsColour.ItemsSource = dbColours;
        }
        private List<string> RetrieveColoursFromDatabase()
        {
            List<string> dbColours = new List<string>();

            try
            {
                using (OleDbConnection connection = new OleDbConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT Colour FROM (" +
                                   "SELECT Colour FROM Cow " +
                                   "UNION ALL " +
                                   "SELECT Colour FROM Goat " +
                                   "UNION ALL " +
                                   "SELECT Colour FROM Sheep) AS AllColours";

                    using (OleDbCommand command = new OleDbCommand(query, connection))
                    using (OleDbDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            dbColours.Add(reader["Colour"].ToString());
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error retrieving Colours from the database: " + ex.Message);
            }

            return dbColours;
        }
        private bool IsIDValid(string selectedID, OleDbConnection connection)
        {
            using (OleDbCommand cmd = connection.CreateCommand())
            {
                cmd.CommandType = CommandType.Text;

                cmd.CommandText = "SELECT ID FROM Cow WHERE ID = ? " +
                                  "UNION " +
                                  "SELECT ID FROM Goat WHERE ID = ? " +
                                  "UNION " +
                                  "SELECT ID FROM Sheep WHERE ID = ?";

                cmd.Parameters.AddWithValue("@ID1", selectedID);
                cmd.Parameters.AddWithValue("@ID2", selectedID);
                cmd.Parameters.AddWithValue("@ID3", selectedID);

                object result = cmd.ExecuteScalar();

                return result != null;
            }
        }
        private void exitApp(object sender, RoutedEventArgs e)
        {
            MessageBoxResult iExit;
            iExit = MessageBox.Show("Are you sure you wish to exit?", "LMS GUI Application", MessageBoxButton.YesNo, MessageBoxImage.Information);

            if (iExit == MessageBoxResult.Yes)
            {
                Application.Current.Shutdown();
            }
        }
        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox textBox)
            {
                textBox.Foreground = System.Windows.Media.Brushes.Black;

                if (textBox.Text == "Invalid Type." || textBox.Text == "Enter a number." ||
                    textBox.Text == "Enter a type." || textBox.Text == "Type Mismatch." || textBox.Text == "Invalid Weight Threshold." ||
                    textBox.Text == "Invalid Colour.")
                {
                    textBox.Text = ""; 
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }
        private void ComboBox_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is ComboBox comboBox)
            {
                comboBox.Foreground = System.Windows.Media.Brushes.Black;

                if (comboBox.Text == "ID already exists in database." || comboBox.Text == "Invalid ID." || comboBox.Text == "Invalid Colour." ||
                    comboBox.Text == "ID and Type Mismatch." || comboBox.Text == "Enter or Select a ID." || comboBox.Text == "Record not found." ||
                    comboBox.Text == "Enter or Select a colour." || comboBox.Text == "ID Not Found.")
                {
                    comboBox.Text = null;
                    comboBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }
        // Livestock
        void livestockdataviewer()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand(
                    "SELECT ID, 'Cow' AS Type, Water, Cost, Weight, Colour, Milk AS MilkWool FROM Cow " +
                    "UNION ALL " +
                    "SELECT ID, 'Goat' AS Type, Water, Cost, Weight, Colour, Milk AS MilkWool FROM Goat " +
                    "UNION ALL " +
                    "SELECT ID, 'Sheep' AS Type, Water, Cost, Weight, Colour, Wool AS MilkWool FROM Sheep " +
                    "ORDER BY ID;", conn))
                    {
                        DataTable dt = new DataTable();
                        OleDbDataAdapter dp = new OleDbDataAdapter(cmd);
                        dp.Fill(dt);
                        dataGridLivestockView.ItemsSource = dt.DefaultView;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void txtLivestockID_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                if (txtLivestockID.SelectedItem != null)
                {
                    string selectedID = txtLivestockID.SelectedItem.ToString();

                    string query = "SELECT ID, 'Cow' AS TableName, Water, Cost, Weight, Colour, Milk AS MilkWool FROM Cow WHERE ID = @SelectedID " +
                                   "UNION ALL SELECT ID, 'Goat' AS TableName, Water, Cost, Weight, Colour, Milk AS MilkWool FROM Goat WHERE ID = @SelectedID " +
                                   "UNION ALL SELECT ID, 'Sheep' AS TableName, Water, Cost, Weight, Colour, Wool AS MilkWool FROM Sheep WHERE ID = @SelectedID";

                    using (OleDbConnection connection = new OleDbConnection(connectionString))
                    {
                        connection.Open();
                        using (OleDbCommand command = new OleDbCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@SelectedID", selectedID);

                            using (OleDbDataReader reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    txtLivestockType.Text = reader["TableName"].ToString();
                                    txtLivestockWater.Text = reader["Water"].ToString();
                                    txtLivestockCost.Text = reader["Cost"].ToString();
                                    txtLivestockWeight.Text = reader["Weight"].ToString();
                                    txtLivestockColour.Text = reader["Colour"].ToString();
                                    txtLivestockMilkWool.Text = reader["MilkWool"].ToString();
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("An error occurred: " + ex.Message);
            }
        }
        private void LivestockDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridLivestockView.SelectedItem != null)
            {
                if (dataGridLivestockView.SelectedItem is DataRowView selectedRow)
                {
                    txtLivestockID.SelectedItem = selectedRow["ID"].ToString();
                    txtLivestockType.Text = selectedRow["Type"].ToString();
                    txtLivestockWater.Text = selectedRow["Water"].ToString();
                    txtLivestockCost.Text = selectedRow["Cost"].ToString();
                    txtLivestockWeight.Text = selectedRow["Weight"].ToString();
                    txtLivestockColour.Text = selectedRow["Colour"].ToString();
                    txtLivestockMilkWool.Text = selectedRow["MilkWool"].ToString();

                    foreach (UIElement element in gridLivestock.Children)
                    {
                        if (element is TextBox textBox)
                        {
                            textBox.Foreground = System.Windows.Media.Brushes.Black;
                        }
                        if (element is ComboBox comboBox)
                        {
                            comboBox.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    }
                }
            }
        }
        private void btnLivestockInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLivestockID.Text) ||
                         string.IsNullOrWhiteSpace(txtLivestockType.Text) ||
                         string.IsNullOrWhiteSpace(txtLivestockWater.Text) ||
                         string.IsNullOrWhiteSpace(txtLivestockCost.Text) ||
                         string.IsNullOrWhiteSpace(txtLivestockWeight.Text) ||
                         string.IsNullOrWhiteSpace(txtLivestockColour.Text) ||
                         string.IsNullOrWhiteSpace(txtLivestockMilkWool.Text))
                {
                    MessageBox.Show("Please enter into all fields.", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    using (OleDbConnection conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        OleDbCommand cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.Text;

                        double id, water, cost, weight, milkwool;

                        string colour = txtLivestockColour.Text;

                        string lowerCaseInput = colour.ToLower();

                        string capitalizedInput = char.ToUpper(lowerCaseInput[0]) + lowerCaseInput.Substring(1);

                        if (double.TryParse(txtLivestockID.Text, out id) &&
                            double.TryParse(txtLivestockWater.Text, out water) &&
                            double.TryParse(txtLivestockCost.Text, out cost) &&
                            double.TryParse(txtLivestockWeight.Text, out weight) &&
                            double.TryParse(txtLivestockMilkWool.Text, out milkwool) &&
                            IsEnteredColorValid(txtLivestockColour.Text))
                        {
                            cmd.CommandText = $"SELECT ID FROM Cow WHERE ID = {id} " +
                                              $"UNION ALL SELECT ID FROM Goat WHERE ID = {id} " +
                                              $"UNION ALL SELECT ID FROM Sheep WHERE ID = {id}";
                            var existingId = cmd.ExecuteScalar();

                            if (existingId == null)
                            {
                                if (txtLivestockType.Text == "Cow")
                                {
                                    cmd.CommandText = "INSERT INTO Cow(ID,Water,Cost,Weight,Colour,Milk)" +
                                                      $"VALUES('{id}', '{water}', '{cost}', '{weight}', '{capitalizedInput}', '{milkwool}')";
                                }
                                else if (txtLivestockType.Text == "Goat")
                                {
                                    cmd.CommandText = "INSERT INTO Goat(ID,Water,Cost,Weight,Colour,Milk)" +
                                                      $"VALUES('{id}', '{water}', '{cost}', '{weight}', '{capitalizedInput}', '{milkwool}')";
                                }
                                else if (txtLivestockType.Text == "Sheep")
                                {
                                    cmd.CommandText = "INSERT INTO Sheep(ID,Water,Cost,Weight,Colour,Wool)" +
                                                      $"VALUES('{id}', '{water}', '{cost}', '{weight}', '{capitalizedInput}', '{milkwool}')";
                                }
                                else
                                {
                                    txtLivestockType.SelectedItem = "Select a Type.";
                                    txtLivestockType.Foreground = System.Windows.Media.Brushes.Red;
                                    SystemSounds.Beep.Play();
                                    return;
                                }
                                cmd.ExecuteNonQueryAsync();
                                conn.Close();
                                MessageBox.Show("Record Inserted Successfully", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Information);
                                livestockdataviewer();
                                PopulateComboBoxWithDbIds();
                                PopulateComboBoxWithDbColours();
                                txtLivestockID.Text = "";
                                txtLivestockType.Text = "";
                                txtLivestockWater.Text = "";
                                txtLivestockCost.Text = "";
                                txtLivestockWeight.Text = "";
                                txtLivestockColour.Text = "";
                                txtLivestockMilkWool.Text = "";
                                txtLivestockSearch.Text = "";

                                foreach (UIElement element in gridLivestock.Children)
                                {
                                    if (element is TextBox textBox)
                                    {
                                        textBox.Foreground = System.Windows.Media.Brushes.Black;
                                        txtLivestockID.Foreground = System.Windows.Media.Brushes.Black;
                                    }
                                }
                            }
                            else
                            {
                                txtLivestockID.Text = "ID already exists in database.";
                                txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                            }
                        }
                        else if (txtLivestockID.Text == null)
                        {
                            txtLivestockID.Text = "Enter or Select a ID.";
                            txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (!double.TryParse(txtLivestockID.Text, out id))
                        {
                            txtLivestockID.Text = "Enter or Select a ID.";
                            txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (!double.TryParse(txtLivestockWater.Text, out water))
                        {
                            txtLivestockWater.Text = "Enter a number.";
                            txtLivestockWater.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (!double.TryParse(txtLivestockCost.Text, out cost))
                        {
                            txtLivestockCost.Text = "Enter a number.";
                            txtLivestockCost.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (!double.TryParse(txtLivestockWeight.Text, out weight))
                        {
                            txtLivestockWeight.Text = "Enter a number.";
                            txtLivestockWeight.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (!IsEnteredColorValid(capitalizedInput))
                        {
                            txtLivestockColour.Text = "Invalid colour.";
                            txtLivestockColour.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (txtLivestockColour.Text == null)
                        {
                            txtLivestockColour.Text = "Enter or Select a colour.";
                            txtLivestockColour.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (!double.TryParse(txtLivestockMilkWool.Text, out milkwool))
                        {
                            txtLivestockMilkWool.Text = "Enter a number.";
                            txtLivestockMilkWool.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else
                        {
                            MessageBox.Show("Invalid Field Input", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnLivestockUpdate_Click(object sender, RoutedEventArgs e) 
        {
            try
            {
                double id;
                
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    if (string.IsNullOrWhiteSpace(txtLivestockID.Text) &&
                    string.IsNullOrWhiteSpace(txtLivestockType.Text) &&
                    string.IsNullOrWhiteSpace(txtLivestockWater.Text) &&
                    string.IsNullOrWhiteSpace(txtLivestockCost.Text) &&
                    string.IsNullOrWhiteSpace(txtLivestockWeight.Text) &&
                    string.IsNullOrWhiteSpace(txtLivestockColour.Text) &&
                    string.IsNullOrWhiteSpace(txtLivestockMilkWool.Text))
                {
                    txtLivestockID.Text = "Enter or Select a ID.";
                    txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                    SystemSounds.Beep.Play();
                }
                    if (!IsIDValid(txtLivestockID.Text, conn))
                    {
                        txtLivestockID.Text = "Invalid ID.";
                        txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                    }

                    else if (!double.TryParse(txtLivestockID.Text?.ToString(), out id))
                    {
                        txtLivestockID.Text = "Invalid ID.";
                        txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                    }
                    else if (!IsEnteredColorValid(txtLivestockColour.Text))
                    {
                        txtLivestockColour.Text = "Invalid Colour.";
                        txtLivestockColour.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                    }
                    else
                    {
                        OleDbCommand cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.Text;

                        double water, cost, weight, milkwool;

                        string tableName = txtLivestockType.Text;

                        string colour = txtLivestockColour.Text;

                        string lowerCaseInput = colour.ToLower();

                        string capitalizedInput = char.ToUpper(lowerCaseInput[0]) + lowerCaseInput.Substring(1);

                        if (IsTypeValid(tableName, conn))
                        {
                            if (double.TryParse(txtLivestockID.SelectedItem?.ToString(), out id) &&
                            double.TryParse(txtLivestockWater.Text, out water) &&
                            double.TryParse(txtLivestockCost.Text, out cost) &&
                            double.TryParse(txtLivestockWeight.Text, out weight) &&
                            double.TryParse(txtLivestockMilkWool.Text, out milkwool) &&
                            IsEnteredColorValid(txtLivestockColour.Text))
                            {
                                cmd.CommandText = $"SELECT ID FROM {tableName} WHERE ID = {id}";
                                var existingId = cmd.ExecuteScalar();

                                if (existingId != null)
                                {
                                    string milkWoolColumnName = (tableName == "Sheep") ? "Wool" : "Milk";

                                    cmd.CommandText = $"UPDATE {tableName} SET Water=?, Cost=?, Weight=?, Colour=?, {milkWoolColumnName}=? WHERE ID=?";
                                    cmd.Parameters.AddWithValue("Water", water);
                                    cmd.Parameters.AddWithValue("Cost", cost);
                                    cmd.Parameters.AddWithValue("Weight", weight);
                                    cmd.Parameters.AddWithValue("Colour", capitalizedInput);
                                    cmd.Parameters.AddWithValue(milkWoolColumnName, milkwool);
                                    cmd.Parameters.AddWithValue("ID", id);

                                    cmd.ExecuteNonQuery();
                                    conn.Close();
                                    MessageBox.Show("Record Updated Successfully", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Information);
                                    livestockdataviewer();
                                    PopulateComboBoxWithDbIds();
                                    PopulateComboBoxWithDbColours();
                                    txtLivestockID.Text = "";
                                    txtLivestockType.Text = "";
                                    txtLivestockWater.Text = "";
                                    txtLivestockCost.Text = "";
                                    txtLivestockWeight.Text = "";
                                    txtLivestockColour.Text = "";
                                    txtLivestockMilkWool.Text = "";
                                    txtLivestockSearch.Text = "";

                                    foreach (UIElement element in gridLivestock.Children)
                                    {
                                        if (element is TextBox textBox)
                                        {
                                            textBox.Foreground = System.Windows.Media.Brushes.Black;
                                        }
                                    }
                                }
                                else
                                {
                                    txtLivestockID.Text = "ID and Type Mismatch.";
                                    txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;

                                    SystemSounds.Beep.Play();
                                }
                            }
                            else if (!double.TryParse(txtLivestockID.SelectedItem?.ToString(), out id))
                            {
                                txtLivestockID.Text = "Invalid ID.";
                                txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                            }
                            else if (!double.TryParse(txtLivestockWater.Text, out water))
                            {
                                txtLivestockWater.Text = "Enter a number.";
                                txtLivestockWater.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                            }
                            else if (!double.TryParse(txtLivestockCost.Text, out cost))
                            {
                                txtLivestockCost.Text = "Enter a number.";
                                txtLivestockCost.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                            }
                            else if (!double.TryParse(txtLivestockWeight.Text, out weight))
                            {
                                txtLivestockWeight.Text = "Enter a number.";
                                txtLivestockWeight.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                            }
                            else if (!double.TryParse(txtLivestockMilkWool.Text, out milkwool))
                            {
                                txtLivestockMilkWool.Text = "Enter a number.";
                                txtLivestockMilkWool.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                            }
                            else
                            {
                                MessageBox.Show("Invalid Field Input", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                        else if (!double.TryParse(txtLivestockID.Text?.ToString(), out id))
                        {
                            txtLivestockID.Text = "Invalid ID.";
                            txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        else if (txtLivestockType.Text != "Cow" || txtLivestockType.Text != "Goat" || txtLivestockType.Text != "Sheep")
                        {
                            txtLivestockType.Text = "Invalid Type.";
                            txtLivestockType.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnLivestockDelete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtLivestockID.Text))
                {
                    txtLivestockID.Text = "Enter or Select a ID.";
                    txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                    SystemSounds.Beep.Play();
                }
                else
                {
                    using (OleDbConnection conn = new OleDbConnection(connectionString))
                    {
                        conn.Open();
                        string selectedID = txtLivestockID.Text;

                        if (IsIDValid(selectedID, conn))
                        {
                            MessageBoxResult result = MessageBox.Show("Are you sure you want to delete this record?", "LMS GUI Application", MessageBoxButton.YesNo, MessageBoxImage.Information);
                            if (result == MessageBoxResult.Yes)
                            {
                                OleDbCommand cmd = conn.CreateCommand();
                                cmd.CommandType = CommandType.Text;
                                cmd.CommandText = "DELETE FROM Cow WHERE ID = ?";
                                cmd.Parameters.AddWithValue("@ID", selectedID);
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "DELETE FROM Goat WHERE ID = ?";
                                cmd.ExecuteNonQuery();

                                cmd.CommandText = "DELETE FROM Sheep WHERE ID = ?";
                                cmd.ExecuteNonQuery();

                                MessageBox.Show("Record Deleted Successfully", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Information);
                                livestockdataviewer();
                                txtLivestockID.Text = "";
                                txtLivestockType.Text = "";
                                txtLivestockWater.Text = "";
                                txtLivestockCost.Text = "";
                                txtLivestockWeight.Text = "";
                                txtLivestockColour.Text = "";
                                txtLivestockMilkWool.Text = "";
                                txtLivestockSearch.Text = "";
                            }
                        }
                        else
                        {
                            txtLivestockID.Text = "ID Not Found.";
                            txtLivestockID.Foreground = System.Windows.Media.Brushes.Red;
                            SystemSounds.Beep.Play();
                        }
                        conn.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnLivestockClear_Click(object sender, RoutedEventArgs e)
        {
            livestockdataviewer();

            txtLivestockID.Text = "";
            txtLivestockType.Text = "";
            txtLivestockWater.Text = "";
            txtLivestockCost.Text = "";
            txtLivestockWeight.Text = "";
            txtLivestockColour.Text = "";
            txtLivestockMilkWool.Text = "";
            txtLivestockSearch.Text = "";

            foreach (UIElement element in gridLivestock.Children)
            {
                if (element is TextBox textBox)
                {
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
                if (element is ComboBox comboBox)
                {
                    comboBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }

        }
        private void btnLivestockSearch_Click(object sender, RoutedEventArgs e)
        {
            checker = 0;

            string searchText = txtLivestockSearch.Text;

            if (!string.IsNullOrWhiteSpace(searchText) && !int.TryParse(searchText, out _))
            {
                txtLivestockSearch.Text = "Invalid ID.";
                txtLivestockSearch.Foreground = System.Windows.Media.Brushes.Red;
                SystemSounds.Beep.Play();
                livestockdataviewer();
                return;
            }

            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = "SELECT ID, 'Cow' AS Type, Water, Cost, Weight, Colour, Milk AS MilkWool FROM Cow WHERE ID = ?" +
                                      "UNION " +
                                      "SELECT ID, 'Goat' AS Type, Water, Cost, Weight, Colour, Milk AS MilkWool FROM Goat WHERE ID = ?" +
                                      "UNION " +
                                      "SELECT ID, 'Sheep' AS Type, Water, Cost, Weight, Colour, Wool AS MilkWool FROM Sheep WHERE ID = ?";

                    for (int i = 1; i <= 3; i++)
                    {
                        cmd.Parameters.AddWithValue($"ID{i}", txtLivestockSearch.Text);
                    }

                    cmd.ExecuteNonQuery();
                    DataTable dt = new DataTable();
                    OleDbDataAdapter dp = new OleDbDataAdapter(cmd);
                    dp.Fill(dt);
                    checker = Convert.ToInt32(dt.Rows.Count.ToString());
                    dataGridLivestockView.ItemsSource = dt.DefaultView;
                    conn.Close();

                    if (checker == 0)
                    {
                        txtLivestockSearch.Text = "Record not found.";
                        txtLivestockSearch.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                        livestockdataviewer();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void btnLivestockRefresh_Click(object sender, RoutedEventArgs e)
        {
            livestockdataviewer();
        }
        // Commodity
        void commoditydataviewer()
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    using (OleDbCommand cmd = new OleDbCommand("SELECT Item, Price FROM Commodity;", conn))
                    {
                        DataTable dt = new DataTable();
                        OleDbDataAdapter dp = new OleDbDataAdapter(cmd);
                        dp.Fill(dt);

                        dt.Columns.Add("SortOrder", typeof(int));

                        foreach (DataRow row in dt.Rows)
                        {
                            string item = row["Item"].ToString();
                            switch (item)
                            {
                                case "LivestockWeightTax":
                                    row["SortOrder"] = 1;
                                    break;
                                case "Water":
                                    row["SortOrder"] = 2;
                                    break;
                                case "CowMilk":
                                    row["SortOrder"] = 3;
                                    break;
                                case "GoatMilk":
                                    row["SortOrder"] = 4;
                                    break;
                                case "SheepWool":
                                    row["SortOrder"] = 5;
                                    break;
                                default:
                                    row["SortOrder"] = 6;
                                    break;
                            }
                        }

                        dt.DefaultView.Sort = "SortOrder ASC, Price DESC";

                        dataGridCommodityView.ItemsSource = dt.DefaultView;

                        dataGridCommodityView.AutoGeneratingColumn += (sender, e) =>
                        {
                            if (e.PropertyName == "SortOrder")
                            {
                                e.Cancel = true; 
                            }
                        };

                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CommodityDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dictionary<string, TextBox> commodityTextBoxes = new Dictionary<string, TextBox>();

            commodityTextBoxes.Add("LivestockWeightTax", txtCommodityLivestockWeightTax);
            commodityTextBoxes.Add("Water", txtCommodityWaterPrice);
            commodityTextBoxes.Add("CowMilk", txtCommodityCowMilkPrice);
            commodityTextBoxes.Add("GoatMilk", txtCommodityGoatMilkPrice);
            commodityTextBoxes.Add("SheepWool", txtCommoditySheepWoolPrice);

            if (dataGridCommodityView.SelectedItem != null)
            {
                if (dataGridCommodityView.SelectedItem is DataRowView selectedRow)
                {
                    string commodityType = selectedRow["Item"].ToString();
                    double price;

                    if (double.TryParse(selectedRow["Price"].ToString(), out price))
                    {
                        if (commodityTextBoxes.ContainsKey(commodityType))
                        {
                            commodityTextBoxes[commodityType].Text = price.ToString();

                            foreach (TextBox textBox in commodityTextBoxes.Values)
                            {
                                textBox.Foreground = System.Windows.Media.Brushes.Black;
                            }
                        }
                    }
                }
            }
        }
        private void btnCommodityUpdate_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbTransaction transaction = conn.BeginTransaction();
                    OleDbCommand cmd = conn.CreateCommand();
                    cmd.Transaction = transaction;

                    try
                    {
                        double livestockWeightTax;

                        bool hasUpdates = false;

                        if (!string.IsNullOrEmpty(txtCommodityLivestockWeightTax.Text))
                        {
                            if (double.TryParse(txtCommodityLivestockWeightTax.Text, out double newPrice))
                            {
                                UpdateCommodityPrice(cmd, "LiveStockWeightTax", txtCommodityLivestockWeightTax.Text);
                                hasUpdates = true;
                            }
                            else
                            {
                                txtCommodityLivestockWeightTax.Text = "Enter a number.";
                                txtCommodityLivestockWeightTax.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(txtCommodityWaterPrice.Text))
                        {
                            if (double.TryParse(txtCommodityWaterPrice.Text, out double newPrice))
                            {
                                UpdateCommodityPrice(cmd, "Water", txtCommodityWaterPrice.Text);
                                hasUpdates = true;
                            }
                            else
                            {
                                txtCommodityWaterPrice.Text = "Enter a number.";
                                txtCommodityWaterPrice.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(txtCommodityCowMilkPrice.Text))
                        {
                            if (double.TryParse(txtCommodityCowMilkPrice.Text, out double newPrice))
                            {
                                UpdateCommodityPrice(cmd, "CowMilk", txtCommodityCowMilkPrice.Text);
                                hasUpdates = true;
                            }
                            else
                            {
                                txtCommodityCowMilkPrice.Text = "Enter a number.";
                                txtCommodityCowMilkPrice.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(txtCommodityGoatMilkPrice.Text))
                        {
                            if (double.TryParse(txtCommodityGoatMilkPrice.Text, out double newPrice))
                            {
                                UpdateCommodityPrice(cmd, "GoatMilk", txtCommodityGoatMilkPrice.Text);
                                hasUpdates = true;
                            }
                            else
                            {
                                txtCommodityGoatMilkPrice.Text = "Enter a number.";
                                txtCommodityGoatMilkPrice.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                                return;
                            }
                        }
                        if (!string.IsNullOrEmpty(txtCommoditySheepWoolPrice.Text))
                        {
                            if (double.TryParse(txtCommoditySheepWoolPrice.Text, out double newPrice))
                            {
                                UpdateCommodityPrice(cmd, "SheepWool", txtCommoditySheepWoolPrice.Text);
                                hasUpdates = true;
                            }
                            else
                            {
                                txtCommoditySheepWoolPrice.Text = "Enter a number.";
                                txtCommoditySheepWoolPrice.Foreground = System.Windows.Media.Brushes.Red;
                                SystemSounds.Beep.Play();
                                return;
                            }
                        }
                        if (!hasUpdates)
                        {
                            MessageBox.Show("Please update at least one field", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        else
                        {
                            transaction.Commit();
                            MessageBox.Show("Records Updated Successfully", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Information);
                            commoditydataviewer();
                        }
                    }
                    catch (Exception ex)
                    {
                        transaction.Rollback();
                        MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    finally
                    {
                        conn.Close();
                    }
                    txtCommodityLivestockWeightTax.Text = "";
                    txtCommodityWaterPrice.Text = "";
                    txtCommodityCowMilkPrice.Text = "";
                    txtCommodityGoatMilkPrice.Text = "";
                    txtCommoditySheepWoolPrice.Text = "";
                    foreach (UIElement element in gridCommodity.Children)
                    {
                        if (element is TextBox textBox)
                        {
                            textBox.Foreground = System.Windows.Media.Brushes.Black;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void UpdateCommodityPrice(OleDbCommand cmd, string item, string price)
        {
            double livestockWeightTax;
            if (double.TryParse(price, out double newPrice))
            {
                cmd.CommandText = $"UPDATE Commodity SET Price = ? WHERE Item = ?";
                cmd.Parameters.Clear();
                cmd.Parameters.AddWithValue("@Price", newPrice);
                cmd.Parameters.AddWithValue("@Item", item);
                cmd.ExecuteNonQuery();
            }
            else
            {
                throw new Exception($"Invalid Field Input for {item}");
            }
        }
        private void btnCommodityClear_Click(object sender, RoutedEventArgs e)
        {
            commoditydataviewer();

            txtCommodityLivestockWeightTax.Text = "";
            txtCommodityWaterPrice.Text = "";
            txtCommodityCowMilkPrice.Text = "";
            txtCommodityGoatMilkPrice.Text = "";
            txtCommoditySheepWoolPrice.Text = "";

            foreach (UIElement element in gridCommodity.Children)
            {
                if (element is TextBox textBox)
                {
                    textBox.Foreground = System.Windows.Media.Brushes.Black;
                }
            }
        }
        // Colour Query
        private static bool IsColourValid(string colour, OleDbConnection connection)
        {
            string colourCheckQuery = "SELECT COUNT(*) FROM (SELECT Colour FROM Cow UNION ALL " +
                                     "SELECT Colour FROM Goat UNION ALL " +
                                     "SELECT Colour FROM Sheep) AS LivestockColors " +
                                     "WHERE Colour = ?";
            using (OleDbCommand colorCheckCommand = new OleDbCommand(colourCheckQuery, connection))
            {
                colorCheckCommand.Parameters.AddWithValue("@Colour", colour);
                int colourCount = Convert.ToInt32(colorCheckCommand.ExecuteScalar());
                return colourCount > 0;
            }
        }
        bool IsEnteredColorValid(string input)
        {
            try
            {
                System.Drawing.Color color = System.Drawing.ColorTranslator.FromHtml(input);

                return true;
            }
            catch
            {
                return false;
            }
        }
        private void LivestockStatisticsColour(string colour)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    if (IsColourValid(colour, conn))
                    {
                        cmd.CommandText = $"SELECT COUNT(*) FROM Cow WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        int cowCount = (int)cmd.ExecuteScalar();

                        cmd.CommandText = $"SELECT COUNT(*) FROM Goat WHERE Colour = ?";
                        int goatCount = (int)cmd.ExecuteScalar();

                        cmd.CommandText = $"SELECT COUNT(*) FROM Sheep WHERE Colour = ?";
                        int sheepCount = (int)cmd.ExecuteScalar();

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT COUNT(*) FROM Cow";
                        int totalCowCount = (int)cmd.ExecuteScalar();

                        cmd.CommandText = $"SELECT COUNT(*) FROM Goat";
                        int totalGoatCount = (int)cmd.ExecuteScalar();

                        cmd.CommandText = $"SELECT COUNT(*) FROM Sheep";
                        int totalSheepCount = (int)cmd.ExecuteScalar();

                        double livestockWeightTax = 0.0;

                        cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                        cmd.Parameters.AddWithValue("Item", "LivestockWeightTax");
                        object livestockWeightTaxObj = cmd.ExecuteScalar();
                        if (livestockWeightTaxObj != null)
                        {
                            livestockWeightTax = Convert.ToDouble(livestockWeightTaxObj);
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT SUM(Weight) FROM Cow WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        object cowTotalWeightObj = cmd.ExecuteScalar();
                        double cowTotalWeight = cowTotalWeightObj != DBNull.Value ? Convert.ToDouble(cowTotalWeightObj) : 0.0;
                        double cowTotalTax = cowTotalWeight * livestockWeightTax;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT SUM(Weight) FROM Goat WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        object goatTotalWeightObj = cmd.ExecuteScalar();
                        double goatTotalWeight = goatTotalWeightObj != DBNull.Value ? Convert.ToDouble(goatTotalWeightObj) : 0.0;
                        double goatTotalTax = goatTotalWeight * livestockWeightTax;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT SUM(Weight) FROM Sheep WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        object sheepTotalWeightObj = cmd.ExecuteScalar();
                        double sheepTotalWeight = sheepTotalWeightObj != DBNull.Value ? Convert.ToDouble(sheepTotalWeightObj) : 0.0;
                        double sheepTotalTax = sheepTotalWeight * livestockWeightTax;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT SUM(Milk) FROM Cow WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);

                        object cowTotalMilkObj = cmd.ExecuteScalar();
                        double cowTotalMilk = cowTotalMilkObj != DBNull.Value ? Convert.ToDouble(cowTotalMilkObj) : 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT SUM(Milk) FROM Goat WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);

                        object goatTotalMilkObj = cmd.ExecuteScalar();
                        double goatTotalMilk = goatTotalMilkObj != DBNull.Value ? Convert.ToDouble(goatTotalMilkObj) : 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $"SELECT SUM(Wool) FROM Sheep WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);

                        object sheepTotalWoolObj = cmd.ExecuteScalar();
                        double sheepTotalWool = sheepTotalWoolObj != DBNull.Value ? Convert.ToDouble(sheepTotalWoolObj) : 0.0;

                        double cowMilkPrice = 0.0;
                        double goatMilkPrice = 0.0;
                        double sheepWoolPrice = 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                        cmd.Parameters.AddWithValue("Item", "CowMilk");

                        object cowMilkPriceObj = cmd.ExecuteScalar();
                        if (cowMilkPriceObj != null)
                        {
                            cowMilkPrice = Convert.ToDouble(cowMilkPriceObj);
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                        cmd.Parameters.AddWithValue("Item", "GoatMilk");

                        object goatMilkPriceObj = cmd.ExecuteScalar();
                        if (goatMilkPriceObj != null)
                        {
                            goatMilkPrice = Convert.ToDouble(goatMilkPriceObj);
                        }

                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                        cmd.Parameters.AddWithValue("Item", "SheepWool");

                        object sheepWoolPriceObj = cmd.ExecuteScalar();
                        if (sheepWoolPriceObj != null)
                        {
                            sheepWoolPrice = Convert.ToDouble(sheepWoolPriceObj);
                        }

                        double waterPrice = 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                        cmd.Parameters.AddWithValue("Item", "Water");
                        object waterPriceObj = cmd.ExecuteScalar();
                        if (waterPriceObj != null)
                        {
                            waterPrice = Convert.ToDouble(waterPriceObj);
                        }

                        double totalCowCost = 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalCowCost FROM Cow WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        object totalCowCostObj = cmd.ExecuteScalar();
                        if (totalCowCostObj != DBNull.Value && totalCowCostObj != null)
                        {
                            totalCowCost = Convert.ToDouble(totalCowCostObj);
                        }

                        double totalGoatCost = 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalGoatCost FROM Goat WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        object totalGoatCostObj = cmd.ExecuteScalar();
                        if (totalGoatCostObj != DBNull.Value && totalGoatCostObj != null)
                        {
                            totalGoatCost = Convert.ToDouble(totalGoatCostObj);
                        }

                        double totalSheepCost = 0.0;

                        cmd.Parameters.Clear();
                        cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalSheepCost FROM Sheep WHERE Colour = ?";
                        cmd.Parameters.AddWithValue("Colour", colour);
                        object totalSheepCostObj = cmd.ExecuteScalar();
                        if (totalSheepCostObj != DBNull.Value && totalSheepCostObj != null)
                        {
                            totalSheepCost = Convert.ToDouble(totalSheepCostObj);
                        }

                        double cowTotalIncome = cowTotalMilk * cowMilkPrice;

                        double goatTotalIncome = goatTotalMilk * goatMilkPrice;

                        double sheepTotalIncome = sheepTotalWool * sheepWoolPrice;

                        int animalsInColour = cowCount + goatCount + sheepCount;

                        int totalAnimals = totalCowCount + totalGoatCount + totalSheepCount;

                        double percentage = (double)animalsInColour / totalAnimals * 100;

                        double totalTax = cowTotalTax + goatTotalTax + sheepTotalTax;

                        double totalIncome = cowTotalIncome + goatTotalIncome + sheepTotalIncome;

                        double totalCost = totalCowCost + totalGoatCost + totalSheepCost;

                        double totalProfitLoss = totalIncome - totalCost;

                        conn.Close();

                        if (totalProfitLoss > 0)
                        {
                            var messageText = $"Number of animals in colour {colour}: {animalsInColour}\n" +
                                              $"Percentage of animals with colour {colour}: {percentage:F2}%\n" +
                                              $"Total Tax per day by {colour} animals: {totalTax:C}\n" +
                                              $"Total income generated by {colour}: {totalIncome:C}\n" +
                                              $"Total cost generated by {colour}: {totalCost:C}\n\n" +
                                              $"Profit: ${totalProfitLoss.ToString("F2")}";

                            var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                            if (response == MessageBoxResult.Yes)
                            {
                                Clipboard.SetText(messageText);
                            }
                        }
                        else if (totalProfitLoss < 0)
                        {
                            var messageText = $"Number of animals in colour {colour}: {animalsInColour}\n" +
                                              $"Percentage of animals with colour {colour}: {percentage:F2}%\n" +
                                              $"Total Tax per day by {colour} animals: {totalTax:C}\n" +
                                              $"Total income generated by {colour}: {totalIncome:C}\n" +
                                              $"Total cost generated by {colour}: {totalCost:C}\n\n" +
                                              $"Loss: ${totalProfitLoss.ToString("F2")}";

                            var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                            if (response == MessageBoxResult.Yes)
                            {
                                Clipboard.SetText(messageText);
                            }
                        }
                        txtStatisticsColour.Text = "";
                    }
                    else if (string.IsNullOrWhiteSpace(colour))
                    {
                        // Skips showing info
                    }
                    else if (!IsColourValid(colour, conn))
                    {
                        txtStatisticsColour.Text = "Invalid Colour.";
                        txtStatisticsColour.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                    }
                    else
                    {
                        MessageBox.Show($"Invalid Colour", "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Type Query
        private static bool IsTypeValid(string type, OleDbConnection connection)
        {
            DataTable schema = connection.GetSchema("Tables");
            foreach (DataRow row in schema.Rows)
            {
                string tableName = row["TABLE_NAME"].ToString();
                if (tableName.Equals(type, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        private void LivestockStatisticsType(string type)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    string lowercaseLivestockStatisticsType = txtStatisticsType.Text.ToLower();

                    double livestockWeightTax = 0.0;

                    cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
                    cmd.Parameters.AddWithValue("Item", "LivestockWeightTax");
                    object livestockWeightTaxObj = cmd.ExecuteScalar();
                    if (livestockWeightTaxObj != null)
                    {
                        livestockWeightTax = Convert.ToDouble(livestockWeightTaxObj);
                    }

                    if (IsTypeValid(type, conn))
                    {
                        if (lowercaseLivestockStatisticsType == "cow")
                        {
                            cmd.CommandText = $"SELECT SUM(Milk) FROM Cow";
                            double cowProduceTotal = (double)cmd.ExecuteScalar();

                            cmd.CommandText = $"SELECT SUM(Water) FROM Cow";
                            double cowWaterTotal = (double)cmd.ExecuteScalar();

                            cmd.CommandText = $"SELECT SUM(Weight) FROM Cow";
                            double cowWeightTotal = (double)cmd.ExecuteScalar();

                            double cowTotalTax = cowWeightTotal * livestockWeightTax;

                            var messageText = $"Produce amount for {lowercaseLivestockStatisticsType}: {cowProduceTotal}\n" +
                                              $"Water consumption for {lowercaseLivestockStatisticsType}: {cowWaterTotal}\n" +
                                              $"Tax for {lowercaseLivestockStatisticsType}: {cowTotalTax:C}";

                            var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                            if (response == MessageBoxResult.Yes)
                            {
                                Clipboard.SetText(messageText);
                            }
                        }
                        else if (lowercaseLivestockStatisticsType == "goat")
                        {
                            cmd.CommandText = $"SELECT SUM(Milk) FROM Goat";
                            double goatProduceTotal = (double)cmd.ExecuteScalar();

                            cmd.CommandText = $"SELECT SUM(Water) FROM Goat";
                            double goatWaterTotal = (double)cmd.ExecuteScalar();

                            cmd.CommandText = $"SELECT SUM(Weight) FROM Goat";
                            double goatWeightTotal = (double)cmd.ExecuteScalar();

                            double goatTotalTax = goatWeightTotal * livestockWeightTax;

                            var messageText = $"Produce amount for {lowercaseLivestockStatisticsType}: {goatProduceTotal}\n" +
                                              $"Water consumption for {lowercaseLivestockStatisticsType}: {goatWaterTotal}\n" +
                                              $"Tax for {lowercaseLivestockStatisticsType}: {goatTotalTax:C}";

                            var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                            if (response == MessageBoxResult.Yes)
                            {
                                Clipboard.SetText(messageText);
                            }
                        }
                        else if (lowercaseLivestockStatisticsType == "sheep")
                        {
                            cmd.CommandText = $"SELECT SUM(Wool) FROM Sheep";
                            double sheepProduceTotal = (double)cmd.ExecuteScalar();

                            cmd.CommandText = $"SELECT SUM(Water) FROM Sheep";
                            double sheepWaterTotal = (double)cmd.ExecuteScalar();

                            cmd.CommandText = $"SELECT SUM(Weight) FROM Sheep";
                            double sheepWeightTotal = (double)cmd.ExecuteScalar();

                            double sheepTotalTax = sheepWeightTotal * livestockWeightTax;

                            var messageText = $"Produce amount for {lowercaseLivestockStatisticsType}: {sheepProduceTotal}\n" +
                                              $"Water consumption for {lowercaseLivestockStatisticsType}: {sheepWaterTotal}\n" +
                                              $"Tax for {lowercaseLivestockStatisticsType}: {sheepTotalTax:C}";

                            var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                            if (response == MessageBoxResult.Yes)
                            {
                                Clipboard.SetText(messageText);
                            }
                        }
                        txtStatisticsType.Text = "";
                    }
                    else if (string.IsNullOrWhiteSpace(type))
                    {
                        // Skips showing info
                    }
                    else if (!IsTypeValid(type, conn))
                    {
                        txtStatisticsType.Text = "Invalid Type.";
                        txtStatisticsType.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                    }
                    else
                    {
                        MessageBox.Show($"Invalid Type", "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Weight Query
        private static bool IsWeightValid(string weightThreshold)
        {
            if (string.IsNullOrWhiteSpace(weightThreshold))
            {
                return false;
            }

            if (double.TryParse(weightThreshold, out double result))
            {
                return true; 
            }

            return false; 
        }
        private void LivestockStatisticsWeight(string weightThreshold)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    if (IsWeightValid(weightThreshold))
                    {
                        conn.Open();
                        OleDbCommand cmd = conn.CreateCommand();
                        cmd.CommandType = CommandType.Text;

                        cmd.CommandText = "SELECT AVG(Weight) AS AvgWeight " +
                                          "FROM (SELECT Weight FROM Cow UNION ALL " +
                                          "      SELECT Weight FROM Goat UNION ALL " +
                                          "      SELECT Weight FROM Sheep) AS LivestockWeight " +
                                          "WHERE Weight > ?";
                        cmd.Parameters.AddWithValue("@Weight", weightThreshold);

                        object avgWeightObj = cmd.ExecuteScalar();

                        if (avgWeightObj != DBNull.Value)
                        {
                            double avgWeight = Convert.ToDouble(avgWeightObj);

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Weight", weightThreshold);
                            cmd.CommandText = $"SELECT SUM(Milk) FROM Cow WHERE Weight > ?";
                            object cowMilkTotalObj = cmd.ExecuteScalar();
                            double cowMilkTotal = GetDoubleValue(cowMilkTotalObj);

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Weight", weightThreshold);
                            cmd.CommandText = $"SELECT SUM(Milk) FROM Goat WHERE Weight > ?";
                            object goatMilkTotalObj = cmd.ExecuteScalar();
                            double goatMilkTotal = GetDoubleValue(goatMilkTotalObj);

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Weight", weightThreshold);
                            cmd.CommandText = $"SELECT SUM(Wool) FROM Sheep WHERE Weight > ?";
                            object sheepWoolTotalObj = cmd.ExecuteScalar();
                            double sheepWoolTotal = GetDoubleValue(sheepWoolTotalObj);

                            double cowMilkPrice = GetCommodityPrice(cmd, "CowMilk");
                            double goatMilkPrice = GetCommodityPrice(cmd, "GoatMilk");
                            double sheepWoolPrice = GetCommodityPrice(cmd, "SheepWool");

                            double totalIncome = (cowMilkTotal * cowMilkPrice) + (goatMilkTotal * goatMilkPrice) + (sheepWoolTotal * sheepWoolPrice);

                            double waterPrice = GetCommodityPrice(cmd, "Water");
                            double livestockWeightTax = GetCommodityPrice(cmd, "livestockWeightTax");

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Weight", weightThreshold);
                            cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalCowCost FROM Cow WHERE Weight > ?";
                            object totalCowCostObj = cmd.ExecuteScalar();
                            double totalCowCost = GetDoubleValue(totalCowCostObj);

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Weight", weightThreshold);
                            cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalGoatCost FROM Goat WHERE Weight > ?";
                            object totalGoatCostObj = cmd.ExecuteScalar();
                            double totalGoatCost = GetDoubleValue(totalGoatCostObj);

                            cmd.Parameters.Clear();
                            cmd.Parameters.AddWithValue("@Weight", weightThreshold);
                            cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalSheepCost FROM Sheep WHERE Weight > ?";
                            object totalSheepCostObj = cmd.ExecuteScalar();
                            double totalSheepCost = GetDoubleValue(totalSheepCostObj);

                            double totalCost = totalCowCost + totalGoatCost + totalSheepCost;

                            double totalProfitLoss = totalIncome - totalCost;

                            if (totalProfitLoss > 0)
                            {
                                var messageText = $"Operation income of livestock above {weightThreshold}kg: {totalIncome:C}\n" +
                                                  $"Operation cost of livestock above {weightThreshold}kg: {totalCost:C}\n" +
                                                  $"Average weight of livestock above {weightThreshold}kg: {avgWeight:F2}kg\n\n" +
                                                  $"Profit: ${totalProfitLoss.ToString("F2")}";

                                var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                                if (response == MessageBoxResult.Yes)
                                {
                                    Clipboard.SetText(messageText);
                                }
                            }
                            if (totalProfitLoss < 0) 
                            {
                                var messageText = $"Operation income of livestock above {weightThreshold}kg: {totalIncome:C}\n" +
                                                  $"Operation cost of livestock above {weightThreshold}kg: {totalCost:C}\n" +
                                                  $"Average weight of livestock above {weightThreshold}kg: {avgWeight:F2}kg\n\n" +
                                                  $"Loss: ${totalProfitLoss.ToString("F2")}";
                                
                                var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                                if (response == MessageBoxResult.Yes)
                                {
                                    Clipboard.SetText(messageText);
                                }
                            }
                            txtStatisticsWeight.Text = "";
                        }
                        else
                        {
                            MessageBox.Show($"No livestock found above {weightThreshold}kg", "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Information);
                        }

                    }
                    else if (string.IsNullOrWhiteSpace(weightThreshold))
                    {
                        // Skips showing info
                    }
                    else if (!IsWeightValid(weightThreshold))
                    {
                        txtStatisticsWeight.Text = "Invalid Weight Threshold.";
                        txtStatisticsWeight.Foreground = System.Windows.Media.Brushes.Red;
                        SystemSounds.Beep.Play();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        // Operation Statistics
        private void LivestockStatisticsOperation(object sender, RoutedEventArgs e)
        {
            try
            {
                using (OleDbConnection conn = new OleDbConnection(connectionString))
                {
                    conn.Open();
                    OleDbCommand cmd = conn.CreateCommand();
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = "SELECT SUM(Weight) AS totalWeight FROM (SELECT Weight FROM Cow UNION ALL SELECT Weight FROM Goat UNION ALL SELECT Weight FROM Sheep) AS AllLivestock";
                    object totalWeightObj = cmd.ExecuteScalar();
                    double totalWeight = GetDoubleValue(totalWeightObj);

                    double livestockWeightTax = GetCommodityPrice(cmd, "LivestockWeightTax");

                    double totalMonthlyWeightTax = (totalWeight * livestockWeightTax) * 30;

                    cmd.Parameters.Clear();
                    cmd.CommandText = "SELECT SUM(TotalIncome) AS totalIncome FROM (" +
                                      "  SELECT (SUM(Milk) * (SELECT Price FROM Commodity WHERE Item = 'CowMilk')) AS TotalIncome FROM Cow UNION ALL " +
                                      "  SELECT (SUM(Milk) * (SELECT Price FROM Commodity WHERE Item = 'GoatMilk')) AS TotalIncome FROM Goat UNION ALL " +
                                      "  SELECT (SUM(Wool) * (SELECT Price FROM Commodity WHERE Item = 'SheepWool')) AS TotalIncome FROM Sheep) AS AllIncome";

                    object totalIncomeObj = cmd.ExecuteScalar();
                    double totalIncome = GetDoubleValue(totalIncomeObj);

                    double waterPrice = GetCommodityPrice(cmd, "Water");

                    cmd.Parameters.Clear();
                    cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalCowCost FROM Cow";
                    object totalCowCostObj = cmd.ExecuteScalar();
                    double totalCowCost = GetDoubleValue(totalCowCostObj);

                    cmd.Parameters.Clear();
                    cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalGoatCost FROM Goat";
                    object totalGoatCostObj = cmd.ExecuteScalar();
                    double totalGoatCost = GetDoubleValue(totalGoatCostObj);

                    cmd.Parameters.Clear();
                    cmd.CommandText = $@"SELECT SUM(Cost +(Water * {waterPrice}) +(Weight * {livestockWeightTax})) AS totalSheepCost FROM Sheep";
                    object totalSheepCostObj = cmd.ExecuteScalar();
                    double totalSheepCost = GetDoubleValue(totalSheepCostObj);

                    cmd.Parameters.Clear();
                    cmd.CommandType = CommandType.Text;

                    cmd.CommandText = "SELECT AVG(Weight) AS AvgWeight " +
                                      "FROM (SELECT Weight FROM Cow UNION ALL " +
                                      "      SELECT Weight FROM Goat UNION ALL " +
                                      "      SELECT Weight FROM Sheep) AS LivestockWeight ";

                    object avgWeightObj = cmd.ExecuteScalar();

                    double avgWeight = Convert.ToDouble(avgWeightObj);

                    double totalCost = totalCowCost + totalGoatCost + totalSheepCost;

                    double totalProfitLoss = totalIncome - totalCost;

                    if (totalProfitLoss > 0)
                    {
                        var messageText = $"Total monthly tax for all animals: {totalMonthlyWeightTax:C}\n" +
                                          $"Total income for all animals: {totalIncome:C}\n" +
                                          $"Total cost for all animals: {totalCost:C}\n" +
                                          $"Average weight of livestock: {avgWeight:F2}kg\n\n" +
                                          $"Profit: ${totalProfitLoss.ToString("F2")}";

                        var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                        if (response == MessageBoxResult.Yes)
                        {
                            Clipboard.SetText(messageText);
                        }
                    }
                    else if (totalProfitLoss < 0)
                    {
                        var messageText = $"Total monthly tax for all animals: {totalMonthlyWeightTax:C}\n" +
                                          $"Total income for all animals: {totalIncome:C}\n" +
                                          $"Total cost for all animals: {totalCost:C}\n" +
                                          $"Average weight of livestock: {avgWeight:F2}kg\n\n" +
                                          $"Loss: ${totalProfitLoss.ToString("F2")}";

                        var response = MessageBox.Show(messageText + "\r\n\r\nCopy to clipboard?", "Livestock Statistics Report", MessageBoxButton.YesNo);

                        if (response == MessageBoxResult.Yes)
                        {
                            Clipboard.SetText(messageText);
                        }
                    }

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private double GetDoubleValue(object value)
        {
            if (value != null && value != DBNull.Value)
            {
                double result;
                if (double.TryParse(value.ToString(), out result))
                {
                    return result;
                }
                else
                {
                    MessageBox.Show($"Failed to convert {value.GetType().Name} to double.", "LMS GUI Application", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            return 0.0;
        }
        private double GetCommodityPrice(OleDbCommand cmd, string item)
        {
            cmd.Parameters.Clear();
            cmd.CommandText = "SELECT Price FROM Commodity WHERE Item = ?";
            cmd.Parameters.AddWithValue("@Item", item);
            object priceObj = cmd.ExecuteScalar();
            return GetDoubleValue(priceObj);
        }
        private void btnStatisticsCalculate_Click(object sender, RoutedEventArgs e)
        { 
            string color = txtStatisticsColour.Text;
            string type = txtStatisticsType.Text;
            string weightThreshold = txtStatisticsWeight.Text;

            LivestockStatisticsColour(color);

            LivestockStatisticsType(type);

            LivestockStatisticsWeight(weightThreshold);

            if (checkboxStatisticsOperationReport != null && checkboxStatisticsOperationReport.IsChecked == true)
            {
                LivestockStatisticsOperation(sender, e);
            }
            if (string.IsNullOrWhiteSpace(color) &&
                string.IsNullOrWhiteSpace(type) &&
                string.IsNullOrWhiteSpace(weightThreshold) &&
                checkboxStatisticsOperationReport.IsChecked == false)
            {
                MessageBox.Show("Please enter at least one field.", "Livestock Statistics Report", MessageBoxButton.OK, MessageBoxImage.Error);
                txtStatisticsColour.Text = "";
                txtStatisticsColour.Foreground = System.Windows.Media.Brushes.Black;
                txtStatisticsType.Text = "";
                txtStatisticsType.Foreground = System.Windows.Media.Brushes.Black;
                txtStatisticsWeight.Text = "";
                txtStatisticsWeight.Foreground = System.Windows.Media.Brushes.Black;
            }
            checkboxStatisticsOperationReport.IsChecked = false;
        }
    }
}