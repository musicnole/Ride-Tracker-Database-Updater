using System;
using System.Collections;
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
using Ride_Tracker_Database_Updater.DAL;
using Ride_Tracker_Database_Updater.Model;

namespace Ride_Tracker_Database_Updater
{
    /// <summary>
    /// Interaction logic for UpdateMiles.xaml
    /// </summary>
    public partial class UpdateMiles : Window
    {
        private int MilesTableCount = 0;
        public UpdateMiles()
        {
            InitializeComponent();
            MilesTableCount= CheckMilesTableCount();
            if (MilesTableCount > 0)
            {
                GetMilesTable();
            }
            else
            {
                btnCreateMiles.Visibility = Visibility.Visible;
                btnUpdateMiles.Visibility = Visibility.Collapsed;
            }
        }

        private int CheckMilesTableCount()
        {
            int mCount = 0;
            MilesDataContext mdc = new MilesDataContext();
            mCount = mdc.Miles.Count();
            return mCount;
        }
        
        /// <summary>
        /// Loads the Grid with the miles table info
        /// </summary>
        private void GetMilesTable()
        {
            MilesDataContext mdc = new MilesDataContext();
            List<ChapterMiles> lcm = new List<ChapterMiles>();
            Dictionary<int, string> chList = Helper.Helper.GetChapters();
            var lcmSQL = from m in mdc.Miles
                         select new ChapterMiles() {MilesId = m.MilesId,FromID = m.FromId, ToId = m.ToId,GoogleUri= m.GoogleUri, Miles=m.Miles } ;
            lcm = lcmSQL.ToList();
            foreach(ChapterMiles cm in lcm)
            {
                cm.ToChapter = chList.Single(x => x.Key == cm.ToId).Value;
                cm.FromChapter = chList.Single(x => x.Key == cm.FromID).Value;
            }
            dgChapterMiles.ItemsSource = lcm;
        }
                
        private void dgChapters_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataGrid dg = sender as DataGrid;

            Model.ChapterMiles mile = (Model.ChapterMiles)dg.SelectedItem;
            //MessageBox.Show(ch.ChapterName.ToString());

            SurpressJavascriptErrors();
            TripBrowser.Navigate(mile.GoogleUri.ToString());
            txtUrl.Text = mile.GoogleUri.ToString();
        }


     
        /// <summary>
        /// This Method will add the chapters that do not exist in the Miles table to the miles table
        /// </summary>
        /// <param name="chAddList"></param>
        private void AddChapterToMilesTable(List<int> chAddList, Dictionary<int, string> chList)
        {
            using(MilesDataContext mdc =  new MilesDataContext())
            {
                // Loop through the Add list and then the list of current chapters to create the Mile
                foreach (int chId in chAddList)
                {
                    foreach (var ch in chList)
                    {
                        if (chId != ch.Key)
                        {
                            Mile mAdd = new Mile();
                            mAdd.FromId = chId;
                            mAdd.ToId = ch.Key;
                            mAdd.Active = true;
                            mAdd.DateCreated = System.DateTime.Now;
                            mAdd.DateModified = System.DateTime.Now;
                            mdc.Miles.InsertOnSubmit(mAdd);
                        }

                    }
                }
                
                try
                {
                    mdc.SubmitChanges();
                }
                catch (Exception exceptAdd)
                {
                    MessageBox.Show(exceptAdd.Message.ToString());
                }
            }

        }
        
        /// <summary>
        /// This Method Looks at the Miles Table, Gets a disticnt list of Chapters in the From Id column
        /// checks the list against the chapters in the chapters table
        /// Returns the list of chapters that need to be added to the Miles table
        /// </summary>
        private List<int> GetChapterToAddList()
        {
            List<int> chAddList = new List<int>();
            List<int> distinctList = new List<int>();
            bool exists = false;
            MilesDataContext mdc = new MilesDataContext();
            
            //Get the distinct list of Chapters in Miles table
            var idList = mdc.Miles.Select(m => m.FromId).Distinct();
            distinctList = idList.ToList();

            //Get the List of Chapters from The Chapters Table
            Dictionary<int, string> chList = Helper.Helper.GetChapters();
            
            //Compare the 2 lists and add a chapter that needs to be added to the Add List
            foreach(var ch in chList)
            {
                exists = distinctList.Any(id => id == ch.Key);
                if (exists == false)
                {
                    chAddList.Add(ch.Key);
                }
            }

            return chAddList;
        }
        
