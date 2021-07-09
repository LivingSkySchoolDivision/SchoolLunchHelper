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
        private List<School> schools;
        private ObservableCollection<FoodItem> foodItems;
        private ObservableCollection<Transaction> guiTransactions;
        private ObservableCollection<Transaction> unsyncedTransactions; //this gets the transactions from the JSON that did not sync, don't want them on the GUI so keep them separate
        private List<Student> students;
        private HttpClient client; //the application shares one http client
        private String apiUri;
        private Window loadingWindow;

        private Transaction lastTransaction;
        private string transactionsJsonPath = "TransactionsLog.json"; //this path will need to change
        //may need to add isProgramBusy field, maybe there's a built-in method

        private Stopwatch lastTransactionStopwatch; //keeps track of time since the last transaction

        private static School _ThisSchool = new School("school1", "1");
        public static School ThisSchool { get { return _ThisSchool; } set { _ThisSchool = value; } }


        public MainWindow()
        {
            InitializeComponent();

            schools = new();
            foodItems = new();
            guiTransactions = new();
            students = new();
            unsyncedTransactions = new();
            lastTransactionStopwatch = new();
        }


        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //txtEnterStudentNum.IsEnabled = false;
            //buttonUndoTransaction.IsEnabled = false;
            //buttonSync.IsEnabled = false;

            loadingWindow = new LoadingBox(this);
            loadingWindow.Show();
            IsEnabled = false;
            
            //DEBUG START 
            foodItems.Add(new FoodItem("pizza", 1.11M, "test description test description test description test description test description test description test description test description test description", "school1"));
            foodItems.Add(new FoodItem("soup", 2.22M, ""));
            students.Add(new Student("1111", "student1", "1", 11, "no medical info (1)"));
            students.Add(new Student("2222", "student2", "2", 22, "no medical info (2)"));
            schools.Add(new School("school1", "1"));
            schools.Add(new School("school2", "2"));
            schools.Add(new School("school3", "3"));
            //DEBUG END
            

            //String config = ConfigurationManager.ConnectionStrings["database"].ConnectionString;
            //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var connectionString = configFile.ConnectionStrings.ConnectionStrings["database"].ConnectionString;
            //MessageBox.Show(connectionString); //DEBUG

            //var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            //var apiUri = configFile.ConnectionStrings.ConnectionStrings["test"].ConnectionString;

            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            apiUri = configFile.AppSettings.Settings["test"].Value.ToString();
            string thisSchoolID = configFile.AppSettings.Settings["thisSchool"].Value;
            //MessageBox.Show(apiUri); //DEBUG
            Trace.WriteLine("ApiUri: " + apiUri); //DEBUG
            Trace.WriteLine("schoolID: " + thisSchoolID); //DEBUG

            ApiHelper.Init(apiUri); //initializes settings for the HttpClient
            client = ApiHelper.ApiClient; //gets the newly initialized HttpClient

            LoadTransactionsFromJson();

            try
            {
                //await ClearDbTransactionsAsync(); //DEBUG - deletes all transactions in the database
                await GetDataAsync();
                Trace.WriteLine("2"); //DEBUG
                //GetDataAsync().Wait();
                //gettingData.GetAwaiter().GetResult();
            }
            catch
            {
                MessageBox.Show("Failed to connect to the database", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                //!!may want some code here to load locally saved objects from the last time the program connected to the database
            }
            
            //set up the data binding
            dataGridTransactions.DataContext = guiTransactions;
            dataGridTransactions.ItemsSource = guiTransactions;
            dataGridFoodItems.DataContext = foodItems;
            dataGridFoodItems.ItemsSource = foodItems;
            txtUnsyncedTransactionsCount.DataContext = unsyncedTransactions.Count.ToString();

            //txtEnterStudentNum.IsEnabled = true;
            //buttonSync.IsEnabled = true;
            Trace.WriteLine("3"); //DEBUG
            loadingWindow.Hide();
            Trace.WriteLine("4"); //DEBUG
            IsEnabled = true;
            Trace.WriteLine("5"); //DEBUG
        }


        private async void Window_Closing(object sender, CancelEventArgs e) //!! IN PROGRESS
        {
            if (unsyncedTransactions.Count != 0 ) 
            {
                MessageBoxResult result =
                  MessageBox.Show(
                    "Send all transactions and exit the program?",
                    "Lunch Helper",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
                if (result == MessageBoxResult.No)
                {
                    e.Cancel = true;
                    Trace.WriteLine("the window closing method does not return when the e.cancel is set to true"); //this prints
                    return;
                }
                else
                {
                    loadingWindow.Show(); 
                    IsEnabled = false;
                    await SyncAllTransactionsAsync();
                    //await SyncTransactionsAsync(); //!!this isn't being waited for and it's causing all the Ef Core exceptions with syncing
                    //var syncTask = SyncTransactionsAsync(); 
                    //syncTask.Wait(); 

                }
            }

            //!! this doesnt work yet
            //ProgressBar pb = new();
            //pb.IsIndeterminate = true;
            
            Trace.WriteLine("closing the program..."); 

            ApiHelper.ApiClient.Dispose(); 
        }


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
                        //!!do something with the unreadable info
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


        private async Task GetDataAsync() 
        {/*!!causes: "Exception thrown: 'System.InvalidOperationException' in System.Net.Http.dll
An exception of type 'System.InvalidOperationException' occurred in System.Net.Http.dll but was not handled in user code
An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set."
          */
            //try
            //{ !!commented out try and catch to see the exceptions for debugging
                var responseStudents = await client.GetAsync("api/Students");
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
            
                    
                var responseFood = await client.GetAsync("api/FoodItems");
            foodItems = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();
            //List<FoodItem> foodItems = await responseFood.Content.ReadAsAsync<List<FoodItem>>(); //DEBUG
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

            //}
            //catch
            //{
            //MessageBox.Show("Failed to connect to the database", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
            //}
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

        public async Task AddTestDataToDatabase()
        {
            Student student1 = new Student("1", "Student1", "1", 10.00M, "test medical info");
            Student student2 = new Student("2", "Student2", "1", 20.00M, "");
            School school1 = new School("School1", "1");
            FoodItem food1 = new FoodItem("Pancake", 2.00M, "1");
            FoodItem food2 = new FoodItem("Apple", 1.00M, "1");

            string jsonStringStudent1 = JsonSerializer.Serialize(student1);
            var httpContentStudent1 = new StringContent(jsonStringStudent1, Encoding.UTF8, "application/json");
            var response = await client.PostAsync("api/Students", httpContentStudent1); 

            string jsonStringStudent2 = JsonSerializer.Serialize(student2);
            var httpContentStudent2 = new StringContent(jsonStringStudent2, Encoding.UTF8, "application/json");
            var response2 = await client.PostAsync("api/Students", httpContentStudent2);

            string jsonStringSchool1 = JsonSerializer.Serialize(school1);
            var httpContentSchool1 = new StringContent(jsonStringSchool1, Encoding.UTF8, "application/json");
            var response3 = await client.PostAsync("api/Schools", httpContentSchool1);

            string jsonStringFood1 = JsonSerializer.Serialize(food1);
            var httpContentFood1 = new StringContent(jsonStringFood1, Encoding.UTF8, "application/json");
            var response4 = await client.PostAsync("api/FoodItems", httpContentFood1);

            string jsonStringFood2 = JsonSerializer.Serialize(food2);
            var httpContentFood2 = new StringContent(jsonStringFood2, Encoding.UTF8, "application/json");
            var response5 = await client.PostAsync("api/FoodItems", httpContentFood2);


            if (response.IsSuccessStatusCode && response2.IsSuccessStatusCode && response3.IsSuccessStatusCode && response4.IsSuccessStatusCode && response5.IsSuccessStatusCode)
            {
                Trace.WriteLine("successfully added all test data to the database");
            }
            else
            {
                Trace.WriteLine("failed to add to the database. responses:");
                Trace.WriteLine("student1: " + response);
                Trace.WriteLine("student2: " + response2);
                Trace.WriteLine("school1: " + response3);
                Trace.WriteLine("food1: " + response4);
                Trace.WriteLine("food2: " + response5);
                
            }
        }

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

        /**<remarks>Returns null if the student cannot be found, make sure a student was returned before trying to use the student object</remarks>
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



        /**<summary>Generates a new transaction and adds it to the transactions collection
         * </summary>
         */
        private async Task GenerateTransaction(string StudentID, string FoodID, string foodName, decimal cost)
        {
            IsEnabled = false;
            loadingWindow.Show();

            Student student = GetStudentByID(StudentID);
            if (student == null)
            {
                MessageBox.Show("Student not found, please ensure the student number was entered correctly.", "Unknown student", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            //!!async code
            //if transactions haven't been synced in a while, try to sync them
            if (unsyncedTransactions.Count >= 1) //DEBUG - !!this is set to a low number for debugging
            {
                //var syncTask = SyncTransactionsAsync();
                //syncTask.Wait();
                //await SyncTransactionsAsync();
                await SyncAllTransactionsAsync();
            }
            if (!File.Exists(transactionsJsonPath))
            {
                File.Create(transactionsJsonPath);
            }
            //end of async code

            lastTransaction = new Transaction(StudentID, FoodID, foodName, cost, student.Name, ThisSchool.ID, ThisSchool.Name); //sets last transaction so it can be undone

            //add new transaction to the GUI list and the JSON list
            guiTransactions.Add(lastTransaction);
            unsyncedTransactions.Add(lastTransaction);

            //serialize and overwrite
            //string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
            //File.WriteAllText(transactionsJsonPath, SerializeJsonString); //don't need to worry about overwriting on this line, unsyncedTransactions has all the old transactions in it already
            using FileStream createStream = File.Create(transactionsJsonPath);
            await JsonSerializer.SerializeAsync(createStream, unsyncedTransactions);
            await createStream.DisposeAsync();
            //Trace.WriteLine(File.ReadAllText(transactionsJsonPath)); //DEBUG

            loadingWindow.Hide();
            IsEnabled = true;

            txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString(); 
            buttonUndoTransaction.IsEnabled = true; //enable the undo button

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

        /**<remarks>Use SyncAllTransactionsAsync instead. This method causes more concurrent modification exceptions 
         * and is more likely to cause errors in syncing the transaction log</remarks>
         */
        private async Task SyncTransactionsAsync()
        {
            //!! 20210708092602651 is duplicate primary key
            //   20210708095115301 is the actual ID, EF Core claims its ID is 20210708092602651
            //   actual: 20210708111341192  duplicate: 20210708095115301

            List<string> syncedTransactionIDs = new List<string>(); //stores the IDs of the transactions that have been synced
            foreach (Transaction i in unsyncedTransactions) //DEBUG
            {
                Trace.WriteLine("unsyned transaction-> ID: " + i.ID + " name: " + i.StudentName + " studentID: " + i.StudentID + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID); //DEBUG
            }
            //List<Transaction> syncedTransactions = new List<Transaction>();
            bool successfullySynced = true;

            //add loading bar, the main window should not be able to be interacted with while syncing

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
            
        }
        
        private async Task SyncAllTransactionsAsync() //alternative to SyncTransactionsAsync with a different implementation (that should work better)
        {
            for (int i = unsyncedTransactions.Count - 1; i >= 0; i--)
            {
                Trace.WriteLine("trying to sync a transaction with ID: " + unsyncedTransactions[i].ID); //DEBUG
                string jsonString = JsonSerializer.Serialize(unsyncedTransactions[i]);
                var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                var response = await client.PostAsync("api/Transactions", httpContent);
                Trace.WriteLine("Transaction sync response: " + response); //DEBUG
                Trace.WriteLine("IsSuccessStatusCode: " + response.IsSuccessStatusCode); //DEBUG
                if (response.IsSuccessStatusCode)
                {
                    unsyncedTransactions.RemoveAt(i);
                    string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
                    File.WriteAllText(transactionsJsonPath, SerializeJsonString);
                }

            }

            txtUnsyncedTransactionsCount.Text = unsyncedTransactions.Count.ToString(); 
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

        private void txtEnterStudentNum_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(txtEnterStudentNum);
        }
    }
}
