using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Model
{
    public class ChapterMiles
    {
        public int MilesId { get; set; }
        public int FromID { get; set; }
        public string FromChapter { get; set; }
        public int ToId { get; set; }
        public string ToChapter { get; set; }
        public int  Miles { get; set; }
        public string  GoogleUri { get; set; }

    }
}