        /// <summary>
        /// Method that updates the miles in the miles table
        /// </summary>
        /// <param name="originKey"></param>
        /// <param name="lookupOrigin"></param>
        /// <param name="lookupDestinationDict"></param>
        private void UpdateMileage(int originKey, string lookupOrigin, Dictionary<int, string> lookupDestinationDict)
        {
            int destKey = 0;
            bool updateAll = UpdateAll.IsChecked.Value;
            using (var mdc = new MilesDataContext())
            {
                foreach (var dest in lookupDestinationDict)
                {
                    destKey = dest.Key;
                    if (updateAll)
                    {
                        //Get the Mile Record
                        var mRec = from mtable in mdc.Miles
                                   where mtable.FromId == originKey && mtable.ToId == destKey
                                   select mtable;
                        foreach (Mile mile in mRec)
                        {
                            mile.Miles = (Int32)Helper.GoogleHelper.GetMileage(lookupOrigin, dest.Value);
                        }
                    }
                    else
                    {
                        //Get the Mile Record
                        var mRec = from mtable in mdc.Miles
                                   where mtable.FromId == originKey && mtable.ToId == destKey && mtable.Miles== 0
                                   select mtable;
                        foreach (Mile mile in mRec)
                        {
                            mile.Miles = (Int32)Helper.GoogleHelper.GetMileage(lookupOrigin, dest.Value);
                        }
                    }
                    
                    //Submit changes to table
                    try
                    {
                        mdc.SubmitChanges();
                    }
                    catch (Exception exceptUpdate)
                    {

                        MessageBox.Show(exceptUpdate.Message.ToString());
                    }

                }
            }
        }

        
        private void UpdateGoogleMilesUri(int originKey, Dictionary<int, string> lookupDestinationDict)
        {
            int destKey = 0;
            bool updateAll = UpdateAll.IsChecked.Value;
            using (var mdc = new MilesDataContext())
            {
                foreach (var dest in lookupDestinationDict)
                {
                    destKey = dest.Key;
                   
                        //Get the Mile Record
                        var mRec = from mtable in mdc.Miles
                                   where mtable.FromId == originKey && mtable.ToId == destKey
                                   select mtable;
                        foreach (Mile mile in mRec)
                        {
                            mile.GoogleUri = Helper.GoogleHelper.CreateGoogleMilesTableLink(originKey, mile.ToId);
                        }

                    //Submit changes to table
                    try
                    {
                        mdc.SubmitChanges();
                    }
                    catch (Exception exceptUpdate)
                    {

                        MessageBox.Show(exceptUpdate.Message.ToString());
                    }

                }
            }
        }

        /// <summary>
        /// If do not want to update the whole table then only update where miles =  0
        /// so this method checks to see if there are any miles = 0
        /// </summary>
        /// <returns></returns>
        private bool CheckZeroMiles()
        {
            bool zeroMiles = false;
            using (var mdc = new MilesDataContext())
            {
                var mRec = from mtable in mdc.Miles
                           where mtable.Miles == 0
                           select mtable;
                if (mRec.Count() > 0) zeroMiles = true;
            }
                return zeroMiles;
        }

        /// <summary>
        /// Used by the update button to get all the destination addresses
        /// </summary>
        /// <param name="chIdOrigin"></param>
        /// <param name="caList"></param>
        /// <returns></returns>
        private Dictionary<int,string> CreateDestinationAddressList(int chIdOrigin, List<DAL.ChapterAddress> caList)
        {
            Dictionary<int, string> lookupDestinationList = new Dictionary<int, string>();
            StringBuilder sblookupDestination = new StringBuilder();
            string lookupDestination = string.Empty;

            foreach (DAL.ChapterAddress ca in caList)
            {
                lookupDestination = string.Empty;
                //Make sure not to put the origin chapter into this address list
                if (ca.ChapterId != chIdOrigin)
                {
                    sblookupDestination.Append(ca.StreetAddress1);
                    sblookupDestination.Append(" ");
                    sblookupDestination.Append(ca.City);
                    sblookupDestination.Append(" ");
                    sblookupDestination.Append(Helper.Helper.GetStateCode((Int32)ca.StateId));
                    sblookupDestination.Append(" ");
                    sblookupDestination.Append(ca.Zip);
                    lookupDestination = sblookupDestination.ToString();
                }
                else continue;
                lookupDestinationList.Add(ca.ChapterId,lookupDestination);
                sblookupDestination.Clear();
            }
            return lookupDestinationList;
        }

        private static List<ChapterAddressMiles> CreateClubhouseAddressList(Dictionary<int, string> chList, List<DAL.ChapterAddress> caList)
        {
            //Create a list of Clubhouses and address 
            List<ChapterAddressMiles> camList = new List<ChapterAddressMiles>();
            //Add Clubhouse and the addresses
            foreach (var ch in chList)
            {
                ChapterAddressMiles cam = new ChapterAddressMiles();
                cam.ChapterID = ch.Key;
                cam.ChapterName = ch.Value;
                cam.ClubhouseAddressID = (from addr in caList where ch.Key == addr.ChapterId select addr.ChapterAddressId).FirstOrDefault();
                cam.StreetAddress = (from addr in caList where ch.Key == addr.ChapterId select addr.StreetAddress1).FirstOrDefault();
                cam.City = (from addr in caList where ch.Key == addr.ChapterId select addr.City).FirstOrDefault();
                cam.stateId = (Int32)(from addr in caList where ch.Key == addr.ChapterId select addr.StateId).FirstOrDefault();
                cam.Zip = (from addr in caList where ch.Key == addr.ChapterId select addr.Zip).FirstOrDefault();
                camList.Add(cam);
            }
            return camList;
        }


