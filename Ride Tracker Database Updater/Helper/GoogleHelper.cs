using Ride_Tracker_Database_Updater.DAL;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Helper
{
    public static class GoogleHelper
    {
        private static IEnumerable chList;
        private const string uriGoogle = "https://www.google.com/";
        private const string gUri = "https://www.google.com/maps/place/" + "{SA}+" + "{City}+" + "{State}+" + "{Zip}/";
        private const string gMapUri = "https://www.google.com/maps/dir/" + "{FromSA}+" + "{FromCity}+" + "{FromState}+" + "{FromZip}/"+ "{ToSA}+" + "{ToCity}+" + "{ToState}+" + "{ToZip}/";
        private static string ApiKey { get; set; }

        public static IEnumerable GetChapters()
        {

            ChapterClassDataContext cdc = new ChapterClassDataContext();
            chList = (from a in cdc.Chapters select a).ToList();
            //Putting Chapters into DataGrid to see the list
            //dgChapters.ItemsSource = chList;
            return chList;
        }
        
        /// <summary>
        /// Creates the Google Link in the Miles table, shows the best google route, Below is an example
        /// ///https://www.google.com/maps/dir/2472+Marietta+Hwy+Canton+GA+30114/221+Amber+Ridge+Rd+Statham+GA+30680/
        /// </summary>
        /// <param name="toChapter"></param>
        /// <param name="fromChapter"></param>
        /// <returns></returns>
        public static string CreateGoogleMilesTableLink(int fromChapter, int toChapter)
        {
            string milesgUri = string.Empty;
            ChapterAddressDataContext cda = new ChapterAddressDataContext();
           
            ChapterAddress toChapterAddress = new ChapterAddress();
            toChapterAddress = Helper.GetChapterAddress(toChapter);
           
            ChapterAddress fromChapterAddress = new ChapterAddress();
            fromChapterAddress = Helper.GetChapterAddress(fromChapter);
            
            string fromStateCode = GetStateName((Int32)fromChapterAddress.StateId);
            string toStateCode = GetStateName((Int32)toChapterAddress.StateId);

            milesgUri = gMapUri.Replace("{FromSA}",fromChapterAddress.StreetAddress1)
                               .Replace("{FromCity}",fromChapterAddress.City)
                               .Replace("{FromState}", fromStateCode)
                               .Replace("{FromZip}", fromChapterAddress.Zip)
                               .Replace("{ToSA}", toChapterAddress.StreetAddress1)
                               .Replace("{ToCity}", toChapterAddress.City)
                               .Replace("{ToState}", toStateCode)
                               .Replace("{ToZip}", toChapterAddress.Zip);

            return milesgUri;
        }

        public static Dictionary<int, string> CreateGoogleMapLink()
        {
            string googleuri = string.Empty;
            List<string> googleUrisList = new List<string>();
            Dictionary<int, string> chGlink = new Dictionary<int, string>();
            foreach (DAL.Chapter ch in chList)
            {
                //Get the Chapter Address
                DAL.ChapterAddress chA;
                ChapterAddressDataContext cadc = new ChapterAddressDataContext();
                chA = (from ca in cadc.ChapterAddresses where ca.ChapterId == ch.ChapterId select ca).FirstOrDefault();
                string streetAddress = (chA.StreetAddress2 == null) ? chA.StreetAddress1 : string.Concat(chA.StreetAddress1, " ", chA.StreetAddress2);

                //StateDataContext sdc = new StateDataContext();
                //string stateCode = (from st in sdc.States where st.Id == chA.StateId select st.Code).FirstOrDefault();
                string stateCode = GetStateName(Convert.ToInt32(chA.StateId));
                googleuri = gUri.Replace("{SA}", streetAddress).Replace("{City}", chA.City).Replace("{State}", stateCode).Replace("{Zip}", chA.Zip.ToString());

                chGlink.Add(ch.ChapterId, googleuri);
            }

            return chGlink;
        }
        /// <summary>
        /// Creates the Chapters Google Link 
        /// </summary>
        /// <param name="streetAddress"></param>
        /// <param name="City"></param>
        /// <param name="stateId"></param>
        /// <param name="Zip"></param>
        /// <returns></returns>
        public static string CreateChapterGLink(string streetAddress, string City, int stateId, string Zip)
        {
            string googleuri = string.Empty;
            string stateCode = GetStateName(stateId);

            googleuri = gUri.Replace("{SA}", streetAddress).Replace("{City}", City).Replace("{State}", stateCode).Replace("{Zip}", Zip);
            return googleuri;
        }

        private static string GetStateName(int stateId)
        {
            StateDataContext sdc = new StateDataContext();
            string stateCode = (from st in sdc.States where st.Id == stateId select st.Code).FirstOrDefault();
            return stateCode;
        }

        public static string GetChGLink(int ChID)
        {
            ChapterClassDataContext cdc = new ChapterClassDataContext();
            string gLink = (from a in cdc.Chapters where a.ChapterId==ChID select a.GoogleLink).FirstOrDefault(); ;
            return gLink;
        }
        
        private static decimal ParseMilesResult(MilesResponse results)
        {
            decimal distance = 0;
            string resultMileage = string.Empty;
            try
            {
                foreach (Row row in results.rows)
                {
                    foreach (Elements element in row.elements)
                    {
                        resultMileage = element.distance.distance.ToString().Replace("mi", string.Empty).Trim();
                        distance = Math.Ceiling(decimal.Parse(resultMileage));
                    }
                }
            }
            catch (Exception)
            {
                throw;
            }
            return distance;
        }
       
        //https://www.google.com/maps/dir/2472+Marietta Hwy+Canton+GA+30114/216+Grover Burt Road+Dawsonville+GA+30534/
        /// Api Call
        /// https://maps.googleapis.com/maps/api/distancematrix/json?origins=Seattle&destinations=San+Francisco&key=YOUR_API_KEY
        public static decimal GetMileage(string lookupOrigin, string lookupDestination)
        {
            decimal miles = 0;
            try
            {
                ApiKey = ConfigurationManager.AppSettings["APIKey"];

                StringBuilder sbRequest = new System.Text.StringBuilder();
                sbRequest.Append("https://maps.googleapis.com/maps/api/distancematrix/json?origins=");
                sbRequest.Append(lookupOrigin);

                sbRequest.Append("&destinations=");
                sbRequest.Append(lookupDestination);
                sbRequest.Append("&units=imperial");
                sbRequest.Append("&key=");
                sbRequest.Append(ApiKey);
                MakeApiRequest makeRequest = new MakeApiRequest();
                 makeRequest.HttpGet(sbRequest.ToString());
                miles = ParseMilesResult(makeRequest.HttpGet(sbRequest.ToString()));
            }
            catch (Exception)
            {
                return 0;
                throw;
            }

            return miles;
        }

    }
}
