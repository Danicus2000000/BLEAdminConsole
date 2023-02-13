using MySql.Data.MySqlClient;
using System.Windows;
using System.Text.Json;
using System.IO;
namespace AdminConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool credentialsRead = true;
        public MainWindow()
        {
            InitializeComponent();
        }
        private void updateBeacon_btn_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            (string username, string password) = readJsonFile("pass.json");
            if (credentialsRead)
            {
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
                    organisation_cmb.SelectedIndex = 0;
                }
            }
            
        }
        private (string, string) readJsonFile(string fileName)
        {
            string text = File.ReadAllText(fileName);
            user user = JsonSerializer.Deserialize<user>(text);
            return (user.username, user.pass);
        }
    }
    internal class user 
    {
        public string username { get; set; }
        public string pass { get; set; }
    }
}