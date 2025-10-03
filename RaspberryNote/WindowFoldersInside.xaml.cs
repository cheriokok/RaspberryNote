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
using System.Windows.Shapes;

namespace RaspberryNote
{
    /// <summary>
    /// Логика взаимодействия для WindowFoldersInside.xaml
    /// </summary>
    public partial class WindowFoldersInside : Window
    {
        public WindowFoldersInside()
        {
            InitializeComponent();
            LoadPage($"PageFolder1.xaml");
        }
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
    }
}
