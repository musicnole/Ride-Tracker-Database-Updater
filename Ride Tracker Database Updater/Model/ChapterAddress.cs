using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Ride_Tracker_Database_Updater.Model
{
   public class ChapterAddress : ObservableObject,IDataErrorInfo
    {
        public int ChapterAddressId { get; set; }

        public int ChapterId { get; set; }
        public string Error { get { return null; } }
        public Dictionary<string, string> ErrorCollection { get; private set; } = new Dictionary<string, string>();
        
        public string this[string name] {
            get
            {
                string result = null;
                switch (name)
                {
                    case "StreetAddress1":
                        if (string.IsNullOrWhiteSpace(StreetAddress1))
                            result = "StreetAddress1 cannot be empty";
                        else if (StreetAddress1.Length < 5)
                            result = "StreetAddress1 must be a minimum of 3 characters.";
                        break;

                    case "City":
                        if (string.IsNullOrWhiteSpace(City))
                            result = "City cannot be empty";
                        break;
                    
                    case "Zip":
                        if (string.IsNullOrWhiteSpace(Zip))
                             result = "Enter a valid Zip";
                        break;
                }

                if (ErrorCollection.ContainsKey(name))
                    ErrorCollection[name] = result;
                else if (result != null)
                    ErrorCollection.Add(name, result);

                OnPropertyChanged("ErrorCollection");
                return result;

            }
        }

        private string _streetAddress1;
        public string StreetAddress1
        {
            get { return _streetAddress1; }

            set {
                OnPropertyChanged(ref _streetAddress1, value);
            }
        }

        public string StreetAddress2 { get; set; }

        private string _city;
        public string City
        {
            get { return _city; }

            set
            {
                OnPropertyChanged(ref _city, value);
            }
        }

        public int StateId { get; set; }
        
        private string _zip;
       // string _usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";
        public string Zip
        {
            get { return _zip; }

            set
            {
                OnPropertyChanged(ref _zip, value);
            }
        }


        public string Active { get; set; }

        public DateTime DateModified { get; set; }

        public DateTime DateCreated { get; set; }
        
    }
}
