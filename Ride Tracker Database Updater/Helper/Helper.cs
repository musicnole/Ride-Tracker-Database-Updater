using Ride_Tracker_Database_Updater.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Ride_Tracker_Database_Updater.Helper
{
    public static class Helper
    {
        public static Dictionary<int,string> GetChapters()
        {
            Dictionary<int, string> ChapterDict = new Dictionary<int, string>();
            ChapterClassDataContext cdc = new ChapterClassDataContext();
            ChapterDict = cdc.Chapters.ToDictionary(x => x.ChapterId, x => x.ChapterName);

            return ChapterDict;
        }
       
        public static Dictionary<int,string> GetStates()
        {
            Dictionary<int, string> statesDict = new Dictionary<int, string>();
            StateDataContext sdc = new StateDataContext();

            statesDict = sdc.States.ToDictionary(x => x.Id, x => x.Name);
            
            return statesDict;
        }

        public static string GetStateCode(int stateId)
        {
            string stateName = string.Empty;
            StateDataContext sdc = new StateDataContext();
            stateName = sdc.States.Where(a => a.Id == stateId).Select(b => b.Code).FirstOrDefault();
            return stateName;
        }
       
        public static ChapterAddress GetChapterAddress(int chId)
        {
            ChapterAddress selChapAddr = new ChapterAddress();
            ChapterAddressDataContext cadc = new ChapterAddressDataContext();
            selChapAddr =  (ChapterAddress)cadc.ChapterAddresses.Where(s => s.ChapterId == chId).FirstOrDefault();
            return selChapAddr;
        }

        public static Chapter GetChapter(int chID)
        {
            Chapter selChapter = new Chapter();
            ChapterClassDataContext cdc = new ChapterClassDataContext();
            selChapter = (Chapter)cdc.Chapters.Where(a => a.ChapterId == chID).FirstOrDefault();
            return selChapter;
        }

        public static List<ChapterAddress> GetChapterAddressesAll()
        {
            List<ChapterAddress> chList = new List<ChapterAddress>();
            ChapterAddressDataContext cadc = new ChapterAddressDataContext();
            chList = cadc.ChapterAddresses.ToList() ;
            return chList;
        }

        public static void AddChapterToMilesTable(List<int> chAddList)
        {
            Dictionary<int, string> chList = new Dictionary<int, string>();
            chList = GetChapters();
            using (MilesDataContext mdc = new MilesDataContext())
            {
                // Loop through the Add list and then the list of current chapters to create the Mile
                foreach (int chId in chAddList)
                {
                    //This is for the From ID
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
                    //This is for the To ID
                    foreach (var ch in chList)
                    {
                        if (chId != ch.Key)
                        {
                            Mile mAdd = new Mile();
                            mAdd.ToId = chId;
                            mAdd.FromId = ch.Key;
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
        /// Method that updates the miles in the miles table
        /// </summary>
        /// <param name="originKey"></param>
        /// <param name="lookupOrigin"></param>
        /// <param name="lookupDestinationDict"></param>

        public static void UpdateMileage(int originKey, string lookupOrigin, Dictionary<int, string> lookupDestinationDict, bool updateAll = false)
        {
            int destKey = 0;
         
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
                            mile.Miles = (Int32)GoogleHelper.GetMileage(lookupOrigin, dest.Value);
                        }
                    }
                    else
                    {
                        //Get the Mile Record
                        var mRec = from mtable in mdc.Miles
                                   where mtable.FromId == originKey && mtable.ToId == destKey && mtable.Miles == 0
                                   select mtable;
                        foreach (Mile mile in mRec)
                        { 
                            mile.Miles = (Int32)GoogleHelper.GetMileage(lookupOrigin, dest.Value);
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

        public static void CreateChapterMilesUpdateLists(List<DAL.ChapterAddress> caList, List<Model.ChapterAddressMiles> camList, bool updateAll = false)
        {
            StringBuilder sblookupOrigin = new StringBuilder();
            string lookupOrigin = string.Empty;
            Dictionary<int, string> lookupDestinationDict = new Dictionary<int, string>();
            string test = string.Empty;
            //Get Mileage, Loop through list of Chapters to set Origin Address
            foreach (Model.ChapterAddressMiles cam in camList)
            {
                //Create Origin
                sblookupOrigin.Append(cam.StreetAddress);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.City);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(GetStateCode(cam.stateId));
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.Zip);
                lookupOrigin = sblookupOrigin.ToString();

                //Just to make life easy create a list of destination addresses
                lookupDestinationDict = CreateDestinationAddressList(cam.ChapterID, caList);

                //Make the call to google to get the miles between addresses The original address is being sent and a list of all addresses 
                // The update Mileage method will update all miles for 1 origin address
                // 20200819 Moved to helper so can update miles when creating a chapter
                UpdateMileage(cam.ChapterID, lookupOrigin, lookupDestinationDict, updateAll);

                ////Update the Miles table Google uri
                //UpdateGoogleMilesUri(cam.ChapterID, lookupDestinationDict);

                sblookupOrigin.Clear();
                lookupOrigin = string.Empty;
            }
        }

        public static Dictionary<int, string> CreateDestinationAddressList(int chIdOrigin, List<DAL.ChapterAddress> caList)
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
                    sblookupDestination.Append(GetStateCode((Int32)ca.StateId));
                    sblookupDestination.Append(" ");
                    sblookupDestination.Append(ca.Zip);
                    lookupDestination = sblookupDestination.ToString();
                }
                else continue;
                lookupDestinationList.Add(ca.ChapterId, lookupDestination);
                sblookupDestination.Clear();
            }
            return lookupDestinationList;
        }

        public static void UpdateGoogleMilesUri(int originKey, Dictionary<int, string> lookupDestinationDict, bool updateAll = false)
        {
            int destKey = 0;
            //bool updateAll = UpdateAll.IsChecked.Value;
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
                        mile.GoogleUri = GoogleHelper.CreateGoogleMilesTableLink(originKey, mile.ToId);
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

        public static void UpdateGoogleUrl(bool updateAll = false)
        {
            //Get all The Chapters into a List
            Dictionary<int, string> chList = new Dictionary<int, string>();
            List<int> chAddList = new List<int>();
            chList = GetChapters();

            //Get All the Chapter Addresses into a List
            List<DAL.ChapterAddress> caList = new List<DAL.ChapterAddress>();
            caList = GetChapterAddressesAll();

            // Create the List of clubhouse addresses
            List<Model.ChapterAddressMiles> camList = new List<Model.ChapterAddressMiles>();
            camList = CreateClubhouseAddressList(chList, caList);

            StringBuilder sblookupOrigin = new StringBuilder();
            string lookupOrigin = string.Empty;
            Dictionary<int, string> lookupDestinationDict = new Dictionary<int, string>();
            string test = string.Empty;
            //Loop through list of Chapters to set Origin Address
            foreach (Model.ChapterAddressMiles cam in camList)
            {
                //Create Origin
                sblookupOrigin.Append(cam.StreetAddress);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.City);
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(GetStateCode(cam.stateId));
                sblookupOrigin.Append(" ");
                sblookupOrigin.Append(cam.Zip);
                lookupOrigin = sblookupOrigin.ToString();

                //Just to make life easy create a list of destination addresses
                lookupDestinationDict = CreateDestinationAddressList(cam.ChapterID, caList);

                //Update the Miles table Google uri
                UpdateGoogleMilesUri(cam.ChapterID, lookupDestinationDict, updateAll);

                sblookupOrigin.Clear();
                lookupOrigin = string.Empty;
            }

        }

        public static List<Model.ChapterAddressMiles> CreateClubhouseAddressList(Dictionary<int, string> chList, List<DAL.ChapterAddress> caList)
        {
            //Create a list of Clubhouses and address 
            List<Model.ChapterAddressMiles> camList = new List<Model.ChapterAddressMiles>();
            //Add Clubhouse and the addresses
            foreach (var ch in chList)
            {
                Model.ChapterAddressMiles cam = new Model.ChapterAddressMiles();
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

    }
}
