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
    /// Логика взаимодействия для PageFolders.xaml
    /// </summary>
    public partial class PageFolders : Page
    {
        public PageFolders()
        {
            InitializeComponent();
        }
        private void SubPageButton_Click(object sender, RoutedEventArgs e)
        {
            Window foldersInside;
            foldersInside = new WindowFoldersInside();
            foldersInside.Show();
            Window currentWindow = Window.GetWindow(this);
            currentWindow.Close();

        }
    }
    
}
