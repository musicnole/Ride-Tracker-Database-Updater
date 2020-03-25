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
using System.Data;
using Ride_Tracker_Database_Updater.Model;
using Ride_Tracker_Database_Updater.DAL;
using System.Collections;
using Chapter = Ride_Tracker_Database_Updater.Model.Chapter;
using System.Reflection;

namespace Ride_Tracker_Database_Updater
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {

        private IEnumerable chList;
        private const string uriGoogle = "https://www.google.com/";
        
        public MainWindow()
        {
            InitializeComponent();
            //Get List of Chapters from Db
            chList = Helper.GoogleHelper.GetChapters();
            dgChapters.ItemsSource = chList;
            TripBrowser.Navigate(uriGoogle);
            txtUrl.Text = uriGoogle;
        
        }

       
        private void UpdateChapterGoDaddyLink(object sender, RoutedEventArgs e)
        {
            // Create  a google Link
            //Return a dictionary of the Chapter ID and Google Link to update the chapter table
            Dictionary<int, string> chGoogleLink = new Dictionary<int, string>();
            chGoogleLink = Helper.GoogleHelper.CreateGoogleMapLink();
            UpdateChapterGoogleLinkDB(chGoogleLink);
            chList = Helper.GoogleHelper.GetChapters();
            dgChapters.ItemsSource = chList;

        }

        private static void UpdateChapterGoogleLinkDB(Dictionary<int, string> chGoogleLink)
        {
            using (var chTableCon = new ChapterClassDataContext())
            {
                foreach (var ch in chTableCon.Chapters)
                {
                    ch.GoogleLink = chGoogleLink[ch.ChapterId];
                    chTableCon.SubmitChanges();

                }
            }
        }
             
        private void dgChapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;
            DAL.Chapter ch = (DAL.Chapter)dg.SelectedItem;
            //MessageBox.Show(ch.ChapterName.ToString());

            SurpressJavascriptErrors();
            TripBrowser.Navigate(ch.GoogleLink.ToString());
            txtUrl.Text = ch.GoogleLink.ToString();
        }


        #region Browser Methods
        private void TripBrowser_Navigating(object sender, System.Windows.Navigation.NavigatingCancelEventArgs e)
        {
            txtUrl.Text = e.Uri.OriginalString;
        }
        private void SurpressJavascriptErrors()
        {
            dynamic activeX = this.TripBrowser.GetType().InvokeMember("ActiveXInstance",
                BindingFlags.GetProperty | BindingFlags.Instance | BindingFlags.NonPublic,
                null, this.TripBrowser, new object[] { });

            activeX.Silent = true;
        }

        private void TripBrowser_LoadCompleted(object sender, NavigationEventArgs e)
        {
            WebBrowser tb = (WebBrowser)sender;
            txtUrl.Text = tb.Source.ToString();
        }
        private void txtUrl_KeyUp(object sender, KeyEventArgs e)
        {
            string uriCheck = txtUrl.Text;
            if (e.Key == Key.Enter)
                if(uriCheck.Contains("http://")== false){
                    uriCheck = string.Concat("http://", uriCheck);
                }
                if (uriCheck.Contains(".com") == false)
                {
                    uriCheck = string.Concat(uriCheck,".com");
                }
            if (Uri.IsWellFormedUriString(uriCheck, UriKind.Absolute))
                {
                    SurpressJavascriptErrors();
                    TripBrowser.Navigate(uriCheck);
                }
                    
        }
        private void BrowseBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((TripBrowser != null) && (TripBrowser.CanGoBack));
        }

        private void BrowseBack_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TripBrowser.GoBack();
        }

        private void BrowseForward_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = ((TripBrowser != null) && (TripBrowser.CanGoForward));
        }

        private void BrowseForward_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TripBrowser.GoForward();
        }

        private void GoToPage_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void GoToPage_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            TripBrowser.Navigate("https://www.google.com/");
        }
        #endregion

        private void UpdateChapterAddress(object sender, RoutedEventArgs e)
        {
            UpdateChapterAddress ca = new UpdateChapterAddress();
            ca.ShowDialog(); 
            
        }

        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
