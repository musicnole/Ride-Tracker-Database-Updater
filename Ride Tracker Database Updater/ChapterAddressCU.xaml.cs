using Ride_Tracker_Database_Updater.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Ride_Tracker_Database_Updater
{
    /// <summary>
    /// Interaction logic for UpdateChapterAddress.xaml
    /// </summary>
    public partial class UpdateChapterAddress : Window
    {
        public Dictionary<int, string> ChapterDict = new Dictionary<int, string>();
        private int selChapt { get; set; }
        private int selState { get; set; }
        private const string _bullet = "\u2022 ";
        private const string _address1Error = "Please put in a street address.";
        private const string _cityError = "Please put in a city.";
        private const string _zipError = "Please put in a Zip Code.";
        private const string _stateError = "Please Select the State.";
        private const string _gLinkTestError = "Must have a google link to test!";
        private const string _usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";
        private const string _successMsg = " Congratulations you have successfuly updated Chapter ";
        public UpdateChapterAddress()
        {
            InitializeComponent();
            FillCombo();
        }

        private void FillCombo()
        {
            cboChapter.ItemsSource = Helper.Helper.GetChapters();
            cboState.ItemsSource = Helper.Helper.GetStates();
        }

        private void cboChapter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshErrorBlock();
            selChapt = Convert.ToInt32( cboChapter.SelectedValue);
            LoadChapterInfo();
        }

        private void LoadChapterInfo()
        {
            ChapterAddress selChapterAddress = new ChapterAddress();
            selChapterAddress = Helper.Helper.GetChapterAddress(selChapt);
            txtStreetAddr1.Text = (selChapterAddress.StreetAddress1 == null || selChapterAddress.StreetAddress1.ToString() == string.Empty) ? string.Empty : selChapterAddress.StreetAddress1.ToString(); 
            txtCity.Text = (selChapterAddress.City == null || selChapterAddress.City.ToString() == string.Empty) ? string.Empty : selChapterAddress.City.ToString(); 
            txtZip.Text = (selChapterAddress.Zip == null || selChapterAddress.Zip.ToString() == string.Empty) ? string.Empty : selChapterAddress.Zip.ToString(); 
            txtGlink.Text = Helper.GoogleHelper.GetChGLink(selChapt);
            cboState.SelectedValue = (selChapterAddress.StateId == null) ? 1: selChapterAddress.StateId ;
        }
       
        private void UpdateChapterAddress_Click(object sender, RoutedEventArgs e)
        {
            bool validForm;
            bool updateSuccess = true;
            string chapterName = string.Empty;
            string newGLink = string.Empty;

            RefreshErrorBlock();
            validForm = ValidateForm();
            if (validForm) {
                //Update the Chapter Address
                int chID = selChapt;
                ChapterAddressDataContext cadc = new ChapterAddressDataContext();
                ChapterAddress chA = new ChapterAddress();
                chA = (from chaA in cadc.ChapterAddresses
                            where chaA.ChapterId == chID
                            select chaA).FirstOrDefault();
                
                chA.StreetAddress1 = txtStreetAddr1.Text.ToString();
                chA.StateId = Convert.ToInt32(cboState.SelectedValue);
                chA.City = txtCity.Text.ToString();
                chA.Zip = txtZip.Text;
                string streetAddress = (chA.StreetAddress2 == null) ? chA.StreetAddress1 : string.Concat(chA.StreetAddress1, " ", chA.StreetAddress2);
                chA.DateModified = System.DateTime.Now;
                try
                {
                    cadc.SubmitChanges();
                }
                catch (Exception exceptCA)
                {
                    MessageBox.Show(exceptCA.Message.ToString());
                    updateSuccess = false;
                }

                try
                {
                    //Update Chapter Google Link
                    ChapterClassDataContext cdc = new ChapterClassDataContext();
                    Chapter chU = new Chapter();
                    chU = (from ch in cdc.Chapters
                           where ch.ChapterId == chID
                           select ch).FirstOrDefault();
                    newGLink  = Helper.GoogleHelper.CreateChapterGLink(streetAddress, chA.City, Convert.ToInt32(cboState.SelectedValue), chA.Zip);
                    chU.GoogleLink = newGLink;
                    txtGlink.Text = newGLink;
                    chapterName = chU.ChapterName;
                    chU.DateModified = System.DateTime.Now;
                }
                catch (Exception exceptCh)
                {
                    MessageBox.Show(exceptCh.Message.ToString());
                    updateSuccess = false;
                }

                if (updateSuccess)
                    MessageBox.Show(string.Concat(_successMsg, chapterName));
            }
        }

        private bool ValidateForm()
        {
            bool isFormValid = true;

            if (Validation.GetHasError(txtStreetAddr1))
            {
                errListChapterAddr.Visibility = Visibility.Visible;
                Paragraph errPSA1 = new Paragraph();
                errPSA1.Inlines.Add(_bullet + _address1Error);
                errListChapterAddr.Document.Blocks.Add(errPSA1);
                isFormValid = false;
            }

            if (Validation.GetHasError(txtCity))
            {
                errListChapterAddr.Visibility = Visibility.Visible;
                Paragraph errPSCity = new Paragraph();
                errPSCity.Inlines.Add(_bullet + _cityError);
                errListChapterAddr.Document.Blocks.Add(errPSCity);
                isFormValid = false;
            }

            if (Validation.GetHasError(txtZip))
            {
                errListChapterAddr.Visibility = Visibility.Visible;
                Paragraph errPSZip = new Paragraph();
                errPSZip.Inlines.Add(_bullet + _zipError);
                errListChapterAddr.Document.Blocks.Add(errPSZip);
                isFormValid = false;
            }

            if ((!Regex.Match(txtZip.Text.ToString(), _usZipRegEx).Success) && !Validation.GetHasError(txtZip))
            {
                errListChapterAddr.Visibility = Visibility.Visible;
                Paragraph errPSZip = new Paragraph();
                errPSZip.Inlines.Add(_bullet + _zipError);
                errListChapterAddr.Document.Blocks.Add(errPSZip);
                isFormValid = false;
            }

            if (cboState.SelectedItem == null)
            {
                errListChapterAddr.Visibility = Visibility.Visible;
                Paragraph errPSState = new Paragraph();
                errPSState.Inlines.Add(_bullet + _stateError);
                errListChapterAddr.Document.Blocks.Add(errPSState);
                isFormValid = false;
            }

            return isFormValid;
        }

        private void cboState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selState = Convert.ToInt32(cboState.SelectedValue);
        }

        private void TestGoogleURL_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(txtGlink.Text.ToString()))
            {
                TestGoogleURL tg = new TestGoogleURL(txtGlink.Text.ToString());
                tg.ShowDialog();
            }
            else
            {
                RefreshErrorBlock();
                if (Validation.GetHasError(txtStreetAddr1))
                {
                    errListChapterAddr.Visibility = Visibility.Visible;
                    Paragraph errGLink = new Paragraph();
                    errGLink.Inlines.Add(_bullet + _gLinkTestError);
                    errListChapterAddr.Document.Blocks.Add(errGLink);
                }
            }


        }

        private void RefreshErrorBlock()
        {
            errListChapterAddr.Document.Blocks.Clear();
            errListChapterAddr.Visibility = Visibility.Hidden;
        }
    }
}

