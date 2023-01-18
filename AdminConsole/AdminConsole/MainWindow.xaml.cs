using MySqlConnector;
using System.Threading.Tasks;
using System.Windows;
using System.Collections.Generic;
namespace AdminConsole
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void updateBeacon_btn_Click(object sender, RoutedEventArgs e)
        {

        }
        private static async Task<List<string>> GetOrganisations()
        {
            var builder = new MySqlConnectionStringBuilder
            {
                Server = "bledata.mysql.database.azure.com",
                Database = "bledata",
                UserID = "",
                Password = "",
                SslMode = MySqlSslMode.Required
            };
            List<string> returnTo = new List<string>();
            using (var conn = new MySqlConnection(builder.ConnectionString))
            {
                await conn.OpenAsync();

                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "select organisationName from organisation";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            returnTo.Add(reader.GetString(0));
                        }
                    }
                }
            }
            return returnTo;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            organisation_cmb.Items.Add(GetOrganisations().Result);
        }
    }
}