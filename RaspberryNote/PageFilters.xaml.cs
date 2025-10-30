using System;
using System.Data;
using System.Windows;
using System.Windows.Controls;

namespace RaspberryNote
{
    public partial class PageFilters : Page
    {
        private const string connectionString = "data source=HONOR_RINAOUKO\\MSSQLSERVER01;initial catalog=RaspberryNote;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";

        public PageFilters()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT CategoryId FROM Categories ORDER BY CategoryId";

                    using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                    using (var adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                    {
                        var dataTable = new DataTable();
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

        private void LoadTasksByCategory(string categoryId)
        {
            try
            {
                var dataTable = new DataTable();

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    var tableNames = new[] { "TasksFolder1", "TasksFolder2", "TasksFolder3" };

                    foreach (var tableName in tableNames)
                    {
                        var query = $@"
                            SELECT 
                                NoteDescription,
                                NoteState,
                                NoteFinishDate,
                                NoteCategory,
                                '@TableName' as 'Папка'
                            FROM {tableName} 
                            WHERE NoteCategory = (SELECT CategoryId FROM Categories WHERE CategoryId = @CategoryId)
                            AND NoteDescription IS NOT NULL";

                        query = query.Replace("@TableName", tableName);

                        using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@CategoryId", categoryId);

                            using (var adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                            {
                                adapter.Fill(dataTable);
                            }
                        }
                    }
                }

                TasksDataGrid.ItemsSource = dataTable.DefaultView;

                string categoryName = GetCategoryName(categoryId);
                StatusTextBlock.Text = $"Найдено {dataTable.Rows.Count} задач в категории '{categoryName}'";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
                StatusTextBlock.Text = "Ошибка загрузки задач";
            }
        }

        private string GetCategoryName(string categoryId)
        {
            try
            {
                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    var query = "SELECT CategoryId FROM Categories WHERE CategoryId = @CategoryId";

                    using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@CategoryId", categoryId);
                        return command.ExecuteScalar()?.ToString() ?? "Неизвестная категория";
                    }
                }
            }
            catch
            {
                return "Неизвестная категория";
            }
        }

        private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoryComboBox.SelectedValue != null)
            {
                string selectedCategoryId = CategoryComboBox.SelectedValue.ToString();
                LoadTasksByCategory(selectedCategoryId);
            }
        }

        private void View1Button_Click(object sender, RoutedEventArgs e)
        {
            LoadFixedView("Срочные", "WHERE Priority >= 3");
        }

        private void View2Button_Click(object sender, RoutedEventArgs e)
        {
            LoadFixedView("Важные", "WHERE Priority >= 4");
        }

        private void View3Button_Click(object sender, RoutedEventArgs e)
        {
            LoadFixedView("Выполненные", "WHERE IsCompleted = 1");
        }

        private void View4Button_Click(object sender, RoutedEventArgs e)
        {
            LoadFixedView("Просроченные", "WHERE DueDate < GETDATE() AND IsCompleted = 0");
        }

        private void View5Button_Click(object sender, RoutedEventArgs e)
        {
            LoadFixedView("Новые", "WHERE CreatedDate >= DATEADD(day, -7, GETDATE())");
        }

        private void LoadFixedView(string viewName, string whereClause)
        {
            try
            {
                var dataTable = new DataTable();

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    var tableNames = new[] { "TasksFolder1", "TasksFolder2", "TasksFolder3" };

                    foreach (var tableName in tableNames)
                    {
                        var query = $@"
                            SELECT 
                                NoteDescription as 'Описание',
                                NoteState as 'Статус',
                                NoteFinishDate as 'Дэдлайн',
                                NoteCategory as 'Категория',
                                '@TableName' as 'Папка'
                            FROM {tableName} 
                            {whereClause}
                            AND NoteDescription IS NOT NULL";

                        query = query.Replace("@TableName", tableName);

                        using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                        using (var adapter = new System.Data.SqlClient.SqlDataAdapter(command))
                        {
                            adapter.Fill(dataTable);
                        }
                    }
                }

                TasksDataGrid.ItemsSource = dataTable.DefaultView;
                StatusTextBlock.Text = $"Фильтр: {viewName} - найдено {dataTable.Rows.Count} задач";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки представления: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}