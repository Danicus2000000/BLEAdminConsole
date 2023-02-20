using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.Json;
using System.IO;
using System.Windows.Threading;
using System;
using System.Windows.Input;

namespace AdminConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string loggedInCompany = "";
        private int loginAttmepts = 0;
        private DispatcherTimer timer=new DispatcherTimer();
        public MainWindow()
        {
            InitializeComponent();
        }
        private (string, string) readJsonFile()//read in username and password for database access from JSON file
        {
            string text = File.ReadAllText("pass.json");
            user user = JsonSerializer.Deserialize<user>(text);
            return (user.username, user.pass);
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            organisation_cmb.Items.Clear();
            (string username, string password) = readJsonFile();
            string connectionString = "Server=bledata.mysql.database.azure.com; " +
            "Database=bledata; Uid=" + username + "; Pwd=" + password + ";";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand selectCommand = new MySqlCommand("SELECT organisationName FROM organisations", conn);
                MySqlDataReader results = selectCommand.ExecuteReader();
                while (results.Read())
                {
                    organisation_cmb.Items.Add(results[0]);
                }
            }
            organisation_cmb.SelectedIndex = 0;
        }
        private void organisationPassword_txt_KeyDown(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if ((e.Key == Key.Enter || e.Key == Key.Return) && !timer.IsEnabled)
            {
                login_btn_Click(sender, e);
            }
        }
        private void CommandBinding_CanExecutePaste(object sender, CanExecuteRoutedEventArgs e)//stops content being pasted into password box
        {
            e.CanExecute = false;
            e.Handled = true;
        }
        private void login_btn_Click(object sender, RoutedEventArgs e)
        {
            (string username, string password) = readJsonFile();
            string connectionString = "Server=bledata.mysql.database.azure.com; " +
            "Database=bledata; Uid=" + username + "; Pwd=" + password + ";";
            int validPassword = 0;
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                string passwordset = organisationPassword_txt.Password.ToString().Replace("'", "\\'");
                MySqlCommand selectCommand = new MySqlCommand("SELECT COUNT(OrganisationName) FROM organisations WHERE OrganisationName='" + organisation_cmb.SelectedValue.ToString().Replace("'", "\\'") + "' AND OrganisationPassword='" + organisationPassword_txt.Password.ToString().Replace("'","\\'") + "'", conn);
                MySqlDataReader results = selectCommand.ExecuteReader();
                while (results.Read())
                {
                    validPassword = int.Parse(results[0].ToString());
                }
            }
            organisationPassword_txt.Password = "";
            if (validPassword > 0)
            {
                loggedInCompany = organisation_cmb.SelectedValue.ToString().Replace("'","\\'");
                loadBeacons();
                OrganisationPassword_lbl.Visibility = Visibility.Hidden;
                organisation_cmb.Visibility = Visibility.Hidden;
                organisationPassword_txt.Visibility = Visibility.Hidden;
                organisation_lbl.Visibility = Visibility.Hidden;
                login_btn.Visibility = Visibility.Hidden;
                exit_btn.Visibility = Visibility.Hidden;
                beacon_cmb.Visibility = Visibility.Visible;
                beacon_lbl.Visibility = Visibility.Visible;
                beaconURL_lbl.Visibility = Visibility.Visible;
                beaconURLActual_lbl.Visibility = Visibility.Visible;
                beaconNewURL_lbl.Visibility = Visibility.Visible;
                beaconNewURL_txt.Visibility = Visibility.Visible;
                beaconNewTitle_lbl.Visibility = Visibility.Visible;
                beaconNewTitle_txt.Visibility = Visibility.Visible;
                updateBeacon_btn.Visibility = Visibility.Visible;
                logOut_btn.Visibility = Visibility.Visible;
            }
            else if (loginAttmepts < 2)
            {
                loginAttmepts++;
                MessageBox.Show("Invalid Password.\n" + (3 - loginAttmepts) + " login attempts remaining.", "Password Error");
            }
            else
            {
                MessageBox.Show("You have been locked out of login attemtps for 1 minuite");
                loginAttmepts = 0;
                login_btn.Click -= login_btn_Click;
                timer.Interval = new System.TimeSpan(0, 1, 0);
                timer.Tick += reEnableLogin;
                timer.Start();
            }
        }
        private void reEnableLogin(object sender, EventArgs e)
        {
            login_btn.Click += login_btn_Click;
            timer.Stop();
        }
        private void loadBeacons()
        {
            beacon_cmb.Items.Clear();
            (string username, string password) = readJsonFile();
            string connectionString = "Server=bledata.mysql.database.azure.com; " +
            "Database=bledata; Uid=" + username + "; Pwd=" + password + ";";
            using (MySqlConnection conn = new MySqlConnection(connectionString))
            {
                conn.Open();
                MySqlCommand selectCommand = new MySqlCommand("SELECT BleDeviceTitle FROM bledevices WHERE BleDeviceOrganisation='" + loggedInCompany + "'",conn);
                MySqlDataReader results = selectCommand.ExecuteReader();
                while (results.Read())
                {
                    beacon_cmb.Items.Add(results[0].ToString());
                }
            }
            beacon_cmb.SelectedIndex = 0;
            updateBeaconURL();
        }
        private void beacon_cmb_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            updateBeaconURL();
        }
        private void updateBeaconURL() 
        {
            if (beacon_cmb.Items.Count > 0)
            {
                (string username, string password) = readJsonFile();
                string connectionString = "Server=bledata.mysql.database.azure.com; " +
                "Database=bledata; Uid=" + username + "; Pwd=" + password + ";";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand selectCommand = new MySqlCommand("SELECT BleDeviceUrlToPointTo FROM bledevices WHERE BleDeviceOrganisation='" + loggedInCompany + "' AND BleDeviceTitle='" + beacon_cmb.SelectedValue.ToString().Replace("'", "\\'") + "'", conn);
                    MySqlDataReader results = selectCommand.ExecuteReader();
                    while (results.Read())
                    {
                        beaconURLActual_lbl.Content = results[0].ToString();
                    }
                }
                beaconURLActual_lbl.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size s = beaconURLActual_lbl.DesiredSize;
                if (s.Width > 20)
                {
                    beacon_cmb.Width = s.Width;
                    beaconNewTitle_txt.Width = s.Width;
                    beaconNewURL_txt.Width = s.Width;
                }
            }
        }
        private void updateBeacon_btn_Click(object sender, RoutedEventArgs e)
        {
            //update database with new data
            string newBeaconTitle=beaconNewTitle_txt.Text;
            string newBeaconUrl = beaconNewURL_txt.Text;
            if(newBeaconTitle=="" && newBeaconUrl=="")
            {
                MessageBox.Show("No Changes proposed!", "No input changes");
                return;
            }
            string messageContent = "";
            string messsageTitle = "";
            string query = "";
            if (newBeaconTitle != "" & newBeaconUrl != "")
            {
                messageContent = "Update beacon URL to be: " + newBeaconUrl + " and beacon Title to be: " + newBeaconTitle + " ?";
                messsageTitle = "Update beacon URL & Title?";
                query= "UPDATE bledevices SET BleDeviceUrlToPointTo='" + newBeaconUrl + "', BleDeviceTitle='" + newBeaconTitle + "' WHERE bleDeviceTitle='" + beacon_cmb.SelectedValue.ToString().Replace("'", "\\'") + "' AND BleDeviceOrganisation='"+loggedInCompany+"'";
            }
            else if (newBeaconTitle != "")
            {
                messageContent = "Update beacon Title to be: " + newBeaconTitle + " ?";
                messsageTitle = "Update beacon Title?";
                query = "UPDATE bledevices SET BleDeviceTitle='" + newBeaconTitle + "' WHERE bleDeviceTitle='" + beacon_cmb.SelectedValue.ToString().Replace("'", "\\'") + "' AND BleDeviceOrganisation='" + loggedInCompany + "'";
            }
            else if (newBeaconUrl != "") 
            {
                messageContent = "Update beacon URL to be: " + newBeaconUrl + " ?";
                messsageTitle = "Update beacon URL?";
                query = "UPDATE bledevices SET BleDeviceUrlToPointTo='" + newBeaconUrl + "' WHERE bleDeviceTitle='" + beacon_cmb.SelectedValue.ToString().Replace("'", "\\'") + "' AND BleDeviceOrganisation='" + loggedInCompany + "'";
            }
            MessageBoxResult messageResult = MessageBox.Show(messageContent, messsageTitle, MessageBoxButton.YesNo);
            if (messageResult == MessageBoxResult.Yes)
            {
                (string username, string password) = readJsonFile();
                string connectionString = "Server=bledata.mysql.database.azure.com; " +
                "Database=bledata; Uid=" + username + "; Pwd=" + password + ";";
                using (MySqlConnection conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                    MySqlCommand selectCommand = new MySqlCommand(query, conn);
                    selectCommand.ExecuteNonQuery();
                }
            }
            loadBeacons();
            beaconNewTitle_txt.Text = "";
            beaconNewURL_txt.Text = "";
            MessageBox.Show("Beacon Update Successful", "Update Complete");
        }
        private void logOut_btn_Click(object sender, RoutedEventArgs e)//when logout button is pressed return to login page
        {
            loggedInCompany = "";
            loginAttmepts = 0;
            OrganisationPassword_lbl.Visibility = Visibility.Visible;
            organisation_cmb.Visibility = Visibility.Visible;
            organisationPassword_txt.Visibility = Visibility.Visible;
            organisation_lbl.Visibility = Visibility.Visible;
            exit_btn.Visibility = Visibility.Visible;
            login_btn.Visibility = Visibility.Visible;
            beacon_cmb.Visibility = Visibility.Hidden;
            beacon_lbl.Visibility = Visibility.Hidden;
            beaconURL_lbl.Visibility = Visibility.Hidden;
            beaconURLActual_lbl.Visibility = Visibility.Hidden;
            beaconNewURL_lbl.Visibility = Visibility.Hidden;
            beaconNewURL_txt.Visibility = Visibility.Hidden;
            beaconNewTitle_lbl.Visibility = Visibility.Hidden;
            beaconNewTitle_txt.Visibility = Visibility.Hidden;
            updateBeacon_btn.Visibility = Visibility.Hidden;
            logOut_btn.Visibility = Visibility.Hidden;
            Window_Loaded(this, e);
        }

        private void exit_btn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
    }
    internal class user //the retrieved data layout from the pass file
    {
        public string username { get; set; }
        public string pass { get; set; }
    }
}