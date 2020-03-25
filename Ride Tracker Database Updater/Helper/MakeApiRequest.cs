using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Helper
{
    public class MakeApiRequest
    {

        public MilesResponse HttpGet(string url)
        {
            HttpWebRequest req = WebRequest.Create(url)
                                 as HttpWebRequest;

            using (HttpWebResponse resp = req.GetResponse()
                                          as HttpWebResponse)
            {
                if (resp.StatusCode != HttpStatusCode.OK)
                    throw new Exception(String.Format(
                    "Server error (HTTP {0}: {1}).",
                    resp.StatusCode,
                    resp.StatusDescription));

                DataContractJsonSerializer jsonSerializer = new DataContractJsonSerializer(typeof(MilesResponse));
                object objResponse = jsonSerializer.ReadObject(resp.GetResponseStream());
                MilesResponse jsonResponse
                = objResponse as MilesResponse;

                return jsonResponse;

            }
        }
    }
}
