using System;
using System.Collections.Generic;
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
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }
        private void NavButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPage($"PageFolders.xaml");
            

        }
        
        private void SettingsButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Проект был выполнен: Дариной Сан  \n Группа: 3pk2  \n Eyaho eyaho eyaho eyaho\r\nEyaho eyaho eyaho eyaho \n Приходите в АСМР салон 'КОМА' \n  Подпись от арины кирш 3пк2", "ТКтктктко",
                                MessageBoxButton.OK, MessageBoxImage.Information);

        }
        private void InfoButton_Click(object sender, RoutedEventArgs e)
        { }
        private void LoadPage(string pageName)
        {
            try
            {
                MainFrame.Source = new System.Uri(pageName, System.UriKind.Relative);
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки страницы: {ex.Message}");
            }
        }

        private void SubPageButton_Click(object sender, RoutedEventArgs e)
        {
            string pageName = "PageFolder1.xaml"; 

            if (sender is Button button)
            {
                switch (button.Name)
                {
                    case "FilterButton":
                        pageName = "PageFilters.xaml";
                        break;
                    case "CalendarButton":
                        pageName = "PageCalendar.xaml";
                        break;
                    case "BtnFolder3":
                        pageName = "PageFolder3.xaml";
                        break;
                    case "FilesButton":
                        pageName = "PageFileStore.xaml";
                        break;
                       
                }


            }

            Window windowAdaptive = new WindowAdaptive(pageName);
            windowAdaptive.Show();

            Window currentWindow = Window.GetWindow(this);
            currentWindow.Close();
        }
    }
}
