using Data.Models;
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

namespace LunchManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient client; //the application shares one http client
        private ObservableCollection<Transaction> _transactions;
        private ObservableCollection<Student> _students;
        private static ObservableCollection<FoodItem> _foodItems;
        private static ObservableCollection<FoodItem> _unsyncedFoodItems; 
        private School thisSchool;
        private Window loadingWindow;
        private AddNewFoodItemHelper addNewFoodItemWindow;
        private List<FoodItem> lastDeletedFoodItems;
        //private string foodItemsJsonPath = "unsyncedFoodItems.json";

        public static ObservableCollection<FoodItem> foodItems { get { return _foodItems; } set { _foodItems = value; } }
        public static ObservableCollection<FoodItem> unsyncedFoodItems { get { return _unsyncedFoodItems; } set { _unsyncedFoodItems = value; } }
        public ObservableCollection<Transaction> transactions { get { return _transactions; } set { _transactions = value; } }
        public ObservableCollection<Student> students { get { return _students; } set { _students = value; } }

        public MainWindow()
        {
            InitializeComponent();

            Application.Current.MainWindow = this;

            transactions = new();
            students = new();
            foodItems = new();
            unsyncedFoodItems = new();
            lastDeletedFoodItems = new();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
            loadingWindow.Show();
            IsEnabled = false;

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

            await GetDataAsync();

            //set up data binding
            //dataGridFoodTypes.DataContext = foodItems;
            dataGridFoodTypes.ItemsSource = foodItems;

            //dataGridFoodTypes.DataContext = this;
            btnDeleteFoodItem.IsEnabled = false;

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

        private async Task<ObservableCollection<FoodItem>> GetFoodItemsAsync()
        {
            //HttpResponseMessage responseFood;
            ObservableCollection<FoodItem> newFoodItemsCollection = new();
            try
            {
                //responseFood = await client.GetAsync("api/FoodItem/School/" + thisSchool.ID);
                //newFoodItemsCollection = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>(); //is null
                var responseFood = await client.GetAsync("api/FoodItems/School/" + thisSchool.ID);
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

        /**<summary>Loads the students, schools, and food items into the main window's lists.</summary>
         */
        private async Task GetDataAsync()
        {
            
            try
            {
                /*
                var responseStudents = await client.GetAsync("api/Students/School/" + thisSchool.ID);
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
                */

                var responseFood = await client.GetAsync("api/FoodItems/School/" + thisSchool.ID);
                foodItems = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();
                Trace.WriteLine("response: " + responseFood); //DEBUG

                Trace.WriteLine("get food response status: " + responseFood.StatusCode); //DEBUG
                if (foodItems != null) //DEBUG
                {
                    foreach (FoodItem i in foodItems)
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

        private void btnEditFoodItem_Click(object sender, RoutedEventArgs e)
        {
            
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
            if (editedRowIndex > foodItems.Count - 1 && editedRowIndex > 0)
            {
                Trace.WriteLine("item that was being added as a new row is out of range"); //DEBUG
                return;
            }
            
            FoodItem editedRowItem = foodItems[e.Row.GetIndex()];
            if ((string.IsNullOrWhiteSpace(editedRowItem.Name)) && (editedRowItem.Cost == 0.00M) && (string.IsNullOrWhiteSpace(editedRowItem.Description)))
            {
                //if the row has all of the default column values when editing ends, delete it
                //dataGridFoodTypes.Items.Remove(newRowItem); //don't do this, edit the observable collection's items instead
                Trace.WriteLine("removing new row with no data entered"); //DEBUG
                Trace.WriteLine("name: " + editedRowItem.Name + ", cost: " + editedRowItem.Cost + ", description: " + editedRowItem.Description);
                foodItems.Remove(editedRowItem);
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
                string jsonString = JsonSerializer.Serialize(editedRowItem);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await client.PutAsync("api/FoodItems/" + editedRowItem.ID, httpContent);
                Trace.WriteLine("put request status code: " + response.StatusCode);
                if (!response.IsSuccessStatusCode)
                {
                    foodItems.Remove(editedRowItem);
                    MessageBox.Show("Cannot save changes because the server could not be reached, please check you internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
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
                foodItems.RemoveAt(editedRowIndex);
                foodItems.Add(reconstructedNewFoodItem);
                Trace.WriteLine("reconstructed a food item-> name: " + reconstructedNewFoodItem.Name + ", cost: " + reconstructedNewFoodItem.Cost + ", description: " + reconstructedNewFoodItem.Description + ", ID: " + reconstructedNewFoodItem.ID + ", SchoolID: " + reconstructedNewFoodItem.SchoolID); //DEBUG
                await SaveUnsyncedFoodItemsAsync();
            }
            
        }

        private async Task SaveUnsyncedFoodItemsAsync()
        {
            loadingWindow.Show();
            IsEnabled = false;
            for (int i = unsyncedFoodItems.Count - 1; i >= 0; i--)
            {
                Trace.WriteLine("trying to sync a foodItem with ID: " + unsyncedFoodItems[i].ID); //DEBUG
                Trace.WriteLine(unsyncedFoodItems[i]); //DEBUG
                string jsonString = JsonSerializer.Serialize(unsyncedFoodItems[i]);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                try
                {
                    var response = await client.PostAsync("api/FoodItems", httpContent);
                    Trace.WriteLine(jsonString); //DEBUG
                    Trace.WriteLine("sync new row response: " + response); //DEBUG
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict) //if the foodItems is in the database, remove it from the list
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

        private void dataGridFoodTypes_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            /*
            if (string.Equals(e.Column.Header.ToString(), "Name"))
            {
                if (string.IsNullOrEmpty(((FoodItem)dataGridFoodTypes.SelectedItem).Name))
                {

                }
            }
            */
        }

        private async void btnAddNewFoodType_Click(object sender, RoutedEventArgs e)
        {
            addNewFoodItemWindow.ShowDialog();
            //if an item was added, try to sync
            await SaveUnsyncedFoodItemsAsync(); 

        }

        private void dataGridFoodTypes_AddingNewItem(object sender, AddingNewItemEventArgs e)
        {
            
        }

        /**<summary>Takes a food item created with the default food item constructor and makes a new one with the proper constructor.
         * The datagrid needs to use the default constructor to create a new row - this method converts it into a proper food item.</summary>
         * <remarks>Assumes the data</remarks>
         */
        private FoodItem ReconstructFoodItem(FoodItem foodItem)
        {
            FoodItem newFoodItem;
            foodItem.Cost = decimal.Round(foodItem.Cost, 2);
            if (foodItem.Description != null)
            {
                newFoodItem = new FoodItem(foodItem.Name, foodItem.Cost, foodItem.Description, thisSchool.ID);
            }
            else
            {
                newFoodItem = new FoodItem(foodItem.Name, foodItem.Cost, "", thisSchool.ID);
            }
            
            return newFoodItem;
        }

        private async Task DeleteSelectedFoodItems()
        {
            if (dataGridFoodTypes.SelectedItem == null || foodItems.Count == 0 || dataGridFoodTypes.SelectedItem.Equals(CollectionView.NewItemPlaceholder))
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
                var result = MessageBox.Show(messageBoxMessage + "?", "Delete items?", MessageBoxButton.YesNo);
                if (result == MessageBoxResult.Yes)
                {
                    for (int i = foodItems.Count - 1; i >= 0; i--)
                    {
                        Trace.WriteLine("index: " + i); //DEBUG
                        Trace.WriteLine("length of foodItems: " + foodItems.Count); //DEBUG
                        if (grid.SelectedItems.Contains(foodItems[i]))
                        {
                            if (!string.IsNullOrWhiteSpace(foodItems[i].ID)) 
                            {
                                Trace.WriteLine("index: " + i); //DEBUG
                                Trace.WriteLine("length of foodItems: " + foodItems.Count); //DEBUG
                                var response = await client.DeleteAsync("api/FoodItems/" + foodItems[i].ID);
                                Trace.WriteLine(response.StatusCode.ToString()); //DEBUG
                                Trace.WriteLine("."); //DEBUG
                                Trace.WriteLine("index: " + i); //DEBUG
                                Trace.WriteLine("length of foodItems: " + foodItems.Count); //DEBUG
                            }
                            
                            //lastDeletedFoodItems.Add(foodItems[i]); //fails here
                            Trace.WriteLine(".."); //DEBUG
                            foodItems.RemoveAt(i);
                        }
                        Trace.WriteLine("..."); //DEBUG
                    }
                    Trace.WriteLine("4"); //DEBUG
                }
                else
                { 
                    Trace.WriteLine("5"); //DEBUG
                }

            }
        }

        private async void btnDeleteFoodItem_Click(object sender, RoutedEventArgs e) //the delete key is set to not delete entries
        {
            Trace.WriteLine("pressed delete button"); //DEBUG
            await DeleteSelectedFoodItems();
            if (foodItems != null)
            {
                foreach (FoodItem i in foodItems)
                {
                    Trace.WriteLine("Remaining foodItem in list ->  Name: " + i.Name + " ID: " + i.ID + " SchoolID: " + i.SchoolID + " Cost: " + i.Cost + " Description: " + i.Description);
                }
            }
        }

        private async void dataGridFoodTypes_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true;
                Trace.WriteLine("pressed delete key"); //DEBUG
                await DeleteSelectedFoodItems();
                if (foodItems != null)
                {
                    foreach (FoodItem i in foodItems)
                    {
                        Trace.WriteLine("Remaining foodItem in list ->  Name: " + i.Name + " ID: " + i.ID + " SchoolID: " + i.SchoolID + " Cost: " + i.Cost + " Description: " + i.Description);
                    }
                }
                
            }
        }

        private async void btnRefreshFoodItems_Click(object sender, RoutedEventArgs e)
        {
            
            loadingWindow.Show();
            IsEnabled = false;

            foodItems = await GetFoodItemsAsync();

            dataGridFoodTypes.ItemsSource = null;
            dataGridFoodTypes.ItemsSource = foodItems;

            loadingWindow.Hide();
            IsEnabled = true;

            
        }

        #endregion


    }
}
