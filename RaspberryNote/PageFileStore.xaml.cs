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
        private void LoadFiles()
        {
            Files = new ObservableCollection<StoredFile>();

            if (!Directory.Exists(storagePath)) return;

            var fileEntries = Directory.GetFiles(storagePath);
            foreach (var filePath in fileEntries)
            {
                var fileInfo = new FileInfo(filePath);
                var file = new StoredFile(OpenFile, DeleteFile)
                {
                    FileName = Path.GetFileName(filePath),
                    OriginalFileName = Path.GetFileName(filePath),
                    FilePath = filePath,
                    FileSize = fileInfo.Length,
                    CreatedDate = fileInfo.CreationTime,
                    LinkedTask = "Не связана"
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

        private void AddFileButton_Click(object sender, RoutedEventArgs e)
        {
            var openFileDialog = new Microsoft.Win32.OpenFileDialog();
            openFileDialog.Multiselect = true;

            if (openFileDialog.ShowDialog() == true)
            {
                foreach (string sourceFilePath in openFileDialog.FileNames)
                {
                    try
                    {
                        string fileName = Path.GetFileName(sourceFilePath);
                        string destFilePath = Path.Combine(storagePath, fileName);

                        File.Copy(sourceFilePath, destFilePath, true);

                        var fileInfo = new FileInfo(destFilePath);
                        var newFile = new StoredFile(OpenFile, DeleteFile)
                        {
                            FileName = fileName,
                            OriginalFileName = fileName,
                            FilePath = destFilePath,
                            FileSize = fileInfo.Length,
                            CreatedDate = fileInfo.CreationTime,
                            LinkedTask = "Не связана"
                        };

                        Files.Add(newFile);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка с файлом {Path.GetFileName(sourceFilePath)}: {ex.Message}",
                                      "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }

                MessageBox.Show("Файлы добавлены!", "Успех",
                              MessageBoxButton.OK, MessageBoxImage.Information);
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

                    using (var command = new System.Data.SqlClient.SqlCommand(insertQuery, connection))
                    {
                        command.Parameters.AddWithValue("@FileName", fileName);
                        command.Parameters.AddWithValue("@TaskDescription", taskDescription);
                        command.Parameters.AddWithValue("@SourceTable", sourceTable);
                        command.ExecuteNonQuery();
                    }
                }

                Console.WriteLine($"Связь сохранена: {fileName} -> {taskDescription}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка сохранения связи: {ex.Message}");
      
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