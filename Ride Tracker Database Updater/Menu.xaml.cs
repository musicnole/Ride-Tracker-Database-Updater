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

namespace Ride_Tracker_Database_Updater
{
    /// <summary>
    /// Interaction logic for Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        public Menu()
        {
            InitializeComponent();
        }

        private void AddChapter_Click(object sender, RoutedEventArgs e)
        {
            ChapterCU cu = new ChapterCU("Create");
            cu.ShowDialog();
        }

        private void UpdateChapter_Click(object sender, RoutedEventArgs e)
        {
            ChapterCU cu = new ChapterCU("Update");
            cu.ShowDialog();
        }

        private void ViewChapter_Click(object sender, RoutedEventArgs e)
        {
            MainWindow mw = new MainWindow();
            mw.ShowDialog();
        }

        private void ViewMiles_Click(object sender, RoutedEventArgs e)
        {
            UpdateMiles um = new UpdateMiles();
            um.ShowDialog();
        }
    }
}
