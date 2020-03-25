using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

namespace Ride_Tracker_Database_Updater
{
    
    public partial class TestGoogleURL : Window
    {
        public TestGoogleURL(string gURL)
        {
            InitializeComponent();
            txtUrl.Text = gURL;
            SurpressJavascriptErrors();
            TripBrowser.Navigate(gURL);
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
                if (uriCheck.Contains("http://") == false)
                {
                    uriCheck = string.Concat("http://", uriCheck);
                }
            if (uriCheck.Contains(".com") == false)
            {
                uriCheck = string.Concat(uriCheck, ".com");
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
    }
}
