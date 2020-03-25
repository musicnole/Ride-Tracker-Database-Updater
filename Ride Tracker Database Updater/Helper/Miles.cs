using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Helper
{
    [DataContract]
    public class MilesResponse
    {
        [DataMember(Name = "destination_addresses")]
        public string[] destination_addresses { get; set; }
        [DataMember(Name = "origin_addresses")]
        public string[] origin_addresses { get; set; }
        [DataMember(Name = "rows")]
        public Row[] rows { get; set; }
        [DataMember(Name = "status")]
        public string status { get; set; }
    }


    [DataContract]
    public class Row
    {
        [DataMember(Name = "elements")]
        public Elements[] elements { get; set; }
    }

    [DataContract]
    public class Elements
    {
        [DataMember(Name = "distance")]
        public ReturnValues distance { get; set; }
        [DataMember(Name = "duration")]
        public ReturnValues duration { get; set; }
        [DataMember(Name = "status")]
        public string status { get; set; }
    }

    [DataContract]
    public class ReturnValues
    {
        [DataMember(Name = "text")]
        public string distance { get; set; }
        [DataMember(Name = "value")]
        public string duration { get; set; }
    }
}
