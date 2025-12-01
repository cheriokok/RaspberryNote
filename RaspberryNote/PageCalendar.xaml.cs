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
    public partial class PageCalendar : Page
    {
        private const string connectionString = "data source=HONOR_RINAOUKO\\MSSQLSERVER01;initial catalog=RaspberryNote;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";

        // Переменные для хранения выбранной записи
        private DataGrid selectedDataGrid;
        private DataRowView selectedRow;
        private string selectedDayTableName;

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

        // Обработчик выбора строки в DataGrid
        private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.SelectedItem != null)
            {
                selectedDataGrid = dataGrid;
                selectedRow = dataGrid.SelectedItem as DataRowView;

                // Определяем название таблицы дня по имени DataGrid
                selectedDayTableName = dataGrid.Name.Replace("Grid", "");
            }
        }

        // Обработчик кнопки удаления
        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (selectedRow == null || selectedDataGrid == null)
            {
                MessageBox.Show("Пожалуйста, выберите запись для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string description = selectedRow["DNoteDescription"]?.ToString();
            string category = selectedRow["DNoteCategory"]?.ToString();

            if (string.IsNullOrEmpty(description))
            {
                MessageBox.Show("Не удалось получить данные выбранной записи", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            // Подтверждение удаления
            MessageBoxResult result = MessageBox.Show(
                $"Вы уверены, что хотите удалить запись:\n\"{description}\"?",
                "Подтверждение удаления",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DeleteEvent(selectedDayTableName, description, category);
                    LoadAllFolders();

                    // Сбрасываем выделение
                    selectedDataGrid.SelectedItem = null;
                    selectedRow = null;
                    selectedDataGrid = null;

                    MessageBox.Show("Запись успешно удалена", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении записи: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        // Метод для удаления события из базы данных
        private void DeleteEvent(string dayTableName, string description, string category)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();
                string query = $@"DELETE FROM {dayTableName} 
                              WHERE DNoteDescription = @Description 
                              AND DNoteCategory = @Category";

                using (SqlCommand command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Description", description);
                    command.Parameters.AddWithValue("@Category", category ?? (object)DBNull.Value);

                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Запись не найдена в базе данных");
                    }
                }
            }
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

        private void AddEventButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tag)
            {
                string[] parts = tag.Split('_');
                if (parts.Length == 2)
                {
                    string dayTableName = parts[0];
                    string dayRussianName = parts[1];

                    CalendarEventWindow addWindow = new CalendarEventWindow(dayTableName, dayRussianName);
                    addWindow.Owner = Window.GetWindow(this);

                    if (addWindow.ShowDialog() == true)
                    {
                        LoadAllFolders();
                    }
                }
            }
        }
    }
}