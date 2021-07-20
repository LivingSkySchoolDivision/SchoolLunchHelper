using System;
using System.Collections.Generic;
using System.Linq;
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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Data;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.IO;
using Data.Models;
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Timers;
using System.Configuration;

namespace CardScannerUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private ObservableCollection<FoodItem> foodItems;
        private ObservableCollection<Transaction> guiTransactions;
        private ObservableCollection<Transaction> unsyncedTransactions; //this gets the transactions from the JSON that did not sync, don't want them on the GUI so keep them separate
        private List<Student> students;
        private HttpClient client; //the application shares one http client
        private String apiUri;
        private Window loadingWindow;

        private Transaction lastTransaction;
        private string transactionsJsonPath = "TransactionsLog.json";

        private Stopwatch lastTransactionStopwatch; //keeps track of time since the last transaction
        private bool shutdown = false;

        private static School _ThisSchool;
        public static School ThisSchool { get { return _ThisSchool; } set { _ThisSchool = value; } }


        public MainWindow()
        {
            InitializeComponent();

            foodItems = new();
            guiTransactions = new();
            students = new();
            unsyncedTransactions = new();
            lastTransactionStopwatch = new();
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
            loadingWindow.Show();
            IsEnabled = false;

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            apiUri = configFile.AppSettings.Settings["apiUri"].Value.ToString();
            string thisSchoolID = configFile.AppSettings.Settings["thisSchool"].Value;
            //MessageBox.Show(apiUri); //DEBUG
            //Trace.WriteLine("ApiUri: " + apiUri); //DEBUG
            //Trace.WriteLine("schoolID: " + thisSchoolID); //DEBUG

            ApiHelper.Init(apiUri); //initializes settings for the HttpClient
            client = ApiHelper.ApiClient; //gets the newly initialized HttpClient

            //await AddTestDataToDatabase(); //DEBUG
            //await ClearDbTransactionsAsync(); //DEBUG
            //throw new Exception(); //DEBUG - end the program when the transactions have been cleared

            //LoadTransactionsFromJson();
            await LoadTransactionsFromJsonAsync();

            await GetThisSchoolFromIdAsync(thisSchoolID);
            //Trace.WriteLine("school name: " + ThisSchool.Name + " school ID: " + ThisSchool.ID); //DEBUG

            //if the request to get the school object fails, the program is closed, but this code will continue to run after
            //it's been closed. This checks if the school is null, and if it is, the method is done and no more code will 
            //run and cause exceptions.
            if (ThisSchool != null)
            {
                this.Title = "Lunch Helper - " + ThisSchool.Name; 

                await GetDataAsync();
                Trace.WriteLine("2"); //DEBUG

                //set up the data binding
                dataGridTransactions.DataContext = guiTransactions;
                dataGridTransactions.ItemsSource = guiTransactions;
                dataGridFoodItems.DataContext = foodItems;
                dataGridFoodItems.ItemsSource = foodItems;
                txtUnsyncedTransactionsCount.DataContext = unsyncedTransactions.Count.ToString();

                //makes the datagrid automatically scroll to the newest transaction if the scroll bar is visible
                guiTransactions.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(delegate (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
                {
                    if (guiTransactions.Count > 0)
                    {
                        dataGridTransactions.ScrollIntoView(guiTransactions[guiTransactions.Count - 1]);
                    }
                });

                //txtEnterStudentNum.IsEnabled = true;
                //buttonSync.IsEnabled = true;
                Trace.WriteLine("3"); //DEBUG
                loadingWindow.Hide();
                Trace.WriteLine("4"); //DEBUG
                IsEnabled = true;
                Trace.WriteLine("5"); //DEBUG
            }
            
        }


        /**<summary>Loads the school object from the database given the school's ID.</summary>
         * <param name="thisSchoolID">string, the ID of the requested school.</param>
         */
        private async Task GetThisSchoolFromIdAsync(string thisSchoolID) 
        {
            try
            {
                var response = await client.GetAsync("api/Schools/" + thisSchoolID);
                ThisSchool = await response.Content.ReadAsAsync<School>();
                Trace.WriteLine("get this school from ID response status: " + response.StatusCode); //DEBUG
            }
            catch (HttpRequestException)
            {
                shutdown = true;
                MessageBox.Show("Failed to connect to the database, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            
        }


        private async void Window_Closing(object sender, CancelEventArgs e)
        { 
            if (unsyncedTransactions.Count != 0 && !shutdown) //if there are unsynced transactions and the program should not be closed without syncing
            {
                e.Cancel = true; //don't close the program yet
                MessageBoxResult result =
                  MessageBox.Show(
                    "Send all transactions and close the program?",
                    "Lunch Helper",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    return;
                }
                else
                {
                    await SyncAllTransactionsAsync();
                    //
                    e.Cancel = false; //tells the program it is allowed to close - the program doesn't close though
                    shutdown = true; //setting shutdown to true will force the window to close when Close() is called again (this is necessary in the case the database can't be reached)
                    ApiHelper.ApiClient.Dispose();
                    this.Close();
                    return; //end this instance of the method after the recursive call
                }
                
            }
            Trace.WriteLine("Closing the program..."); //DEBUG
        }

        /**<summary>Deletes all transactions from the database, student balances are updated automatically. 
         * This method is for testing purposes.</summary>
         */
        private async Task ClearDbTransactionsAsync() //DEBUG
        {
            var responseTransactions = await client.GetAsync("api/Transactions");
            var transactions = await responseTransactions.Content.ReadAsAsync<List<Transaction>>();
            Trace.WriteLine("get transactions response status: " + responseTransactions.StatusCode); 
            foreach (Transaction i in transactions)
            {
                var responseDelete = await client.DeleteAsync("api/Transactions/" + i.ID);
                Trace.WriteLine("api/Transaction/" + i.ID);
                Trace.WriteLine("get transactions response status: " + responseDelete.StatusCode);
                
            }
        }

        /**<summary>Synchronously loads the transactions from the json file into the unsyncedTransactions list.</summary>
         */
        private void LoadTransactionsFromJson()
        {
            if (!File.Exists(transactionsJsonPath))
            {
                File.Create(transactionsJsonPath);
                Trace.WriteLine("nothing to deserialize"); //DEBUG
                txtUnsyncedTransactionsCount.Text = "0"; 
            }
            else
            {
                //read the objects into unsyncedTransactions
                string jsonString = File.ReadAllText(transactionsJsonPath);
                try
                {
                    unsyncedTransactions = JsonSerializer.Deserialize<ObservableCollection<Transaction>>(jsonString);
                }
                catch
                {
                    //what should it do if the json cant be read?
                    var info = new FileInfo(transactionsJsonPath);
                    if (!(info.Length == 0)) //if file is not empty (if it's empty nothing needs to happen, the program can continue as normal)
                    {
                        //need to do something with the unreadable info
                        Trace.WriteLine("could not deserialize"); //DEBUG
                    }
                }
                foreach (Transaction i in unsyncedTransactions) //DEBUG
                {
                    Trace.WriteLine("deserialized-> studentID: " + i.StudentID + " studentName: " + i.StudentName + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID);
                }
                txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString(); 
            }
        }

        /**<summary>Asynchronously gets the transactions from the json file containing the transactions that were not 
         * previously sent to the database and adds them to the unsyncedTransactions list. Updates the unsynced transactions
         * counter in the GUI.</summary>
         */
        private async Task LoadTransactionsFromJsonAsync() 
        {
            if (!File.Exists(transactionsJsonPath))
            {
                File.Create(transactionsJsonPath);
                Trace.WriteLine("nothing to deserialize"); //DEBUG
                txtUnsyncedTransactionsCount.Text = "0";
            }
            else
            {
                //read the objects into unsyncedTransactions
                //string jsonString = File.ReadAllText(transactionsJsonPath);
                try
                {
                    StreamReader sr = new StreamReader(transactionsJsonPath);
                    unsyncedTransactions = await JsonSerializer.DeserializeAsync<ObservableCollection<Transaction>>(sr.BaseStream);
                    sr.Dispose();
                }
                catch
                {
                    var info = new FileInfo(transactionsJsonPath);
                    if (!(info.Length == 0)) //if file is not empty (if it's empty nothing needs to happen, the program can continue as normal)
                    {
                        //try again
                        try
                        {
                            StreamReader sr = new StreamReader(transactionsJsonPath);
                            unsyncedTransactions = await JsonSerializer.DeserializeAsync<ObservableCollection<Transaction>>(sr.BaseStream);
                            sr.Dispose();
                        }
                        catch
                        {
                            //!!do something with the unreadable info - copy the file and make a new one? delete the file?
                            Trace.WriteLine("could not deserialize"); //DEBUG
                        }
                        
                    }
                }
                foreach (Transaction i in unsyncedTransactions) //DEBUG
                {
                    Trace.WriteLine("deserialized-> studentID: " + i.StudentID + " studentName: " + i.StudentName + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID);
                }
                txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString();
            }
        }

        /**<summary>Loads the students, schools, and food items into the main window's lists.</summary>
         */
        private async Task GetDataAsync() 
        {
            try
            {
                var responseStudents = await client.GetAsync("api/Students/School/" + ThisSchool.ID);
                students = await responseStudents.Content.ReadAsAsync<List<Student>>();
                
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

                /*
                var responseSchools = await client.GetAsync("api/Schools");
                schools = await responseSchools.Content.ReadAsAsync<List<School>>();
                Trace.WriteLine("get schools response status: " + responseSchools.StatusCode); //DEBUG
                if (this.schools != null) //DEBUG
                {
                    foreach (School i in schools)
                    {
                        Trace.WriteLine("Name: " + i.Name + " ID: " + i.ID);
                    }
                }
                else
                {
                    Trace.WriteLine("Schools is null");
                }
                */
                var responseFood = await client.GetAsync("api/FoodItems/School/" + ThisSchool.ID);
                Trace.WriteLine(ThisSchool.ID); //DEBUG
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
            }
            catch (HttpRequestException)
            {
                shutdown = true;
                MessageBox.Show("Failed to connect to the database, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
            Trace.WriteLine("1"); //DEBUG
        }

        #region unused async get data methods
        private async Task<List<Student>> GetStudentsAsync()
        {
            var responseStudents = await client.GetAsync("Students");
            return await responseStudents.Content.ReadAsAsync<List<Student>>();
        }

        private async Task<List<School>> GetSchoolsAsync()
        {
            var responseSchools = await client.GetAsync("Schools");
            return await responseSchools.Content.ReadAsAsync<List<School>>();
        }

        private async Task<ObservableCollection<FoodItem>> GetFoodItemsAsync()
        {
            var responseFood = await client.GetAsync("FoodItems");
            return await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();
        }
        #endregion

        /**<summary>For debugging purposes only.</summary>
         */
        public async Task AddTestDataToDatabase() //DEBUG
        {
            //Student student1 = new Student("1", "Student1", "1", 10.00M, "test medical info");
            //Student student2 = new Student("2", "Student2", "1", 20.00M, "");
            //School school1 = new School("School1", "1");
            //FoodItem food1 = new FoodItem("Pancake", 2.00M, "1");
            //FoodItem food2 = new FoodItem("Apple", 1.00M, "1");

            //Student student1 = new Student("3", "Student", "2", 15.00M, "studentID = 3");
            //Student student2 = new Student("4", "Student", "2", 25.00M, "studentID = 4");
            //School school1 = new School("School2", "2");
            //FoodItem food1 = new FoodItem("Pizza", 2.00M, "1"); //food IDs are assigned based on time, creating one object immediately after the other that belongs to the same school may result in duplicate key exceptions
            Student student;
            string jsonStringStudent;
            StringContent httpContentStudent;
            for (int i = 5; i <= 100; i++)
            {
                student = new Student(i.ToString(), "Student " + i, "1", 20.00M, "medical info");
                jsonStringStudent = JsonSerializer.Serialize(student);
                httpContentStudent = new StringContent(jsonStringStudent, Encoding.UTF8, "application/json");
                await client.PostAsync("api/Students", httpContentStudent);
            }

            /*
            string jsonStringStudent1 = JsonSerializer.Serialize(student1);
            var httpContentStudent1 = new StringContent(jsonStringStudent1, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Students", httpContentStudent1); 

            string jsonStringStudent2 = JsonSerializer.Serialize(student2);
            var httpContentStudent2 = new StringContent(jsonStringStudent2, Encoding.UTF8, "application/json");
            var response2 = await client.PostAsync("api/Students", httpContentStudent2);

            string jsonStringSchool1 = JsonSerializer.Serialize(school1);
            var httpContentSchool1 = new StringContent(jsonStringSchool1, Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("api/Schools", httpContentSchool1);
            */

            /*
            string jsonStringFood1 = JsonSerializer.Serialize(food1);
            var httpContentFood1 = new StringContent(jsonStringFood1, Encoding.UTF8, "application/json");
            var response4 = await client.PostAsync("api/FoodItems", httpContentFood1);

            FoodItem food2 = new FoodItem("Soup", 2.00M, "2");
            string jsonStringFood2 = JsonSerializer.Serialize(food2);
            var httpContentFood2 = new StringContent(jsonStringFood2, Encoding.UTF8, "application/json");
            var response5 = await client.PostAsync("api/FoodItems", httpContentFood2);
            */

            //if (response.IsSuccessStatusCode && response2.IsSuccessStatusCode && response3.IsSuccessStatusCode && response4.IsSuccessStatusCode && response5.IsSuccessStatusCode)
            //{
                //Trace.WriteLine("successfully added all test data to the database");
            //}
            //else
            //{
                //Trace.WriteLine("failed to add to the database. responses:");
                //Trace.WriteLine("student1: " + response);
                //Trace.WriteLine("student2: " + response2);
                //Trace.WriteLine("school1: " + response3);
                //Trace.WriteLine("food1: " + response4);
                //Trace.WriteLine("food2: " + response5);
                
            //}
        }


        /*
        public bool ValidStudentID(string studentID)
        {
            if (students == null)
            {
                return false;
            }
            if (string.IsNullOrWhiteSpace(studentID)) //could add more conditions here to avoid searching for things that couldn't be student numbers
            {
                return false;
            }
            foreach (Student i in students) //using linear search since the list may not be sorted by ID
            {
                if (i.StudentID.Equals(studentID))
                {
                    return true;
                }
            }
            return false;
        }
        */

        /**<summary>Searches for a student with the given student ID and returns the student object.</summary>
         * <param name="studentID">The ID of the student to search for.</param>
         * <returns>The Student object with the given studentID. Returns null if the student is not found.</returns>
         */
        public Student GetStudentByID(string studentID) //not going to assume the list is sorted by ID so use linear search
        {
            if (students == null)
            {
                return null;
            }
            if (string.IsNullOrWhiteSpace(studentID)) //could add more conditions here to avoid searching for things that couldn't be student numbers
            {
                return null;
            }

            foreach (Student i in students)
            {
                if (i.StudentID.Equals(studentID))
                {
                    return i;
                }
            }
            return null;
        }


        /**<summary>Generates a new transaction and adds it to the guiTransactions and unsyncedTransactions collections.
         * </summary>
         * <param name="StudentID">The customer's student ID.</param>
         * <param name="FoodID">The ID of the FoodItem to be purchased.</param>
         * <param name="foodName">The name of the FoodItem to be purchased.</param>
         * <param name="cost">The cost of the FoodItem to be purchased.</param>
         */
        private async Task GenerateTransaction(string StudentID, string FoodID, string foodName, decimal cost)
        {
            Student student = GetStudentByID(StudentID);
            if (student == null)
            {
                MessageBox.Show("Student not found, please ensure the student number was entered correctly.", "Unknown student", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //if transactions haven't been synced in a while, try to sync them
            if (unsyncedTransactions.Count % 25 == 0) //!!need to put a good number here
            {
                await SyncAllTransactionsAsync(); //automatically shows the loading dialog box and disables the main window
            }

            lastTransaction = new Transaction(StudentID, FoodID, foodName, cost, student.Name, ThisSchool.ID, ThisSchool.Name); //sets last transaction so it can be undone

            //add new transaction to the GUI list and the JSON list
            guiTransactions.Add(lastTransaction);
            unsyncedTransactions.Add(lastTransaction);

            IsEnabled = false;
            loadingWindow.Show();
            //serialize and overwrite
            using FileStream createStream = File.Create(transactionsJsonPath);
            await JsonSerializer.SerializeAsync(createStream, unsyncedTransactions);
            await createStream.DisposeAsync();
            //Trace.WriteLine(File.ReadAllText(transactionsJsonPath)); //DEBUG
            loadingWindow.Hide();
            IsEnabled = true;

            txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString(); 
            buttonUndoTransaction.IsEnabled = true; //enable the undo button
            Keyboard.Focus(txtEnterStudentNum);

        }


        /**<summary>Undoes the last transaction. Once a transaction is undone, 
         * the button is disabled.</summary>
         */
        private void Click_buttonUndoTransaction(object sender, RoutedEventArgs e)
        {
            if (lastTransaction != null)
            {
                guiTransactions.Remove(lastTransaction);
                unsyncedTransactions.Remove(lastTransaction);
                string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
                File.WriteAllText(transactionsJsonPath, SerializeJsonString); //overwrites the JSON with the updated list of transactions
                lastTransaction = null;
                txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString(); 
                foreach (Transaction i in guiTransactions) //DEBUG
                {
                    Trace.WriteLine("remaining transaction->  name: " + i.StudentName + " ID: " + i.StudentID + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID); //DEBUG
                }

            }
            buttonUndoTransaction.IsEnabled = false; //disable the button after undo
            Keyboard.Focus(txtEnterStudentNum); //keep the cursor in the enter student number box
        }


        private void Click_buttonSync(object sender, RoutedEventArgs e)
        {
            this.Close(); //the window_close event handles syncing and exiting
            //await SyncTransactionsAsync(); //DEBUG
        }


        /**<remarks>Use SyncAllTransactionsAsync instead. This method causes concurrent modification exceptions 
         * and is more likely to cause errors in syncing the transaction log</remarks>
         */
        private async Task SyncTransactionsAsync()
        {
            loadingWindow.Show();
            IsEnabled = false;

            List<string> syncedTransactionIDs = new List<string>(); //stores the IDs of the transactions that have been synced
            foreach (Transaction i in unsyncedTransactions) //DEBUG
            {
                Trace.WriteLine("unsyned transaction-> ID: " + i.ID + " name: " + i.StudentName + " studentID: " + i.StudentID + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID); //DEBUG
            }
            //List<Transaction> syncedTransactions = new List<Transaction>();
            bool successfullySynced = true;

            foreach (Transaction i in unsyncedTransactions) //the transactions are removed from unsyned transactions after all transactions have attempted to be sent to the server, so if the program shuts down while syncing, the synced transactions may try to sync again which causes internal server errors
            {
                try
                {
                    Trace.WriteLine("trying to sync a transaction with ID: " + i.ID); //DEBUG
                    string jsonString = JsonSerializer.Serialize(i);
                    var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/Transactions", httpContent);
                    Trace.WriteLine("Transaction sync response: " + response); //DEBUG
                    Trace.WriteLine("IsSuccessStatusCode: " + response.IsSuccessStatusCode); //DEBUG
                    if (response.IsSuccessStatusCode)
                    {
                        syncedTransactionIDs.Add(i.ID); //if successfully synced, add to list of synced transactions
                    }
                    else
                    {
                        successfullySynced = false;
                    }
                }
                catch
                {
                    //if not synced, do not add to list of synced
                    successfullySynced = false;
                }
            }

            Trace.WriteLine("successfullySynced: " + successfullySynced); //DEBUG
            Trace.WriteLine("num synced transactions: " + syncedTransactionIDs.Count); //DEBUG
            foreach (string i in syncedTransactionIDs)
            {
                Trace.WriteLine("ID of successfully synced transaction: " + i);
            }
            //make sure synced and unsynced transactions lists match, if not, remove synced from unsynced 
            if (successfullySynced)
            {
                File.Delete(transactionsJsonPath);

                /*
                MessageBox.Show(
                "Syncing is complete, the program will now be closed",
                "Syncing complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information); //this dialog box may not be necessary
                */
            }
            else
            {
                //remove the transactions that were synced from the unsynced transactions list (compare by ID to avoid potential bugs)
                int testInt = unsyncedTransactions.ToList().RemoveAll(x => !syncedTransactionIDs.Any(y => y.Equals(x.ID))); //testInt is for DEBUG
                Trace.WriteLine("num transactions removed: " + testInt); //DEBUG - says 1 was removed when 0 were actually removed
                foreach (Transaction i in unsyncedTransactions) //DEBUG
                {
                    Trace.WriteLine("transaction not removed-> ID: " + i.ID + " name: " + i.StudentName + " studentID: " + i.StudentID + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID); //DEBUG
                }
                //unsyncedTransactions.ToList().RemoveAll(x => !syncedTransactions.Any(y => y.ID.Equals(x.ID)));
                string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
                File.WriteAllText(transactionsJsonPath, SerializeJsonString); //overwrites the JSON with the updated list of unsynced transactions
                /*
                MessageBox.Show(
                        "Syncing failed and will be completed later. The program will now be closed",
                        "Syncing failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                */
                
            }
            txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString();
            loadingWindow.Hide();
            IsEnabled = true;
        }


        /**<summary>Attempts to sync all unsynced transactions to the database.
         * Shows and hides a loading window automatically.</summary>
         */
        private async Task SyncAllTransactionsAsync() 
        {
            loadingWindow.Show();
            IsEnabled = false;
            for (int i = unsyncedTransactions.Count - 1; i >= 0; i--)
            {
                Trace.WriteLine("trying to sync a transaction with ID: " + unsyncedTransactions[i].ID); //DEBUG
                Trace.WriteLine(unsyncedTransactions[i]); //DEBUG
                string jsonString = JsonSerializer.Serialize(unsyncedTransactions[i]);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                try
                {
                    var response = await client.PostAsync("api/Transactions", httpContent);
                    //Trace.WriteLine("Transaction sync response: " + response); //DEBUG
                    //Trace.WriteLine("IsSuccessStatusCode: " + response.IsSuccessStatusCode); //DEBUG
                    if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.Conflict) //if the transaction is in the database, remove it from the list
                    {
                        unsyncedTransactions.RemoveAt(i); //remove the transaction from the unsynced transactions list since it exists in the database
                        string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
                        File.WriteAllText(transactionsJsonPath, SerializeJsonString); //update the transactions log
                    }
                }
                catch (HttpRequestException) //this exception is not thrown if the URI can't be found
                {
                    Trace.WriteLine("can't reach the database");
                    break; //if there is a server error, no point trying to sync any more transactions
                }   
            }
            txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString();
            loadingWindow.Hide();
            IsEnabled = true;
            Keyboard.Focus(txtEnterStudentNum); //after the loading box is gone, the box to enter student numbers loses keyboard focus, so give it keyboard focus again
        }


        private void SelectionChanged_dataGridFoodItems(object sender, SelectionChangedEventArgs e)
        {
            FoodItem selectedItem = dataGridFoodItems.SelectedItem as FoodItem; //if casting fails this returns null
            if (selectedItem != null)
            {
                txtSelectedFoodName.Text = selectedItem.Name;
                txtSelectedFoodCost.Text = "$" + selectedItem.Cost.ToString();
                txtSelectedFoodDescription.Text = selectedItem.Description;
            }
            else
            {
                //if the selected item ends up being null the program probably loaded incorrectly, restart the program
                Close();
            }
        }

        /**<summary>Keeps the focus on the box to enter a student number</summary>
         */
        private void GotKeyboardFocus_dataGridFoodItems(object sender, KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(txtEnterStudentNum);
        }


        private async void OnEnterKeyDownHandler_StudentNum(object sender, KeyEventArgs e)
        {
            //might need to check if the program is busy before attempting to make a new transaction
            if ((e.Key == Key.Return) || (e.Key == Key.Enter))
            {
                lastTransactionStopwatch.Stop();
                if ((lastTransaction != null) && (lastTransactionStopwatch.ElapsedMilliseconds < 200)) //helps prevent accidental duplicate scanning
                {
                    txtEnterStudentNum.Text = ""; //clears the textbox
                    Trace.WriteLine("it has been less than 200ms since the last transaction");
                    lastTransactionStopwatch.Start();
                    return;
                }

                Trace.WriteLine("you entered: " + txtEnterStudentNum.Text); //DEBUG
                string studentID = txtEnterStudentNum.Text;
                txtEnterStudentNum.Text = ""; //clears the textbox
                FoodItem selectedItem = dataGridFoodItems.SelectedItem as FoodItem;
                if (selectedItem != null)
                {
                    string foodID = selectedItem.ID;
                    string foodName = selectedItem.Name;
                    decimal cost = selectedItem.Cost;
                    Student student = GetStudentByID(studentID);
                    //Trace.WriteLine(foodID); //DEBUG
                    await GenerateTransaction(studentID, foodID, foodName, cost); //this automatically updates lastTransaction
                    if (student != null)
                    {
                        string medicalInfo = student.MedicalInfo;
                        txtMedicalInfo.Text = medicalInfo;
                    }
                    lastTransactionStopwatch.Restart(); 
                }
                else
                {
                    txtEnterStudentNum.Text = ""; //clears the textbox
                    MessageBox.Show("No item is selected, please select an item in the box on the left before entering a student number.", "No selected item", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
        }

    }
}
