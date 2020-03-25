using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Model
{
    public class ChapterAddressMiles
    {
        public int ChapterID { get; set; }
        
        public string ChapterName { get; set; }
        
        public int ClubhouseAddressID { get; set; }

        public string StreetAddress { get; set; }
        
        public string City { get; set; }

        public int stateId { get; set; }

        public string Zip { get; set; }

        public int Miles { get; set; }
        
        public string GoogleTripUri { get; set; }
        

    }
}
