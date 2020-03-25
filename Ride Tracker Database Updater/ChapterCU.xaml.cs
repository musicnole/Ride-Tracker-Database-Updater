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
    /// Interaction logic for ChapterCU.xaml
    /// </summary>
    public partial class ChapterCU : Window
    {
        private int selChapt { get; set; }
        private int selState { get; set; }
        private const string _bullet = "\u2022 ";
        private const string _chapterError = "Please put in a Chapter Name";
        private const string _address1Error = "Please put in a street address.";
        private const string _cityError = "Please put in a city.";
        private const string _zipError = "Please put in a Zip Code.";
        private const string _stateError = "Please Select the State.";
        private const string _gLinkTestError = "Must have a google link to test!";
        private const string _usZipRegEx = @"^\d{5}(?:[-\s]\d{4})?$";
        private const string _successUpdateMsg = " Congratulations you have Successfully updated Chapter ";
        private const string _successCreateMsg = " Congratulations you have Successfully created Chapter ";
        private const string _modeUpdate = "Update";
        private const string _modeCreate = "Create";
        private const string _successMsg = " Congratulations you have successfuly updated Chapter ";
        private string _mode { get; set; }
        public ChapterCU(string Mode)
        {
            InitializeComponent();
            switch (Mode)
            {
                case _modeCreate:
                    ShowCreateMode();
                    _mode = _modeCreate;
                    break;

                case _modeUpdate:
                    ShowUpdateMode();
                    _mode = _modeUpdate;
                    break;
            }

            ////SetCreateTestSettings();

        }
        
        #region Create Mode
        private void ShowCreateMode()
        {
            lblMode.Content = "Create a new Chapter: ";
            btnChapter.Content = string.Concat(_modeCreate, " Chapter");
            cboState.ItemsSource = Helper.Helper.GetStates();
            cboChapter.Visibility = Visibility.Hidden;
        }
        private void CreateChapter()
        {
            bool validForm;
            string newGLink = string.Empty;
            int newChKey = 0;
            RefreshErrorBlock();
            validForm = ValidateForm();
            
            if (validForm) {
                try
                {
                    ChapterClassDataContext cdc = new ChapterClassDataContext();
                    Chapter ch = new Chapter();
                    ch.ChapterName = txtChapter.Text.ToString();
                    ch.ChapterNickName = txtChapterNN.Text.ToString();
                    ch.Active = true;
                    cdc.Chapters.InsertOnSubmit(ch);
                    cdc.SubmitChanges();
                    newChKey = ch.ChapterId;
                }
                catch (Exception exceptCh)
                {
                    MessageBox.Show(exceptCh.ToString());
                }
                if(newChKey!=0)
                {
                    CreateChapterAddress(newChKey);
                }
            }
        }
        private void CreateChapterAddress(int chID)
        {
            int newChaKey = 0;
            ChapterAddressDataContext cadc = new ChapterAddressDataContext();
            ChapterAddress ca = new ChapterAddress();
            ca.ChapterId = chID;
            ca.Active = true;
            ca.StreetAddress1 = txtStreetAddr1.Text.ToString();
            ca.City = txtCity.Text.ToString();
            ca.StateId = Convert.ToInt32(cboState.SelectedValue);
            ca.Zip = txtZip.Text.ToString();
            ca.DateCreated = System.DateTime.Now;
            ca.DateModified = System.DateTime.Now;
            try
            {
                cadc.ChapterAddresses.InsertOnSubmit(ca);
                cadc.SubmitChanges();
                newChaKey = ca.ChapterAddressId;
            }
            catch (Exception exceptCha)
            {
                MessageBox.Show(exceptCha.ToString());
            }
            //MessageBox.Show("New Chapter ID " + chID.ToString() +" New Chapter Address Key  "+ newChaKey.ToString());
            //If Successful Update the Google Link
            if(newChaKey > 0)
                {
                UpdateChapterGLink(chID, newChaKey);
            }
         }
        #endregion

                                   
        #region Update Mode 
        private void ShowUpdateMode()
        {
            FillCombo();
            txtChapter.Visibility = Visibility.Hidden;
            lblMode.Content = "Update Chapter Info: ";
            btnChapter.Content = "Update Chapter";
            btnChapter.Content = string.Concat(_modeUpdate, " Chapter");
        }

        private void UpdateChapterAddressInfo()
        {
            bool validForm;
            bool updateSuccess = true;
            string chapterName = string.Empty;
            string newGLink = string.Empty;

            RefreshErrorBlock();
            validForm = ValidateForm();
            if (validForm)
            {
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
                    newGLink = Helper.GoogleHelper.CreateChapterGLink(streetAddress, chA.City, Convert.ToInt32(cboState.SelectedValue), chA.Zip);
                    chU.GoogleLink = newGLink;
                    txtGlink.Text = newGLink;
                    chapterName = chU.ChapterName;
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

        private void UpdateChapterInfo()
        {
            bool validForm;
            bool updateSuccess = true;
            string newGLink = string.Empty;
            int chID = Convert.ToInt32(cboChapter.SelectedValue);
            RefreshErrorBlock();
            validForm = ValidateForm();
            if (validForm)
            {
                newGLink = Helper.GoogleHelper.CreateChapterGLink(txtStreetAddr1.Text.ToString(), txtCity.Text.ToString(), Convert.ToInt32(cboState.SelectedValue), txtZip.Text.ToString());
                ChapterClassDataContext cdc = new ChapterClassDataContext();
                Chapter ch = new Chapter();
                ch = (from c in cdc.Chapters
                       where c.ChapterId == chID
                       select c).FirstOrDefault();
                
                ch.ChapterNickName = txtChapterNN.Text.ToString();
                ch.GoogleLink = newGLink;
                ch.DateModified = System.DateTime.Now;

                try
                {
                    cdc.SubmitChanges();

                }
                catch (Exception exceptCH)
                {
                    MessageBox.Show(exceptCH.Message.ToString());
                    updateSuccess = false;
                }
            }
        }


        private void cboState_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            selState = Convert.ToInt32(cboState.SelectedValue);
        }

        private void cboChapter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RefreshErrorBlock();
            selChapt = Convert.ToInt32(cboChapter.SelectedValue);
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
            cboState.SelectedValue = (selChapterAddress.StateId == null) ? 1 : selChapterAddress.StateId;
            Chapter selChapter = new Chapter();
            selChapter = Helper.Helper.GetChapter(selChapt);
            txtChapterNN.Text = (selChapter.ChapterNickName == null || selChapter.ChapterNickName.ToString() == string.Empty) ? string.Empty : selChapter.ChapterNickName.ToString();
        }

        #endregion

        #region Common

        private void UpdateChapterGLink(int chID, int chaID)
        {
            string newGLink = string.Empty;
            bool updateSuccess = true;
            string chapterName = string.Empty;
            //Update Chapter Google Link
            ChapterClassDataContext cdc = new ChapterClassDataContext();
            Chapter chU = new Chapter();
            chU = (from ch in cdc.Chapters
                   where ch.ChapterId == chID
                   select ch).FirstOrDefault();
            newGLink = Helper.GoogleHelper.CreateChapterGLink(txtStreetAddr1.Text.ToString(), txtCity.Text.ToString(), Convert.ToInt32(cboState.SelectedValue), txtZip.Text.ToString());
            chU.GoogleLink = newGLink;
            chU.ClubHouseAddressID = chaID;
            chU.DateCreated = System.DateTime.Now;
            chU.DateModified = System.DateTime.Now;
            txtGlink.Text = newGLink;
            chapterName = chU.ChapterName;
            
            try
            {
                cdc.SubmitChanges();

            }
            catch (Exception exceptCh)
            {
                MessageBox.Show(exceptCh.Message.ToString());
                updateSuccess = false;
            }

            if (updateSuccess)
                if (_mode == _modeCreate) 
                {
                        MessageBox.Show(string.Concat(_successCreateMsg, chapterName));
                }
                    else
                {
                    MessageBox.Show(string.Concat(_successMsg, chapterName));
                }
                
        }

        private bool ValidateForm()
        {
            bool isFormValid = true;

            if (_mode == _modeCreate) {
                if (string.IsNullOrWhiteSpace(txtChapter.Text))
                {
                    errListChapterAddr.Visibility = Visibility.Visible;
                    Paragraph errPSCh = new Paragraph();
                    errPSCh.Inlines.Add(_bullet + _chapterError);
                    errListChapterAddr.Document.Blocks.Add(errPSCh);
                    isFormValid = false;
                }
            
            }


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

        private void FillCombo()
        {
            cboChapter.ItemsSource = Helper.Helper.GetChapters();
            cboState.ItemsSource = Helper.Helper.GetStates();
        }

        private void RefreshErrorBlock()
        {
            errListChapterAddr.Document.Blocks.Clear();
            errListChapterAddr.Visibility = Visibility.Hidden;
        }
        #endregion


        #region Buttons
        private void Chapter_Click(object sender, RoutedEventArgs e)
        {
            switch (_mode)
            {
                case _modeCreate:
                    CreateChapter();
                   
                    break;

                case _modeUpdate:
                    UpdateChapterInfo();
                    UpdateChapterAddressInfo();
                    break;

            }
        }

        private void TestChapterURL_Click(object sender,RoutedEventArgs e)
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
        
        private void btnMenu_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
        #endregion

        private void SetCreateTestSettings()
        {
            txtChapter.Text = "Memphis";
            txtChapterNN.Text = "Hound Dogs";
            txtStreetAddr1.Text = "696 Jackson Ave";
            txtCity.Text = "Memphis";
            txtZip.Text = "38105";
            cboState.SelectedValue = 43;
        }

    }
}
