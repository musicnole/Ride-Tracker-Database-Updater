using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Model
{
    public class Chapter:IDataErrorInfo
    {
        public string Error
        {
            get { return null; }
        }

        public string this[string columnName]
        {

            get
            {
                string result = string.Empty;
                if (columnName == "ChapterName")
                {
                    if (this.ChapterName == "")
                        result = "ChapterName Cannot be empty";
                }
                return result;
            }
        }

        public int ChapterId { get; set; }
        public string ChapterName { get; set; }

        public string ChapterNickName { get; set; }

        public int ClubHouseAddressID { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateCreated { get; set; }

        public string GoogleLink { get; set; }

        public string Active { get; set; }

    }
}
