using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;
using System.Text.Json;
using System.Collections.ObjectModel;
using System.Text.RegularExpressions;
using System.ComponentModel;
using Microsoft.Win32;
using CsvHelper;
using System.Globalization;
using LSSD.Lunch;

namespace LunchManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient client; //the application shares one http client

        public ObservableCollection<FoodItem> unsyncedFoodItems { get; set; }
        public static ObservableCollection<FoodItem> displayedFoodItems { get; set; }
        public ObservableCollection<Transaction> transactions { get; set; }
        public ObservableCollection<Student> students { get; set; }
        public ObservableCollection<Student> displayedStudents { get; set; }

        private School thisSchool;
        private LoadingBox loadingWindow;
        private AddNewFoodItemHelper addNewFoodItemWindow;

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.MainWindow = this;

            transactions = new ObservableCollection<Transaction>();
            students = new ObservableCollection<Student>();
            unsyncedFoodItems = new ObservableCollection<FoodItem>();
        }

        private bool reloadConfigFile() 
        {

            return false;
        }

        /// <summary>
        /// Event handler for the main window's Loaded event.
        /// </summary>
        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            // Show loading window
            loadingWindow = new LoadingBox(this);
            loadingWindow.Show();
            IsEnabled = false;

            // Load config file
            reloadConfigFile();

            

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            string apiUri = "";
            string thisSchoolID = "";
            try
            {
                apiUri = configFile.AppSettings.Settings["apiUri"].Value.ToString();
                thisSchoolID = configFile.AppSettings.Settings["thisSchool"].Value;
            }
            catch
            {
                MessageBox.Show("There was an error reading from the configuration file, closing the program.", "Missing configuration file", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            if (string.IsNullOrEmpty(thisSchoolID) || string.IsNullOrEmpty(apiUri))
            {
                Trace.WriteLine("school or uri was null"); //DEBUG
                MessageBox.Show("There was an error reading from the configuration file, closing the program.", "Missing configuration file", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            ApiHelper.Init(apiUri); //initializes settings for the HttpClient
            client = ApiHelper.ApiClient; //gets the newly initialized HttpClient

            await GetThisSchoolFromIdAsync(thisSchoolID);
            if (thisSchool == null)
            {
                Trace.WriteLine("thisSchool was null"); //DEBUG
                return;
            }
            this.Title = "Lunch Manager - " + thisSchool.Name;
            addNewFoodItemWindow = new AddNewFoodItemHelper(this, thisSchool);

            transactions = await GetTransactionsBetweenAsync(DateTime.Today.AddDays(-1), DateTime.Today, 5000); //gets the previous day's transactions
            LoadOverviewStats();
            if (transactions == null || transactions.Count == 0)
            {
                transactions = await GetTransactionsAsync(50);
            }
            students = await GetStudentsAsync();
            displayedFoodItems = await GetFoodItemsAsync();

            displayedStudents = students;
            //set up data binding
            dataGridFoodTypes.ItemsSource = displayedFoodItems;
            dataGridStudents.ItemsSource = displayedStudents;
            ObservableCollection<int> numTransactionsOptions = new ObservableCollection<int> { 50, 100, 250, 500, 750, 1000 };
            cbNumTransactionsToShow.ItemsSource = numTransactionsOptions;

            cbNumTransactionsToShow.SelectedIndex = 0;


            //dataGridFoodTypes.DataContext = this;
            btnDeleteFoodItem.IsEnabled = false;
            txtNumStudentsShown.Text = "Showing " + displayedStudents.Count + " of " + students.Count + " students";
            txtNumTransactionsShown.Text = "Showing " + transactions.Count + " transactions";
            txtNumFoodTypesShown.Text = "Showing " + displayedFoodItems.Count + " of " + displayedFoodItems.Count + " food types";



            loadingWindow.Hide();
            IsEnabled = true;
        }


        /**<summary>Loads the school object from the database given the school's ID.</summary>
         * <param name="thisSchoolID">string, the ID of the requested school.</param>
         */
        private async Task GetThisSchoolFromIdAsync(string thisSchoolID)
        {
            try
            {
                var response = await client.GetAsync("api/Schools/" + thisSchoolID);
                thisSchool = await response.Content.ReadAsAsync<School>();
                Trace.WriteLine("get this school from ID response status: " + response.StatusCode); //DEBUG
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

        }

        /**<summary>Sets the text in the overview tab to display important data.</summary>
         */
        private void LoadOverviewStats()
        {
            if (transactions.Count == 5000)
            {
                txtOverviewStats.Text = "Yesterday, 5000 or more transactions were sent to the server.";
            }
            else if (transactions.Count == 0)
            {
                txtOverviewStats.Text = "Yesterday, 0 transactions were sent to the server. If this number is always 0, contact your system administrator.";
                txtOverviewStats.Foreground = Brushes.Red;
            }
            else
            {
                txtOverviewStats.Text = "Yesterday, " + transactions.Count + " transactions were sent to the server.";
            }

        }

        /**<summary>Gets the current school's FoodItems.</summary>
         * <returns>The FoodItems that are available at the current school.</returns>
         */
        private async Task<ObservableCollection<FoodItem>> GetFoodItemsAsync()
        {
            //HttpResponseMessage responseFood;
            ObservableCollection<FoodItem> newFoodItemsCollection = new();
            try
            {
                //responseFood = await client.GetAsync("api/FoodItem/School/" + thisSchool.ID);
                //newFoodItemsCollection = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>(); //is null
                var responseFood = await client.GetAsync("api/FoodItems/School/" + thisSchool.Id.ToString());
                newFoodItemsCollection = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();
                if (newFoodItemsCollection == null) //DEBUG
                {
                    Trace.WriteLine("newFoodItemsCollection is null - GetFoodItemsAsync");
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            return newFoodItemsCollection;
        }

        /**<summary>Gets the students for the current school from the database asynchronously.</summary>
         * <returns>Students that go to the current school.</returns>
         */
        private async Task<ObservableCollection<Student>> GetStudentsAsync()
        {
            //HttpResponseMessage responseFood;
            ObservableCollection<Student> newStudentsCollection = new();
            try
            {
                //responseFood = await client.GetAsync("api/FoodItem/School/" + thisSchool.ID);
                //newFoodItemsCollection = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>(); //is null
                var responseFood = await client.GetAsync("api/Students/School/" + thisSchool.Id.ToString());
                newStudentsCollection = await responseFood.Content.ReadAsAsync<ObservableCollection<Student>>();
                if (newStudentsCollection == null) //DEBUG
                {
                    Trace.WriteLine("newStudentsCollection is null - GetStudentsAsync");
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            return newStudentsCollection;
        }

        /**<summary>Gets the x most recent transactions from the database asynchronously.</summary>
         * <param name="numToLoad">The number of transactions to load from the database.</param>
         * <returns>The x most recent transactions.</returns>
         */
        private async Task<ObservableCollection<Transaction>> GetTransactionsAsync(int numToLoad)
        {
            ObservableCollection<Transaction> newTransactionsCollection = new();
            try
            {
                var responseTransactions = await client.GetAsync("api/Transactions/Recent/" + thisSchool.Id.ToString() + "/" + numToLoad);
                newTransactionsCollection = await responseTransactions.Content.ReadAsAsync<ObservableCollection<Transaction>>();
                if (newTransactionsCollection == null) //DEBUG
                {
                    Trace.WriteLine("newTransactionsCollection is null - GetTransactionsAsync");
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            return newTransactionsCollection;
        }

        /**<summary>Loads the students, schools, and food items into the main window's lists.
         * Use the individual loading methods instead.</summary>
         */
        private async Task GetDataAsync()
        {

            try
            {
                var responseStudents = await client.GetAsync("api/Students/School/" + thisSchool.Id.ToString());
                students = await responseStudents.Content.ReadAsAsync<ObservableCollection<Student>>();

                Trace.WriteLine("get students response status: " + responseStudents.StatusCode); //DEBUG
                if (this.students != null) //DEBUG
                {
                    foreach (Student i in students)
                    {
                        Trace.WriteLine("Name: " + i.Name + " StudentID: " + i.StudentID + " SchoolID: " + i.SchoolID + " Balance: " + i.Balance + " MedicalInfo: " + i.MedicalInfo);
                    }
                }
                else
                {
                    Trace.WriteLine("Students is null");
                }


                var responseFood = await client.GetAsync("api/FoodItems/School/" + thisSchool.Id.ToString());
                displayedFoodItems = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();
                Trace.WriteLine("response: " + responseFood); //DEBUG

                Trace.WriteLine("get food response status: " + responseFood.StatusCode); //DEBUG
                if (displayedFoodItems != null) //DEBUG
                {
                    foreach (FoodItem i in displayedFoodItems)
                    {
                        Trace.WriteLine("Name: " + i.Name + " ID: " + i.ID + " SchoolID: " + i.SchoolID + " Cost: " + i.Cost + " Description: " + i.Description);
                    }
                }
                else
                {
                    Trace.WriteLine("FoodItems is null");
                }

                /*
                var responseTransactions = await client.GetAsync("api/Transactions/School/" + thisSchool.ID);
                Trace.WriteLine(thisSchool.ID); //DEBUG
                transactions = await responseTransactions.Content.ReadAsAsync<ObservableCollection<Transaction>>();
                Trace.WriteLine("response: " + responseTransactions); //DEBUG

                Trace.WriteLine("get transactions response status: " + responseTransactions.StatusCode); //DEBUG
                if (foodItems != null) //DEBUG
                {
                    foreach (Transaction i in transactions)
                    {
                        Trace.WriteLine(i.ToString());
                    }
                }
                else
                {
                    Trace.WriteLine("Transactions is null");
                }
                */

            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check you internet connection. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tabManageFoodTypes.IsSelected)
            {
                dataGridFoodTypes.Focus();
            }
        }


        #region Manage Food Types Tab
        private void dataGridFoodTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var editableCollection = (IEditableCollectionView)dataGridFoodTypes.Items;
            var pos = editableCollection.NewItemPlaceholderPosition;
            if ((dataGridFoodTypes.SelectedItem != null) && (!dataGridFoodTypes.SelectedItem.Equals(CollectionView.NewItemPlaceholder)))
            {
                Trace.WriteLine("You selected: " + dataGridFoodTypes.SelectedItem.ToString()); //DEBUG
                btnDeleteFoodItem.IsEnabled = true;
            }
            else
            {
                btnDeleteFoodItem.IsEnabled = false;
            }

        }


        private void dataGridFoodTypes_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
        {
            Trace.WriteLine("beginning edit"); //DEBUG
        }

        private async void dataGridFoodTypes_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e) //this fires when the user quits editing a row in the GUI, i.e. not if a row is added with the helper window
        {
            int editedRowIndex = e.Row.GetIndex();
            Trace.WriteLine("index of edited row: " + editedRowIndex); //DEBUG
            //FoodItem newRowItem = (FoodItem)(e.Row.Item);
            if ((editedRowIndex > displayedFoodItems.Count - 1) || (editedRowIndex < 0))
            {
                Trace.WriteLine("item that was being added as a new row is out of range"); //DEBUG
                return;
            }

            FoodItem editedRowItem = displayedFoodItems[e.Row.GetIndex()];
            if ((string.IsNullOrWhiteSpace(editedRowItem.Name)) && (editedRowItem.Cost == 0.00M) && (string.IsNullOrWhiteSpace(editedRowItem.Description)))
            {
                //if the row has all of the default column values when editing ends, delete it
                //dataGridFoodTypes.Items.Remove(newRowItem); //don't do this, edit the observable collection's items instead
                Trace.WriteLine("removing new row with no data entered"); //DEBUG
                Trace.WriteLine("name: " + editedRowItem.Name + ", cost: " + editedRowItem.Cost + ", description: " + editedRowItem.Description);
                displayedFoodItems.Remove(editedRowItem);
                return;
            }

            if (string.IsNullOrWhiteSpace(editedRowItem.Name))
            {
                //has invalid entries, put it in validation error mode and don't add it to unsynced transactions
                MessageBox.Show("The \"Name\" field cannot be empty. Please enter a name for the food type.", "Required field", MessageBoxButton.OK, MessageBoxImage.Warning);
                //dataGridFoodTypes.CanUserAddRows = false;
                e.Row.Background = Brushes.Coral;
                Trace.WriteLine("row " + editedRowIndex + " has an invalid name field"); //DEBUG
                Trace.WriteLine("name: " + editedRowItem.Name + ", cost: " + editedRowItem.Cost + ", description: " + editedRowItem.Description); //DEBUG
                return;
            }
            if (editedRowItem.Cost < 0M)
            {
                //if the price is negative, show an error
                MessageBox.Show("The \"Price\" field cannot contain a negative value.", "Required field", MessageBoxButton.OK, MessageBoxImage.Warning);
                e.Row.Background = Brushes.Coral;
                Trace.WriteLine("row " + editedRowIndex + " has an invalid name field"); //DEBUG
                Trace.WriteLine("name: " + editedRowItem.Name + ", cost: " + editedRowItem.Cost + ", description: " + editedRowItem.Description); //DEBUG
                return;
            }

            if (!string.IsNullOrWhiteSpace(editedRowItem.ID)) //new items added by the datagrid do not have IDs, modified ones do
            {
                //modifying a datagrid row will change the corresponding foodItem
                if (e.Row.Background == Brushes.Coral) //if the item was invalid before, change the row color back to normal
                {
                    if (editedRowIndex == 0 || editedRowIndex % 2 == 0)
                    {
                        e.Row.Background = Brushes.White;
                    }
                    else
                    {
                        e.Row.Background = Brushes.LightGray;
                    }
                }
                Trace.WriteLine("food item exists, modify instead of adding"); //DEBUG
                editedRowItem.Cost = decimal.Round(editedRowItem.Cost, 2);
                string jsonString = JsonSerializer.Serialize(editedRowItem);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                HttpResponseMessage response = new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest);
                try
                {
                    loadingWindow.Show();
                    IsEnabled = false;

                    response = await client.PutAsync("api/FoodItems/" + editedRowItem.Id.ToString(), httpContent);
                    Trace.WriteLine("put request status code: " + response.StatusCode);

                    loadingWindow.Hide();
                    IsEnabled = true;
                }
                catch (HttpRequestException)
                {
                    loadingWindow.Hide();
                    IsEnabled = true;
                    MessageBox.Show("Cannot save changes because the server could not be reached, please check you internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            else
            {
                if (e.Row.Background == Brushes.Coral) //if the item was invalid before, change the row color back to normal
                {
                    if (editedRowIndex == 0 || editedRowIndex % 2 == 0)
                    {
                        e.Row.Background = Brushes.White;
                    }
                    else
                    {
                        e.Row.Background = Brushes.LightGray;
                    }
                }
                //if the food item is valid enough to reconstruct and does not already exist, add the reconstructed item to the unsyncedFoodItems collection
                FoodItem reconstructedNewFoodItem = ReconstructFoodItem(editedRowItem);
                unsyncedFoodItems.Add(reconstructedNewFoodItem);
                //replace the item in the foodItems list
                displayedFoodItems.RemoveAt(editedRowIndex);
                displayedFoodItems.Add(reconstructedNewFoodItem);
                Trace.WriteLine("reconstructed a food item-> name: " + reconstructedNewFoodItem.Name + ", cost: " + reconstructedNewFoodItem.Cost + ", description: " + reconstructedNewFoodItem.Description + ", ID: " + reconstructedNewFoodItem.ID + ", SchoolID: " + reconstructedNewFoodItem.SchoolID); //DEBUG
                await SaveUnsyncedFoodItemsAsync();
                txtNumFoodTypesShown.Text = "Showing " + displayedFoodItems.Count + " of " + displayedFoodItems.Count + " food types";
            }

        }

        /**<summary>Sends the FoodItems that have not been synced to the database.</summary>
         */
        private async Task SaveUnsyncedFoodItemsAsync()
        {
            loadingWindow.Show();
            IsEnabled = false;
            for (int i = unsyncedFoodItems.Count - 1; i >= 0; i--)
            {
                Trace.WriteLine("trying to sync a foodItem with ID: " + unsyncedFoodItems[i].Id.ToString()); //DEBUG
                Trace.WriteLine(unsyncedFoodItems[i]); //DEBUG
                string jsonString = JsonSerializer.Serialize(unsyncedFoodItems[i]);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                try
                {
                    var response = await client.PostAsync("api/FoodItems", httpContent);
                    Trace.WriteLine(jsonString); //DEBUG
                    Trace.WriteLine("sync new row response: " + response); //DEBUG
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict) //if the foodItem is in the database, remove it from the list
                    {
                        unsyncedFoodItems.RemoveAt(i);
                    }
                }
                catch (HttpRequestException) //this exception is not thrown if the URI can't be found
                {
                    Trace.WriteLine("can't reach the database");
                    MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection or try again later. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    break; //if there is a server error, no point trying to sync any more entries
                }
            }
            loadingWindow.Hide();
            IsEnabled = true;

        }

        private async void btnAddNewFoodType_Click(object sender, RoutedEventArgs e)
        {
            addNewFoodItemWindow.ShowDialog();
            //if an item was added, try to sync
            await SaveUnsyncedFoodItemsAsync();
            txtNumFoodTypesShown.Text = "Showing " + displayedFoodItems.Count + " of " + displayedFoodItems.Count + " food types";

        }

        /**<summary>Takes a food item created with the default food item constructor and makes a new one with the proper constructor.
         * The datagrid needs to use the default constructor to create a new row - this method converts it into a proper food item.</summary>
         * <remarks>Assumes the data in the old FoodItem is valid.</remarks>
         * <param name="foodItem">The FoodItem to reconstruct.</param>
         */
        private FoodItem ReconstructFoodItem(FoodItem foodItem)
        {
            FoodItem newFoodItem;
            foodItem.Cost = decimal.Round(foodItem.Cost, 2);
            if (foodItem.Description != null)
            {
                newFoodItem = new FoodItem(foodItem.Name, foodItem.Cost, foodItem.Description, thisSchool.Id.ToString());
            }
            else
            {
                newFoodItem = new FoodItem(foodItem.Name, foodItem.Cost, "", thisSchool.Id.ToString());
            }

            return newFoodItem;
        }

        private async Task DeleteSelectedFoodItems()
        {
            if (dataGridFoodTypes.SelectedItem == null || displayedFoodItems.Count == 0 || dataGridFoodTypes.SelectedItem.Equals(CollectionView.NewItemPlaceholder))
            {
                return;
            }
            else
            {
                string messageBoxMessage = "Are you sure you want to delete: ";
                var grid = dataGridFoodTypes;
                Trace.WriteLine("1"); //DEBUG
                FoodItem deletedFoodItem;
                foreach (var row in grid.SelectedItems)
                {
                    try
                    {
                        deletedFoodItem = (FoodItem)row;
                        messageBoxMessage += deletedFoodItem.Name + ", ";
                    }
                    catch (InvalidCastException)
                    {
                    }

                }
                Trace.WriteLine("2"); //DEBUG
                messageBoxMessage = messageBoxMessage.Remove(messageBoxMessage.Length - 2);
                Trace.WriteLine("3"); //DEBUG
                var result = MessageBox.Show(messageBoxMessage + "? This action cannot be undone.", "Delete items?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    for (int i = displayedFoodItems.Count - 1; i >= 0; i--)
                    {
                        Trace.WriteLine("index: " + i); //DEBUG
                        Trace.WriteLine("length of displayedFoodItems: " + displayedFoodItems.Count); //DEBUG
                        if (grid.SelectedItems.Contains(displayedFoodItems[i]))
                        {
                            if (!string.IsNullOrWhiteSpace(displayedFoodItems[i].Id.ToString()))
                            {
                                Trace.WriteLine("index: " + i); //DEBUG
                                Trace.WriteLine("length of displayedFoodItems: " + displayedFoodItems.Count); //DEBUG
                                try
                                {
                                    var response = await client.DeleteAsync("api/FoodItems/" + displayedFoodItems[i].Id.ToString());
                                    Trace.WriteLine(response.StatusCode.ToString()); //DEBUG
                                }
                                catch (HttpRequestException)
                                {
                                    Trace.WriteLine("can't reach the database");
                                    MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection or try again later. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Close();
                                    break; //if there is a server error, no point trying to sync any more entries
                                }

                                Trace.WriteLine("."); //DEBUG
                                Trace.WriteLine("index: " + i); //DEBUG
                                Trace.WriteLine("length of displayedFoodItems: " + displayedFoodItems.Count); //DEBUG
                            }

                            Trace.WriteLine(".."); //DEBUG
                            displayedFoodItems.RemoveAt(i);
                        }
                        Trace.WriteLine("..."); //DEBUG
                    }
                    Trace.WriteLine("4"); //DEBUG
                }
                else
                {
                    Trace.WriteLine("5"); //DEBUG
                }
                txtNumFoodTypesShown.Text = "Showing " + displayedFoodItems.Count + " of " + displayedFoodItems.Count + " food types";
            }
        }


        private async void btnDeleteFoodItem_Click(object sender, RoutedEventArgs e) //the delete key is set to not delete entries
        {
            Trace.WriteLine("pressed delete button"); //DEBUG
            await DeleteSelectedFoodItems();
            if (displayedFoodItems != null)
            {
                foreach (FoodItem i in displayedFoodItems) //DEBUG
                {
                    Trace.WriteLine("Remaining foodItem in list ->  Name: " + i.Name + " ID: " + i.ID + " SchoolID: " + i.SchoolID + " Cost: " + i.Cost + " Description: " + i.Description);
                }
            }
        }

        private async void dataGridFoodTypes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true; //stops the default behavior of the delete key so that DeleteSelectedFoodItems can handle it
                Trace.WriteLine("pressed delete key"); //DEBUG
                await DeleteSelectedFoodItems();
                if (displayedFoodItems != null)
                {
                    foreach (FoodItem i in displayedFoodItems) //DEBUG
                    {
                        Trace.WriteLine("Remaining foodItem in list ->  Name: " + i.Name + " ID: " + i.ID + " SchoolID: " + i.SchoolID + " Cost: " + i.Cost + " Description: " + i.Description);
                    }
                }

            }
        }

        private async void txtFoodSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (((e.Key == Key.Return) || (e.Key == Key.Enter)) && txtFoodSearch.IsKeyboardFocused)
            {
                loadingWindow.SetMessage("Searching...");
                loadingWindow.Show();
                IsEnabled = false;

                var allFoodItems = await GetFoodItemsAsync();

                displayedFoodItems = new ObservableCollection<FoodItem>();
                foreach (FoodItem i in allFoodItems)
                {
                    if (i.Name.Contains(txtFoodSearch.Text, StringComparison.OrdinalIgnoreCase))
                    {
                        displayedFoodItems.Add(i);
                    }
                }
                dataGridFoodTypes.ItemsSource = null;
                dataGridFoodTypes.ItemsSource = displayedFoodItems;

                txtNumFoodTypesShown.Text = "Showing " + displayedFoodItems.Count + " of " + allFoodItems.Count + " food types";

                loadingWindow.Hide();
                IsEnabled = true;
                loadingWindow.SetMessage(LoadingBox.defaultMessage);
            }

        }


        private void txtFoodSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtFoodSearch.Text.Equals("Search"))
            {
                txtFoodSearch.Text = "";
                txtFoodSearch.Foreground = Brushes.Black;
            }
        }

        private void txtFoodSearch_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFoodSearch.Text))
            {
                txtFoodSearch.Text = "Search";
                txtFoodSearch.Foreground = Brushes.DarkGray;
            }
        }

        private async void btnClearSearchFoodItems_Click(object sender, RoutedEventArgs e)
        {
            await RefreshFoodTypesDataGrid();

            txtFoodSearch.Text = "Search";
            txtFoodSearch.Foreground = Brushes.DarkGray;

        }

        private async void btnRefreshFoodItems_Click(object sender, RoutedEventArgs e)
        {

            loadingWindow.Show();
            IsEnabled = false;

            await RefreshFoodTypesDataGrid();

            txtFoodSearch.Text = "Search";
            txtFoodSearch.Foreground = Brushes.DarkGray;

            loadingWindow.Hide();
            IsEnabled = true;

        }

        private async Task RefreshFoodTypesDataGrid()
        {
            displayedFoodItems = await GetFoodItemsAsync();

            dataGridFoodTypes.ItemsSource = null;
            dataGridFoodTypes.ItemsSource = displayedFoodItems;

            txtNumFoodTypesShown.Text = "Showing " + displayedFoodItems.Count + " of "+ displayedFoodItems.Count + " food types";
        }
        #endregion Manage Food Types Tab


        #region Manage Students Tab
        private void txtStudentsSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (((e.Key == Key.Return) || (e.Key == Key.Enter)) && txtStudentsSearch.IsKeyboardFocused)
            {
                loadingWindow.SetMessage("Searching...");
                loadingWindow.Show();
                IsEnabled = false;
                if (int.TryParse(txtStudentsSearch.Text, out int num)) //if the text entered is a number, search by student number
                {
                    displayedStudents = new ObservableCollection<Student>();
                    foreach (Student i in students)
                    {
                        if (i.StudentID.Contains(txtStudentsSearch.Text))
                        {
                            displayedStudents.Add(i);
                        }
                    }
                }
                else //if the text cannot be parsed as an int, search by name
                {
                    displayedStudents = new ObservableCollection<Student>();
                    foreach (Student i in students)
                    {
                        if (i.Name.Contains(txtStudentsSearch.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            displayedStudents.Add(i);
                        }
                    }
                }
                dataGridStudents.ItemsSource = null;
                dataGridStudents.ItemsSource = displayedStudents;
                txtNumStudentsShown.Text = "Showing " + displayedStudents.Count + " of " + students.Count + " students";

                loadingWindow.Hide();
                IsEnabled = true;
                loadingWindow.SetMessage(LoadingBox.defaultMessage);
            }

        }

        private void btnShowAllStudents_Click(object sender, RoutedEventArgs e)
        {
            loadingWindow.Show();
            IsEnabled = false;

            displayedStudents = students;

            dataGridStudents.ItemsSource = null;
            dataGridStudents.ItemsSource = displayedStudents;
            txtNumStudentsShown.Text = "Showing " + displayedStudents.Count + " of " + students.Count + " students";
            txtStudentsSearch.Text = "Search";
            txtStudentsSearch.Foreground = Brushes.DarkGray;

            loadingWindow.Hide();
            IsEnabled = true;
        }

        private void btnShowOwingStudents_Click(object sender, RoutedEventArgs e)
        {
            loadingWindow.SetMessage("Searching...");
            loadingWindow.Show();
            IsEnabled = false;

            displayedStudents = new ObservableCollection<Student>();
            foreach (Student i in students)
            {
                if (i.Balance < 0)
                {
                    displayedStudents.Add(i);
                }
            }

            dataGridStudents.ItemsSource = null;
            dataGridStudents.ItemsSource = displayedStudents;
            txtNumStudentsShown.Text = "Showing " + displayedStudents.Count + " of " + students.Count + " students";

            loadingWindow.Hide();
            IsEnabled = true;
            loadingWindow.SetMessage(LoadingBox.defaultMessage);
        }

        private void txtStudentsSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtStudentsSearch.Text.Equals("Search"))
            {
                txtStudentsSearch.Text = "";
                txtStudentsSearch.Foreground = Brushes.Black;
            }

        }

        private void txtStudentsSearch_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtStudentsSearch.Text))
            {
                txtStudentsSearch.Text = "Search";
                txtStudentsSearch.Foreground = Brushes.DarkGray;
            }
        }

        private async void btnAddToStudentBalance_Click(object sender, RoutedEventArgs e)
        {
            if ((dataGridStudents.SelectedIndex > displayedStudents.Count - 1) || (dataGridStudents.SelectedIndex < 0))
            {
                Trace.WriteLine("no student selected"); //DEBUG
                MessageBox.Show("Please select a student in the table first.", "No student selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            loadingWindow.Show();
            IsEnabled = false;

            decimal amount;
            if ((!Decimal.TryParse(txtAddRemoveStudentBalanceAmount.Text, out amount)) || (amount <= 0M))
            {
                txtStudentBalanceAddRemoveError.Visibility = Visibility.Visible;
            }
            else
            {
                txtStudentBalanceAddRemoveError.Visibility = Visibility.Hidden;
                //displayedStudents[dataGridStudents.SelectedIndex].Balance += decimal.Round(amount, 2);
                //await SaveModifiedStudentAsync(displayedStudents[dataGridStudents.SelectedIndex]);
                await SendNewTransactionAsync((amount * -1), displayedStudents[dataGridStudents.SelectedIndex]); //cost is negative to add to balance

                await RefreshStudentsDataGrid();
            }
            txtAddRemoveStudentBalanceAmount.Text = "";
            loadingWindow.Hide();
            IsEnabled = true;
        }

        private async void btnRemoveFromStudentBalance_Click(object sender, RoutedEventArgs e)
        {
            if ((dataGridStudents.SelectedIndex > displayedStudents.Count - 1) || (dataGridStudents.SelectedIndex < 0))
            {
                Trace.WriteLine("no student selected"); //DEBUG
                MessageBox.Show("Please select a student in the table first.", "No student selected", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            loadingWindow.Show();
            IsEnabled = false;

            decimal amount;
            if ((!Decimal.TryParse(txtAddRemoveStudentBalanceAmount.Text, out amount)) || (amount <= 0M))
            {
                txtStudentBalanceAddRemoveError.Visibility = Visibility.Visible;
            }
            else
            {
                txtStudentBalanceAddRemoveError.Visibility = Visibility.Hidden;
                //displayedStudents[dataGridStudents.SelectedIndex].Balance -= decimal.Round(amount, 2);
                //await SaveModifiedStudentAsync(displayedStudents[dataGridStudents.SelectedIndex]);
                await SendNewTransactionAsync(amount, displayedStudents[dataGridStudents.SelectedIndex]);

                await RefreshStudentsDataGrid();
            }
            txtAddRemoveStudentBalanceAmount.Text = "";
            loadingWindow.Hide();
            IsEnabled = true;
        }

        /**<summary>Creates and sends a new transaction to the database.</summary>
         * <param name="student">The student the transaction belongs to. The student must exist in the database.</param>
         * <param name="cost">The amount to remove from the student's balance. Negative to add to a student's balance.</param>
         */
        private async Task SendNewTransactionAsync(decimal cost, Student student)
        {
            string foodName = "";
            if (cost <= 0)
            {
                foodName = "Added to balance";
            }
            else
            {
                foodName = "Removed from balance";
            }
            Transaction newTransaction = new Transaction(student.StudentID, "0", foodName, cost, student.Name, thisSchool.ID, thisSchool.Name);

            Trace.WriteLine("trying to send a tranaction with ID: " + newTransaction.ID); //DEBUG
            string jsonString = JsonSerializer.Serialize(newTransaction);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PostAsync("api/Transactions", httpContent);
                Trace.WriteLine(jsonString); //DEBUG
                Trace.WriteLine("sync new row response: " + response); //DEBUG
                if (!response.IsSuccessStatusCode && !(response.StatusCode == System.Net.HttpStatusCode.Conflict))
                {
                    Trace.WriteLine("can't reach the database");
                    MessageBox.Show("Cannot connect to the server, your last change will not be saved. Please check your internet connection or try again later. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (HttpRequestException) //this exception is not thrown if the URI can't be found
            {
                Trace.WriteLine("can't reach the database");
                MessageBox.Show("Cannot connect to the server, your last change will not be saved. Please check your internet connection or try again later. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

        }

        /**<summary>Saves changes made to a student.</summary>
         * <param name="student">The modified student.</param>
         */
        private async Task SaveModifiedStudentAsync(Student student)
        {
            Trace.WriteLine("trying to sync a student with ID: " + student.StudentID); //DEBUG
            string jsonString = JsonSerializer.Serialize(student);
            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
            try
            {
                var response = await client.PutAsync("api/Students/" + student.StudentID, httpContent);
                Trace.WriteLine(jsonString); //DEBUG
                Trace.WriteLine("sync new row response: " + response); //DEBUG
                if (!response.IsSuccessStatusCode && !(response.StatusCode == System.Net.HttpStatusCode.Conflict))
                {
                    Trace.WriteLine("can't reach the database");
                    MessageBox.Show("Cannot connect to the server, your last change will not be saved. Please check your internet connection or try again later. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                }
            }
            catch (HttpRequestException) //this exception is not thrown if the URI can't be found
            {
                Trace.WriteLine("can't reach the database");
                MessageBox.Show("Cannot connect to the server, your last change will not be saved. Please check your internet connection or try again later. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private async void btnRefreshStudents_Click(object sender, RoutedEventArgs e)
        {
            loadingWindow.Show();
            IsEnabled = false;

            await RefreshStudentsDataGrid();
            txtStudentsSearch.Text = "Search";
            txtStudentsSearch.Foreground = Brushes.DarkGray;

            loadingWindow.Hide();
            IsEnabled = true;
        }

        /**
         * <summary>Refreshes the datagrid displaying students.</summary>
         */
        private async Task RefreshStudentsDataGrid()
        {
            students = await GetStudentsAsync();
            displayedStudents = students;

            dataGridStudents.ItemsSource = null;
            dataGridStudents.ItemsSource = displayedStudents;

            txtNumStudentsShown.Text = "Showing " + displayedStudents.Count + " of " + students.Count + " students";
        }


        private void btnExportStudents_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV (*.csv)|*.csv";
            saveDialog.FileName = "Students-" + DateTime.Now.ToString("MM-dd-yyyy");
            var result = saveDialog.ShowDialog();

            if (result == true) //save button is pressed
            {
                var exportWhatResponse = MessageBox.Show("Would you like to export all student data? Selecting \"No\" will only export data shown in the table.", "Export", MessageBoxButton.YesNoCancel);
                if (exportWhatResponse == MessageBoxResult.Cancel)
                {
                    return;
                }
                loadingWindow.SetMessage("Exporting data...");
                loadingWindow.Show();
                IsEnabled = false;
                if (File.Exists(saveDialog.FileName))
                {
                    try
                    {
                        File.Delete(saveDialog.FileName);
                    }
                    catch (IOException)
                    {
                        loadingWindow.Hide();
                        loadingWindow.SetMessage(LoadingBox.defaultMessage);
                        IsEnabled = true;
                        MessageBox.Show("Student data could not be exported, choose a different file location and try again.", "Could not export", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                using (var writer = new StreamWriter(saveDialog.FileName))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        try
                        {
                            if (exportWhatResponse == MessageBoxResult.Yes)
                            {
                                csv.WriteRecords(students);
                            }
                            else if (exportWhatResponse == MessageBoxResult.No)
                            {
                                csv.WriteRecords(displayedStudents);
                            }

                        }
                        catch (IOException)
                        {
                            MessageBox.Show("Student data could not be exported, choose a different file location and try again.", "Could not export", MessageBoxButton.OK, MessageBoxImage.Error);
                        }

                    }
                }
                loadingWindow.Hide();
                loadingWindow.SetMessage(LoadingBox.defaultMessage);
                IsEnabled = true;
                MessageBox.Show("Successfully exported student data.", "Export successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }


        }


        #endregion Manage Students Tab


        #region View Transactions Tab

        private async void btnRefreshTransactions_Click(object sender, RoutedEventArgs e)
        {
            loadingWindow.Show();
            IsEnabled = false;

            await RefreshTransactionsDataGrid();
            txtTransactionsSearch.Text = "Search";
            txtTransactionsSearch.Foreground = Brushes.DarkGray;

            loadingWindow.Hide();
            IsEnabled = true;
        }

        /**<summary>Refreshes the transactions data grid.</summary>
         */
        private async Task RefreshTransactionsDataGrid()
        {
            DateTime startDate = dpTransactionStart.SelectedDate ?? DateTime.Now;
            DateTime endDate = dpTransactionEnd.SelectedDate ?? DateTime.Now;
            if (!(startDate > endDate) && (dpTransactionStart.SelectedDate != null) && (dpTransactionEnd.SelectedDate != null))
            {
                transactions = await GetTransactionsBetweenAsync(startDate, endDate, 5000);
            }
            else
            {
                transactions = await GetTransactionsAsync((int)cbNumTransactionsToShow.SelectedItem);
            }

            dataGridTransactions.ItemsSource = null;
            dataGridTransactions.ItemsSource = transactions;

            txtNumTransactionsShown.Text = "Showing " + transactions.Count + " transactions";
        }

        private async void txtTransactionsSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (((e.Key == Key.Return) || (e.Key == Key.Enter)) && txtTransactionsSearch.IsKeyboardFocused)
            {
                loadingWindow.SetMessage("Searching...");
                loadingWindow.Show();
                IsEnabled = false;

                await RefreshTransactionsDataGrid();

                var searchedTransactions = new ObservableCollection<Transaction>();
                if (int.TryParse(txtTransactionsSearch.Text, out int num)) //if the text entered is a number, search by student number
                {
                    foreach (Transaction i in transactions)
                    {
                        if (i.StudentID.Contains(txtTransactionsSearch.Text))
                        {
                            searchedTransactions.Add(i);
                        }
                    }
                }
                else //if the text cannot be parsed as an int, search by name
                {
                    foreach (Transaction i in transactions)
                    {
                        if (i.StudentName.Contains(txtTransactionsSearch.Text, StringComparison.OrdinalIgnoreCase))
                        {
                            searchedTransactions.Add(i);
                        }
                    }
                }
                transactions = searchedTransactions;
                dataGridTransactions.ItemsSource = null;
                dataGridTransactions.ItemsSource = transactions;
                txtNumTransactionsShown.Text = "Showing " + transactions.Count + " transactions";

                loadingWindow.Hide();
                IsEnabled = true;
                loadingWindow.SetMessage(LoadingBox.defaultMessage);

                if (transactions.Count == 0)
                {
                    MessageBox.Show("No transactions were found in the table.", "No transactions found", MessageBoxButton.OK, MessageBoxImage.Information);

                }
            }
        }

        private void txtTransactionsSearch_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (txtTransactionsSearch.Text.Equals("Search"))
            {
                txtTransactionsSearch.Text = "";
                txtTransactionsSearch.Foreground = Brushes.Black;
            }
        }

        private void txtTransactionsSearch_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTransactionsSearch.Text))
            {
                txtTransactionsSearch.Text = "Search";
                txtTransactionsSearch.Foreground = Brushes.DarkGray;
            }
        }

        private async void btnClearTransactionsSearch_Click(object sender, RoutedEventArgs e)
        {
            loadingWindow.Show();
            IsEnabled = false;

            await RefreshTransactionsDataGrid();
            txtTransactionsSearch.Text = "Search";
            txtTransactionsSearch.Foreground = Brushes.DarkGray;

            loadingWindow.Hide();
            IsEnabled = true;
        }

        private async void cbNumTransactionsToShow_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            loadingWindow.Show();
            IsEnabled = false;

            //transactions = await GetTransactionsAsync((int)cbNumTransactionsToShow.SelectedItem);
            dpTransactionStart.SelectedDate = null;
            dpTransactionEnd.SelectedDate = null;
            txtStartEndDateError.Visibility = Visibility.Hidden;

            await RefreshTransactionsDataGrid();

            loadingWindow.Hide();
            IsEnabled = true;
        }

        /**<summary>Validates the transaction date range and loads the requested transactions if the range is valid.</summary>
         */
        private async void btnEnterTransactionRange_Click(object sender, RoutedEventArgs e)
        {
            DateTime startDate = dpTransactionStart.SelectedDate ?? DateTime.Now;
            DateTime endDate = dpTransactionEnd.SelectedDate ?? DateTime.Now;
            int maxResults = 5000; //in order to prevent lag, there is a maximum number of transactions that will be loaded from the database and stored in the program at a time
            if (startDate > endDate)
            {
                txtStartEndDateError.Text = "The start date cannot be later than the end date";
                txtStartEndDateError.Visibility = Visibility.Visible;
                return;
            }
            txtStartEndDateError.Visibility = Visibility.Hidden;
            //if the start date is not null, the input is valid. If the end date was not entered it will use today's date
            if (dpTransactionStart.SelectedDate != null)
            {
                loadingWindow.Show();
                IsEnabled = false;

                transactions = await GetTransactionsBetweenAsync(startDate, endDate, maxResults);
                dataGridTransactions.ItemsSource = null;
                dataGridTransactions.ItemsSource = transactions;
                txtNumTransactionsShown.Text = "Showing " + transactions.Count + " transactions";

                loadingWindow.Hide();
                this.IsEnabled = true;

                if (transactions.Count == maxResults)
                {
                    MessageBox.Show("The number of transactions between the two dates is greater than " + maxResults +  ". Only the most recent " + maxResults + " transactions will be shown. Enter dates within a smaller time period to see all results in that time period.", "Showing limited results", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            else
            {
                txtStartEndDateError.Text = "Please enter a start date";
                txtStartEndDateError.Visibility = Visibility.Visible;
            }


        }

        /// <summary>Gets the transactions that took place in a certain date range up to a maximum number to be loaded.</summary>
        /// <param name="startDate">The start of the range of dates. Transactions taking place on this day are included in the results.</param>
        /// <param name="endDate">The end of the range of dates. Transactions taking place on this day are excluded from the results.</param>
        /// <param name="max">The maximum number of transactions to load.</param>
        /// <returns>The transactions within the two dates.</returns>

        private async Task<ObservableCollection<Transaction>> GetTransactionsBetweenAsync(DateTime startDate, DateTime endDate, int max)
        {
            ObservableCollection<Transaction> newTransactionsCollection = new();
            try
            {
                Trace.WriteLine("start: " + startDate); //DEBUG
                Trace.WriteLine("end: " + endDate); //DEBUG
                //var responseTransactions = await client.GetAsync("api/Transactions/Between/" + thisSchool.ID + "/" + startDate + "/" + endDate);
                //var responseTransactions = await client.GetAsync("api/Transactions/Between/1/2021-07-05T14%3A50%3A38.301Z/2021-08-05T14%3A50%3A38.301Z"); //DEBUG
                var responseTransactions = await client.GetAsync("api/Transactions/Between/" + thisSchool.Id.ToString() + "/" + startDate.ToString("yyyyMMdd") + "/" + endDate.ToString("yyyyMMdd") + "/" + max);
                newTransactionsCollection = await responseTransactions.Content.ReadAsAsync<ObservableCollection<Transaction>>();
                if (newTransactionsCollection == null) //DEBUG
                {
                    Trace.WriteLine("newTransactionsCollection is null - GetTransactionsBetweenAsync");
                    Trace.WriteLine(responseTransactions.StatusCode);
                }
                if (responseTransactions.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    return transactions;
                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            return newTransactionsCollection;
        }

        /**<summary>Exports the transactions shown in the datagrid as a CSV. Does not give an option to export
         * all transactions.</summary>
         */
        private void btnExportDisplayedTransactions_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV (*.csv)|*.csv";
            saveDialog.FileName = "Transactions-" + DateTime.Now.ToString("MM-dd-yyyy");
            var result = saveDialog.ShowDialog();

            if (result == true) //save button is pressed
            {
                loadingWindow.SetMessage("Exporting data...");
                loadingWindow.Show();
                IsEnabled = false;
                if (File.Exists(saveDialog.FileName))
                {
                    try
                    {
                        File.Delete(saveDialog.FileName);
                    }
                    catch (IOException)
                    {
                        loadingWindow.Hide();
                        loadingWindow.SetMessage(LoadingBox.defaultMessage);
                        IsEnabled = true;
                        MessageBox.Show("Transaction data could not be exported, choose a different file location and try again.", "Could not export", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                using (var writer = new StreamWriter(saveDialog.FileName))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(transactions);
                    }
                }
                loadingWindow.Hide();
                loadingWindow.SetMessage(LoadingBox.defaultMessage);
                IsEnabled = true;
                MessageBox.Show("Successfully exported transaction data.", "Export successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        #endregion View Transactions Tab
    }
}
