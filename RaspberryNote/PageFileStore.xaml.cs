using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace RaspberryNote
{
    public partial class PageFileStore : Page, INotifyPropertyChanged
    {
        private const string connectionString = "data source=HONOR_RINAOUKO\\MSSQLSERVER01;initial catalog=RaspberryNote;integrated security=True;trustservercertificate=True;MultipleActiveResultSets=True;App=EntityFramework";
        private const long MAX_FILE_SIZE_BYTES = 10 * 1024 * 1024; 

        private ObservableCollection<StoredFile> _files;
        public ObservableCollection<StoredFile> Files
        {
            get => _files;
            set { _files = value; OnPropertyChanged(); }
        }

        private StoredFile _selectedFile;
        public StoredFile SelectedFile
        {
            get => _selectedFile;
            set { _selectedFile = value; OnPropertyChanged(); }
        }

        private string storagePath;

        public PageFileStore()
        {
            InitializeComponent();
            DataContext = this;
            InitializeStorage();
            LoadFiles();
            LoadTasks();
        }

        public class TaskItem
        {
            public string Description { get; set; }
            public string SourceTable { get; set; }

            public override string ToString()
            {
                return $"{Description} ({SourceTable})";
            }
        }

        private void InitializeStorage()
        {
            storagePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "FileStorage");
            if (!Directory.Exists(storagePath))
            {
                Directory.CreateDirectory(storagePath);
            }
        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);

            if (FilesListBox != null)
            {
                FilesListBox.SelectionChanged += (s, args) =>
                {
                    Console.WriteLine($"Selected: {SelectedFile?.FileName}");
                };
            }
        }

        #region ActionsForClicks

        private Dictionary<string, (string TaskDescription, string SourceTable)> LoadFileLinksFromDatabase()
        {
            var links = new Dictionary<string, (string, string)>();

            try
            {
                var query = @"
                    SELECT FileName, TaskDescription, SourceTable 
                    FROM FileTaskLinks 
                    ORDER BY CreatedDate DESC";

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            var fileName = reader["FileName"].ToString();
                            var taskDescription = reader["TaskDescription"].ToString();
                            var sourceTable = reader["SourceTable"].ToString();

                            if (!links.ContainsKey(fileName))
                            {
                                links[fileName] = (taskDescription, sourceTable);
                            }
                        }
                    }
                }

                Console.WriteLine($"Загружено {links.Count} связей из БД");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка загрузки связей: {ex.Message}");
            }

            return links;
        }

        private void LoadFiles()
        {
            Files = new ObservableCollection<StoredFile>();

            if (!Directory.Exists(storagePath)) return;

            var fileLinks = LoadFileLinksFromDatabase();

            var fileEntries = Directory.GetFiles(storagePath);
            foreach (var filePath in fileEntries)
            {
                var fileInfo = new FileInfo(filePath);
                var fileName = Path.GetFileName(filePath);

                string linkedTaskText = "Не связана";
                if (fileLinks.TryGetValue(fileName, out var link))
                {
                    linkedTaskText = $"{link.TaskDescription} ({link.SourceTable})";
                }

                var file = new StoredFile(OpenFile, DeleteFile)
                {
                    FileName = fileName,
                    OriginalFileName = fileName,
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    CreatedDate = fileInfo.CreationTime,
                    LinkedTask = linkedTaskText
                };

                Files.Add(file);
            }
        }

        private void OpenFile(StoredFile file)
        {
            try
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = file.FilePath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при открытии файла: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void DeleteFileTaskLink(string fileName)
        {
            try
            {
                var query = "DELETE FROM FileTaskLinks WHERE FileName = @FileName";

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Связь удалена для файла: {fileName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка удаления связи: {ex.Message}");
            }
        }

        private void DeleteFile(StoredFile file)
        {
            var result = MessageBox.Show($"Удалить файл '{file.FileName}'?",
                                       "Подтверждение удаления",
                                       MessageBoxButton.YesNo,
                                       MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    File.Delete(file.FilePath);

                    DeleteFileTaskLink(file.FileName);

                    Files.Remove(file);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка: {ex.Message}", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void LoadTasks()
        {
            try
            {
                var tasks = new List<TaskItem>();

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    var tableNames = new[] { "TasksFolder1", "TasksFolder2", "TasksFolder3" };

                    foreach (var tableName in tableNames)
                    {
                        var query = $"SELECT NoteDescription FROM {tableName} WHERE NoteDescription IS NOT NULL";

                        using (var command = new System.Data.SqlClient.SqlCommand(query, connection))
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                var description = reader["NoteDescription"]?.ToString();
                                if (!string.IsNullOrEmpty(description))
                                {
                                    tasks.Add(new TaskItem
                                    {
                                        Description = description,
                                        SourceTable = tableName
                                    });
                                }
                            }
                        }
                    }
                }

                tasks = tasks.OrderBy(t => t.Description).ToList();

                if (TasksComboBox != null)
                {
                    TasksComboBox.ItemsSource = tasks;
                    TasksComboBox.DisplayMemberPath = "Description";
                }

                Console.WriteLine($"Загружено {tasks.Count} задач из БД");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки задач из БД: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);

                var testTasks = new List<TaskItem>
                {
                    new TaskItem { Description = "Тестовая задача 1", SourceTable = "TaskFolder1" },
                    new TaskItem { Description = "Тестовая задача 2", SourceTable = "TaskFolder2" }
                };
                TasksComboBox.ItemsSource = testTasks;
                TasksComboBox.DisplayMemberPath = "Description";
            }
        }

        #endregion

        #region ClicksWithFiles

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null)
            {
                OpenFile(SelectedFile);
            }
            else
            {
                MessageBox.Show("Выберите файл для открытия", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void DeleteFileButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null)
            {
                DeleteFile(SelectedFile);
            }
            else
            {
                MessageBox.Show("Выберите файл для удаления", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void LinkTaskButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedFile != null && TasksComboBox?.SelectedItem is TaskItem selectedTask)
            {
                SelectedFile.LinkedTask = $"{selectedTask.Description} ({selectedTask.SourceTable})";

                SaveFileTaskLink(SelectedFile.FileName, selectedTask.Description, selectedTask.SourceTable);

                MessageBox.Show($"Файл '{SelectedFile.FileName}' связан с задачей: {selectedTask.Description}", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Выберите файл и задачу для связывания", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "Б", "КБ", "МБ", "ГБ", "ТБ" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len /= 1024;
            }
            return $"{len:0.##} {sizes[order]}";
        }

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;

            openFileDialog.Filter = "Все файлы (*.*)|*.*|" +
                                   "Документы (*.doc;*.docx;*.pdf;*.txt;*.rtf)|*.doc;*.docx;*.pdf;*.txt;*.rtf|" +
                                   "Изображения (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                                   "Таблицы (*.xls;*.xlsx;*.csv)|*.xls;*.xlsx;*.csv";

            if (openFileDialog.ShowDialog() == true)
            {
                var existingLinks = LoadFileLinksFromDatabase();
                int successfulCount = 0;
                int skippedCount = 0;

                foreach (string sourceFilePath in openFileDialog.FileNames)
                {
                    try
                    {
                        string fileName = Path.GetFileName(sourceFilePath);
                        string destFilePath = Path.Combine(storagePath, fileName);

                        var fileInfo = new FileInfo(sourceFilePath);
                        if (fileInfo.Length > MAX_FILE_SIZE_BYTES)
                        {
                            skippedCount++;
                            string maxSizeFormatted = FormatFileSize(MAX_FILE_SIZE_BYTES);
                            string fileSizeFormatted = FormatFileSize(fileInfo.Length);

                            MessageBox.Show($"Файл '{fileName}' не был загружен.\n" +
                                          $"Размер файла: {fileSizeFormatted}\n" +
                                          $"Максимальный размер: {maxSizeFormatted}\n\n" +
                                          $"Пожалуйста, выберите файл размером не более {maxSizeFormatted}.",
                                          "Файл слишком большой",
                                          MessageBoxButton.OK,
                                          MessageBoxImage.Warning);
                            continue;
                        }

                        if (File.Exists(destFilePath))
                        {
                            var overwriteResult = MessageBox.Show($"Файл '{fileName}' уже существует.\n" +
                                                                "Заменить его?",
                                                                "Подтверждение замены",
                                                                MessageBoxButton.YesNo,
                                                                MessageBoxImage.Question);

                            if (overwriteResult == MessageBoxResult.No)
                            {
                                skippedCount++;
                                continue;
                            }
                        }

                        File.Copy(sourceFilePath, destFilePath, true);

                        var destFileInfo = new FileInfo(destFilePath);

                        string linkedTaskText = "Не связана";
                        if (existingLinks.TryGetValue(fileName, out var link))
                        {
                            linkedTaskText = $"{link.TaskDescription} ({link.SourceTable})";
                        }

                        var newFile = new StoredFile(OpenFile, DeleteFile)
                        {
                            FileName = fileName,
                            OriginalFileName = fileName,
                            FilePath = destFilePath,
                            FileSize = destFileInfo.Length,
                            CreatedDate = destFileInfo.CreationTime,
                            LinkedTask = linkedTaskText
                        };

                        Files.Add(newFile);
                        successfulCount++;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка с файлом {Path.GetFileName(sourceFilePath)}: {ex.Message}",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                string message;
                if (successfulCount > 0 && skippedCount > 0)
                {
                    message = $"Загружено файлов: {successfulCount}\n" +
                             $"Пропущено: {skippedCount}";
                }
                else if (successfulCount > 0)
                {
                    message = $"Успешно загружено {successfulCount} файлов!";
                }
                else
                {
                    message = "Файлы не были загружены.";
                }

                MessageBox.Show(message, "Результат загрузки",
                              MessageBoxButton.OK,
                              successfulCount > 0 ? MessageBoxImage.Information : MessageBoxImage.Warning);
            }
        }
        #endregion

        private void SaveFileTaskLink(string fileName, string taskDescription, string sourceTable)
        {
            try
            {
                var createTableQuery = @"
                    IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='FileTaskLinks' AND xtype='U')
                    CREATE TABLE FileTaskLinks (
                        Id int IDENTITY(1,1) PRIMARY KEY,
                        FileName nvarchar(500) NOT NULL,
                        TaskDescription nvarchar(1000) NOT NULL,
                        SourceTable nvarchar(100) NOT NULL,
                        CreatedDate datetime DEFAULT GETDATE()
                    )";

                var deleteQuery = @"
                    DELETE FROM FileTaskLinks WHERE FileName = @FileName";

                var insertQuery = @"
                    INSERT INTO FileTaskLinks (FileName, TaskDescription, SourceTable) 
                    VALUES (@FileName, @TaskDescription, @SourceTable)";

                using (var connection = new System.Data.SqlClient.SqlConnection(connectionString))
                {
                    connection.Open();

                    using (var command = new System.Data.SqlClient.SqlCommand(createTableQuery, connection))
                    {
                        command.ExecuteNonQuery();
                    }

                    using (var command = new System.Data.SqlClient.SqlCommand(deleteQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.ExecuteNonQuery();
                    }

                    using (var command = new System.Data.SqlClient.SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.Parameters.AddWithValue("@TaskDescription", taskDescription);
                        command.Parameters.AddWithValue("@SourceTable", sourceTable);
                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Связь сохранена: {fileName} -> {taskDescription}");

                if (SelectedFile != null && SelectedFile.FileName == fileName)
                {
                    SelectedFile.LinkedTask = $"{taskDescription} ({sourceTable})";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения связи: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            Window mainWindow = new MainWindow();
            mainWindow.Show();
            Window currentWindow = Window.GetWindow(this);
            currentWindow?.Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}