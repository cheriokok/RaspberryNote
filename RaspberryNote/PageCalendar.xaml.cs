using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RaspberryNote
{
    /// <summary>
    /// Логика взаимодействия для PageCalendar.xaml
    /// </summary>
    public partial class PageCalendar : Page
    {
        private const string connectionString = "data source=HONOR_RINAOUKO\\MSSQLSERVER01;initial catalog=RaspberryNote;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";
        private Frame mainFrame;

        public PageCalendar()
        {
            InitializeComponent();
            LoadAllFolders();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = new MainWindow();
            mainWindow.Show();
            Window currentWindow = Window.GetWindow(this);
            currentWindow?.Close();
        }

        private void LoadAllFolders()
        {
            try
            {
                LoadDayData("Monday", MondayGrid);
                LoadDayData("Tuesday", TuesdayGrid);
                LoadDayData("Wednesday", WednesdayGrid);
                LoadDayData("Thursday", ThursdayGrid);
                LoadDayData("Friday", FridayGrid);
                LoadDayData("Saturday", SaturdayGrid);
                LoadDayData("Sunday", SundayGrid);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке всех данных: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadDayData(string dayTableName, DataGrid dataGrid)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"SELECT 
                    DNoteDescription,
                    DNoteCategory
                    FROM {dayTableName}";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке данных для {dayTableName}: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
