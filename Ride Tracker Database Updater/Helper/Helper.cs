using Ride_Tracker_Database_Updater.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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


    }
}
