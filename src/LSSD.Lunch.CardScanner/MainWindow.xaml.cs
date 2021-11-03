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
using System.Net.Http.Formatting;
using System.Net.Http;
using System.Diagnostics;
using System.Collections.ObjectModel;
using System.Timers;
using System.Configuration;
using LSSD.Lunch;

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
        //if these change, remember to add them to .gitignore:
        private readonly string transactionsJsonPath = "TransactionsLog.json";
        private readonly string foodItemsJsonPath = "FoodItemsBackup.json";
        private readonly string studentsJsonPath = "StudentsBackup.json";
        private readonly string schoolJsonPath = "SchoolBackup.json";

        private Stopwatch lastTransactionStopwatch; //keeps track of time since the last transaction
        private bool shutdown = false;
        private bool connectionFailed = false; 

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

            //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //apiUri = configFile.AppSettings.Settings["apiUri"].Value.ToString();
            //string thisSchoolID = configFile.AppSettings.Settings["thisSchool"].Value;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            string thisSchoolID = "";
            try
            {
                apiUri = configFile.AppSettings.Settings["apiUri"].Value.ToString();
                thisSchoolID = configFile.AppSettings.Settings["thisSchool"].Value;
            }
            catch
            {
                MessageBox.Show("There was an error reading from the configuration file, closing the program.", "Missing configuration file", MessageBoxButton.OK, MessageBoxImage.Error);
                shutdown = true;
                Close();
            }
            if (string.IsNullOrEmpty(thisSchoolID) || string.IsNullOrEmpty(apiUri))
            {
                MessageBox.Show("There was an error reading from the configuration file, closing the program.", "Missing configuration file", MessageBoxButton.OK, MessageBoxImage.Error);
                shutdown = true;
                Close();
            }
            

            ApiHelper.Init(apiUri); //initializes settings for the HttpClient
            client = ApiHelper.ApiClient; //gets the newly initialized HttpClient

            

            //LoadTransactionsFromJson();
            await LoadTransactionsFromJsonAsync();

            await GetThisSchoolFromIdAsync(thisSchoolID);
            //Trace.WriteLine("school name: " + ThisSchool.Name + " school ID: " + ThisSchool.ID); //DEBUG
            if (ThisSchool == null)
            {
                return;
            }
            this.Title = "Lunch Helper - " + ThisSchool.Name;

            await GetDataAsync();
            //if the data was fetched from the database, update the jsons
            if (!connectionFailed)
            {
                await WriteDataToBackupJsonsAsync();
            }
            
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
            buttonUndoTransaction.IsEnabled = false;
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
                ThisSchool = await response.Content.ReadAsAsync<School>();
                Trace.WriteLine("get this school from ID response status: " + response.StatusCode); //DEBUG
            }
            catch 
            {
                GetThisSchoolFromJson();
            }
            
        }

        /**<summary>Loads the school object from the backup JSON file.</summary>
         */
        private void GetThisSchoolFromJson()
        {
            if (!File.Exists(schoolJsonPath))
            {
                Trace.WriteLine("the file does not exist - ReadDataFromJsonAsync"); //DEBUG
                MessageBox.Show("Failed to connect to the database and no backup files exist, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                shutdown = true;
                Close();
            }
            else
            {
                string jsonString = File.ReadAllText(schoolJsonPath);
                try
                {
                    ThisSchool = JsonSerializer.Deserialize<School>(jsonString);
                }
                catch
                {
                    Trace.WriteLine("something went wrong when reading from the file - ReadDataFromJsonAsync"); //DEBUG
                    MessageBox.Show("Failed to connect to the database and backup files do not exist or could not be read, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    shutdown = true;
                    Close();
                }

            }
        }

        /**<summary>Event handler for the window's closing event. If the shutdown field is true the program will close 
         * without trying to sync transactions.</summary>
         */
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
                            try
                            {
                                info.MoveTo(DateTime.Now.ToString("yyyyMMddHHmmssff") + "unreadableTransactions.json", true);
                            }
                            catch
                            {
                                //if nothing else works, just let the file be overwritten
                            }
                            File.Create(transactionsJsonPath);
                            Trace.WriteLine("could not deserialize"); //DEBUG
                        }
                        
                    }
                }

                txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString();
            }
        }

        /**<summary>Loads the students and food items into the main window's lists.</summary>
         */
        private async Task GetDataAsync() 
        {
            try
            {
                var responseStudents = await client.GetAsync("api/Students/School/" + ThisSchool.Id);
                students = await responseStudents.Content.ReadAsAsync<List<Student>>();

                var responseFood = await client.GetAsync("api/FoodItems/School/" + ThisSchool.Id);
                foodItems = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();  
            }
            catch 
            {
                await ReadDataFromJsonAsync();
                connectionFailed = true;
            }
        }

        /**<summary>Asynchronously reads student and foodItem data from the backup JSON files and stores it in the window's lists.</summary>
         */
        private async Task ReadDataFromJsonAsync()
        {
            if (!File.Exists(studentsJsonPath) || !File.Exists(foodItemsJsonPath))
            {
                Trace.WriteLine("File does not exist - ReadDataFromJsonAsync"); //DEBUG
                MessageBox.Show("Failed to connect to the database and backup files do not exist or could not be read, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                shutdown = true;
                Close();
            }
            else
            {
                try
                {
                    StreamReader sr = new StreamReader(studentsJsonPath);
                    students = await JsonSerializer.DeserializeAsync<List<Student>>(sr.BaseStream);
                    sr.Dispose();

                    
                }
                catch
                {
                    Trace.WriteLine("something went wrong when reading from the students file - ReadDataFromJsonAsync"); //DEBUG
                    MessageBox.Show("Failed to connect to the database and backup files do not exist or could not be read, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    shutdown = true;
                    Close();
                }
                try
                {
                    StreamReader sr2 = new StreamReader(foodItemsJsonPath);
                    foodItems = await JsonSerializer.DeserializeAsync<ObservableCollection<FoodItem>>(sr2.BaseStream);
                    sr2.Dispose();
                }
                catch
                {
                    Trace.WriteLine("something went wrong when reading from the foodItems file - ReadDataFromJsonAsync"); //DEBUG
                    MessageBox.Show("Failed to connect to the database and backup files do not exist or could not be read, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    shutdown = true;
                    Close();
                }

            }

        }

        /**<summary>Writes the program's data into JSON files.</summary>
         */
        private async Task WriteDataToBackupJsonsAsync()
        {
            //writes the current school to a json
            string SerializeJsonString = JsonSerializer.Serialize(ThisSchool);
            File.WriteAllText(schoolJsonPath, SerializeJsonString);

            //writes the students to a json
            using FileStream studentStream = File.Create(studentsJsonPath);
            await JsonSerializer.SerializeAsync(studentStream, students);
            await studentStream.DisposeAsync();

            //writes the foodItems to a json
            using FileStream foodItemStream = File.Create(foodItemsJsonPath);
            await JsonSerializer.SerializeAsync(foodItemStream, foodItems);
            await foodItemStream.DisposeAsync();
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
                if (i.StudentId.Equals(studentID))
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
            if (unsyncedTransactions.Count % 30 == 0) //!!need to put a good number here
            {
                await SyncAllTransactionsAsync(); //automatically shows the loading dialog box and disables the main window
            }

            lastTransaction = new Transaction() {
                StudentID = StudentID,
                FoodItem = FoodID
            };
                StudentID, FoodID, foodName, cost, student.Name, ThisSchool.ID, ThisSchool.Name); //sets last transaction so it can be undone

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
            
            //List<Transaction> syncedTransactions = new List<Transaction>();
            bool successfullySynced = true;

            foreach (Transaction i in unsyncedTransactions) //the transactions are removed from unsyned transactions after all transactions have attempted to be sent to the server, so if the program shuts down while syncing, the synced transactions may try to sync again which causes internal server errors
            {
                try
                {
                    Trace.WriteLine("trying to sync a transaction with ID: " + i.Id.ToString()); //DEBUG
                    string jsonString = JsonSerializer.Serialize(i);
                    var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                    var response = await client.PostAsync("api/Transactions", httpContent);
                    Trace.WriteLine("Transaction sync response: " + response); //DEBUG
                    Trace.WriteLine("IsSuccessStatusCode: " + response.IsSuccessStatusCode); //DEBUG
                    if (response.IsSuccessStatusCode)
                    {
                        syncedTransactionIDs.Add(i.Id.ToString()); //if successfully synced, add to list of synced transactions
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
                int testInt = unsyncedTransactions.ToList().RemoveAll(x => !syncedTransactionIDs.Any(y => y.Equals(x.Id.ToString()))); //testInt is for DEBUG
                Trace.WriteLine("num transactions removed: " + testInt); //DEBUG - says 1 was removed when 0 were actually removed
                
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
                Trace.WriteLine("trying to sync a transaction with ID: " + unsyncedTransactions[i].Id); //DEBUG
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
                    string foodID = selectedItem.Id.ToString();
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
