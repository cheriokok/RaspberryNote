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
    /// Логика взаимодействия для PageFolder1.xaml
    /// </summary>
    public partial class PageFolder3 : Page
    {
        private const string connectionString = "data source=HONOR_RINAOUKO\\MSSQLSERVER01;initial catalog=RaspberryNote;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";
        private Frame mainFrame;
        public PageFolder3()
        {
            InitializeComponent();
            LoadCategories();
            LoadFolder();
        }


        private void LoadCategories()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT CategoryId FROM Categories ORDER BY CategoryId";

                    using (SqlCommand command = new SqlCommand(query, connection))
                    using (SqlDataAdapter adapter = new SqlDataAdapter(command))
                    {
                        DataTable dataTable = new DataTable();
                        adapter.Fill(dataTable);
                        CategoryComboBox.DisplayMemberPath = "CategoryId";
                        CategoryComboBox.SelectedValuePath = "CategoryId";
                        CategoryComboBox.ItemsSource = dataTable.DefaultView;

                        if (dataTable.Rows.Count > 0)
                        {
                            CategoryComboBox.SelectedIndex = 0;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedValue != null)
            {
                string selectedCategoryId = CategoryComboBox.SelectedValue.ToString();

            }
        }

        private void AddButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(txtDescription.Text))
                {
                    MessageBox.Show("Введите описание задачи", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string insertQuery = @"INSERT INTO TasksFolder3 
                                         (NoteDescription, NoteState, NoteFinishDate, NoteCategory) 
                                         VALUES (@Description, @State, @FinishDate, @Category)";

                    using (SqlCommand command = new SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@Description", txtDescription.Text);
                        command.Parameters.AddWithValue("@State", (cmbState.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Новая");
                        command.Parameters.AddWithValue("@FinishDate",
    DateTime.TryParse(txtFinishDate.Text, out DateTime date) ? (object)date : DBNull.Value);
                        command.Parameters.AddWithValue("@Category", CategoryComboBox.SelectedValue?.ToString() ?? "Без темы");

                        command.ExecuteNonQuery();
                    }
                }

                ClearForm();
                LoadFolder();
                MessageBox.Show("Задача успешно добавлена!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }


        private void EditButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите задачу для редактирования", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;
                int noteId = Convert.ToInt32(selectedRow["NoteID"]);

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string updateQuery = @"UPDATE TasksFolder3 
                                         SET NoteDescription = @Description,
                                             NoteState = @State,
                                             NoteFinishDate = @FinishDate,
                                             NoteCategory = @Category
                                         WHERE NoteID = @NoteID";

                    using (SqlCommand command = new SqlCommand(updateQuery, connection))
                    {
                        command.Parameters.AddWithValue("@NoteID", noteId);
                        command.Parameters.AddWithValue("@Description", txtDescription.Text);
                        command.Parameters.AddWithValue("@State", (cmbState.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? "Новая");
                        command.Parameters.AddWithValue("@FinishDate",
     DateTime.TryParse(txtFinishDate.Text, out DateTime date) ? (object)date : DBNull.Value);
                        command.Parameters.AddWithValue("@Category", CategoryComboBox.SelectedValue?.ToString() ?? "1");

                        command.ExecuteNonQuery();
                    }
                }

                ClearForm();
                LoadFolder();
                MessageBox.Show("Задача успешно обновлена!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при редактировании: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            if (dataGrid.SelectedItem == null)
            {
                MessageBox.Show("Выберите задачу для удаления", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            MessageBoxResult result = MessageBox.Show("Вы уверены, что хотите удалить выбранную задачу?",
                                                    "Подтверждение удаления",
                                                    MessageBoxButton.YesNo,
                                                    MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;
                    int noteId = Convert.ToInt32(selectedRow["NoteID"]);

                    using (SqlConnection connection = new SqlConnection(connectionString))
                    {
                        connection.Open();

                        string deleteQuery = "DELETE FROM TasksFolder3 WHERE NoteID = @NoteID";

                        using (SqlCommand command = new SqlCommand(deleteQuery, connection))
                        {
                            command.Parameters.AddWithValue("@NoteID", noteId);
                            command.ExecuteNonQuery();
                        }
                    }

                    ClearForm();
                    LoadFolder();
                    MessageBox.Show("Задача успешно удалена!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при удалении: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGrid.SelectedItem != null)
            {
                DataRowView selectedRow = (DataRowView)dataGrid.SelectedItem;
                txtDescription.Text = selectedRow["NoteDescription"].ToString();
                cmbState.Text = selectedRow["NoteState"].ToString();

                if (selectedRow["NoteFinishDate"] != DBNull.Value)
                {
                    txtFinishDate.Text = Convert.ToDateTime(selectedRow["NoteFinishDate"]).ToString("dd.MM.yyyy");
                }
                else
                {
                    txtFinishDate.Text = string.Empty;
                }

                CategoryComboBox.SelectedValue = selectedRow["NoteCategory"].ToString();
            }
        }
        private void ClearForm()
        {
            txtDescription.Text = string.Empty;
            cmbState.SelectedIndex = -1;
            txtFinishDate.Text = string.Empty;
            CategoryComboBox.SelectedIndex = -1;
            dataGrid.SelectedItem = null;
        }
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {


            Window mainWindow = new MainWindow();
            mainWindow.Show();
            Window currentWindow = Window.GetWindow(this);
            currentWindow?.Close();

        }
        private void LoadFolder()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = @"SELECT 
                    NoteID,
                    NoteDescription,
                    NoteState,
                    NoteFinishDate, 
                    NoteCategory
                    FROM TasksFolder3";

                    SqlDataAdapter adapter = new SqlDataAdapter(query, connection);
                    DataTable dataTable = new DataTable();
                    adapter.Fill(dataTable);

                    dataGrid.ItemsSource = dataTable.DefaultView;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