        private void UpdateMilesTable()
        {
            //Get all The Chapters into a List
            Dictionary<int, string> chList = new Dictionary<int, string>();
            List<int> chAddList = new List<int>();
            chList = Helper.Helper.GetChapters();

            //Get All the Chapter Addresses into a List
            List<DAL.ChapterAddress> caList = new List<DAL.ChapterAddress>();
            caList = Helper.Helper.GetChapterAddressesAll();

            // Create the List of clubhouse addresses
            List<ChapterAddressMiles> camList = new List<ChapterAddressMiles>();
            camList = CreateClubhouseAddressList(chList, caList);

            //Get a list of chapters that are not in the Miles Table, will need to insert instead of update
            chAddList = GetChapterToAddList();
            if (chAddList.Count > 0)
            {
                AddChapterToMilesTable(chAddList, chList);
            }

            StringBuilder sblookupOrigin = new StringBuilder();
            string lookupOrigin = string.Empty;
            Dictionary<int, string> lookupDestinationDict = new Dictionary<int, string>();
            string test = string.Empty;
            //Get Mileage, Loop through list of Chapters to set Origin Address
            foreach (ChapterAddressMiles cam in camList)
            {
                //Create Origin
                sblookupOrigin.Append(cam.StreetAddress);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.City);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(Helper.Helper.GetStateCode(cam.stateId));
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.Zip);
                lookupOrigin = sblookupOrigin.ToString();

                //Just to make life easy create a list of destination addresses
                lookupDestinationDict = CreateDestinationAddressList(cam.ChapterID, caList);

                //Make the call to google to get the miles between addresses The original address is being sent and a list of all addresses 
                // The update Mileage method will update all miles for 1 origin address
                UpdateMileage(cam.ChapterID, lookupOrigin, lookupDestinationDict);

                ////Update the Miles table Google uri
                //UpdateGoogleMilesUri(cam.ChapterID, lookupDestinationDict);

                sblookupOrigin.Clear();
                lookupOrigin = string.Empty;
            }

            MessageBox.Show("Miles Update Complete");
            GetMilesTable();
        }

        private void UpdateGoogleUrl()
        {
            //Get all The Chapters into a List
            Dictionary<int, string> chList = new Dictionary<int, string>();
            List<int> chAddList = new List<int>();
            chList = Helper.Helper.GetChapters();

            //Get All the Chapter Addresses into a List
            List<DAL.ChapterAddress> caList = new List<DAL.ChapterAddress>();
            caList = Helper.Helper.GetChapterAddressesAll();

            // Create the List of clubhouse addresses
            List<ChapterAddressMiles> camList = new List<ChapterAddressMiles>();
            camList = CreateClubhouseAddressList(chList, caList);

            StringBuilder sblookupOrigin = new StringBuilder();
            string lookupOrigin = string.Empty;
            Dictionary<int, string> lookupDestinationDict = new Dictionary<int, string>();
            string test = string.Empty;
            //Loop through list of Chapters to set Origin Address
            foreach (ChapterAddressMiles cam in camList)
            {
                //Create Origin
                sblookupOrigin.Append(cam.StreetAddress);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.City);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(Helper.Helper.GetStateCode(cam.stateId));
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.Zip);
                lookupOrigin = sblookupOrigin.ToString();

                //Just to make life easy create a list of destination addresses
                lookupDestinationDict = CreateDestinationAddressList(cam.ChapterID, caList);

                //Update the Miles table Google uri
                UpdateGoogleMilesUri(cam.ChapterID, lookupDestinationDict);

                sblookupOrigin.Clear();
                lookupOrigin = string.Empty;
            }

            MessageBox.Show("Miles Google Url Complete");
            GetMilesTable();
        }

        #region Button Clicks
        /// <summary>
        /// This method does a number of things
        /// checks to make sure that all the chapters are in the Miles table
        /// If a chapter is not in the Miles table then add the chapter before the update 
        /// The update will update the entire table in case an address has changed
        /// Updates the Miles in the Miles table and the Google Uri in the Miles table
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        /// 
        private void btnUpdateMilesTable_Click(object sender, RoutedEventArgs e)
        {
            //Update all chapters or only those with a 0 in the miles If not updating all then only get those where Miles = 0
            bool updateAll = UpdateAll.IsChecked.Value;
            //Lets check to see if anything needs to be update if update All  = false
            //If no records to update then show Message box
            bool recsToUpdate = false;
            if (!updateAll)
            {
                recsToUpdate = CheckZeroMiles();
                if (!recsToUpdate)
                {
                    MessageBox.Show(" There are no Miles that are 0");
                    return;
                }
            }
            UpdateMilesTable();
        }


        private void btnUpdateGoogleUri_Click(object sender, RoutedEventArgs e)
        {
            UpdateGoogleUrl();
        }

       

        private void btnCreateMiles_Click(object sender, RoutedEventArgs e)
        {

        }
        
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion
       
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
