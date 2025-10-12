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
    /// Логика взаимодействия для WindowAdaptive.xaml
    /// </summary>
    public partial class WindowAdaptive : Window
    {
        public WindowAdaptive()
        {
            InitializeComponent();
        }
        public WindowAdaptive(string pageName)
        {
            InitializeComponent();
            LoadPage(pageName);
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
