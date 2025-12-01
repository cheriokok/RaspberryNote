using System;
using System.Windows;
using System.Data.SqlClient;
using System.Windows.Controls;
using System.Data;

namespace RaspberryNote
{
    public partial class CalendarEventWindow : Window
    {
        private const string connectionString = "data source=HONOR_RINAOUKO\\MSSQLSERVER01;initial catalog=RaspberryNote;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";

        public string DayTableName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }

        public CalendarEventWindow(string dayTableName, string dayRussianName)
        {
            InitializeComponent();
            DayTableName = dayTableName;
            TitleText.Text = $"Добавить - {dayRussianName}";
            LoadCategories();
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

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(DescriptionTextBox.Text))
            {
                MessageBox.Show("Введите описание события", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (CategoryComboBox.SelectedValue == null)
            {
                MessageBox.Show("Выберите категорию", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Description = DescriptionTextBox.Text.Trim();
            Category = CategoryComboBox.SelectedValue?.ToString() ?? "1";

            try
            {
                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();
                    string query = $@"INSERT INTO {DayTableName} (DNoteDescription, DNoteCategory) 
                                    VALUES (@Description, @Category)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Description", Description);
                    command.Parameters.AddWithValue("@Category", Category);

                    int result = command.ExecuteNonQuery();

                    if (result > 0)
                    {
                        MessageBox.Show("Событие успешно добавлено!", "Успех",
                                      MessageBoxButton.OK, MessageBoxImage.Information);
                        this.DialogResult = true;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при сохранении: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }
    }
}