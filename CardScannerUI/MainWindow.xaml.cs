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
using LunchAPI;
using System.Net.Http.Formatting;
using System.Net.Http;

//debug:
using System.Diagnostics;
using System.Collections.ObjectModel;


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
        private HttpClient client;

        private Transaction lastTransaction;
        private string transactionsJsonPath = "TransactionsLog.json"; //this path will need to change
        //may need to add isProgramBusy field, maybe there's a built-in method

        private static School _ThisSchool = new School("school1", "1");
        public static School ThisSchool { get { return _ThisSchool; } }


        public MainWindow()
        {
            InitializeComponent(); //this has to happen before anything to do with the GUI

            schools = new();
            foodItems = new();
            guiTransactions = new();
            students = new();

            ApiHelper.Init(); //initializes settings for the HttpClient
            client = ApiHelper.ApiClient; //gets the newly initialized HttpClient
            unsyncedTransactions = new();

            if (!File.Exists(transactionsJsonPath))
            {
                File.Create(transactionsJsonPath);
                Trace.WriteLine("nothing to deserialize"); //DEBUG
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
            }
            

        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //DEBUG START 
            foodItems.Add(new FoodItem("pizza", 1.11, "test description test description test description test description test description test description test description test description test description", "school1"));
            foodItems.Add(new FoodItem("soup", 2.22, ""));
            students.Add(new Student("1111", "student1", "1", 11, "no medical info (1)"));
            students.Add(new Student("2222", "student2", "2", 22, "no medical info (2)"));
            schools.Add(new School("school1", "1"));
            schools.Add(new School("school2", "2"));
            schools.Add(new School("school3", "3"));
            Trace.WriteLine(foodItems[0].Description); //DEBUG
            //DEBUG END

            //GetDataAsync(); //the program is not going to wait for this. adds items from the database to the program's collections
            //var task = GetStudentsAsync();
            //task.Wait();
            var getDataTask = GetDataAsync();
            getDataTask.Wait(); //waits for students, schools, and foodItems collections to get data from the database
            

            //set up the data binding
            dataGridTransactions.DataContext = guiTransactions;
            dataGridTransactions.ItemsSource = guiTransactions;
            dataGridFoodItems.DataContext = foodItems;
            dataGridFoodItems.ItemsSource = foodItems;

            buttonUndoTransaction.IsEnabled = false;
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            MessageBoxResult result =
                  MessageBox.Show(
                    "Sync and exit the program?",
                    "Lunch Helper",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);
            if (result == MessageBoxResult.No)
            {
                e.Cancel = true;
            }
            var syncTask = SyncTransactionsAsync(); 
            syncTask.Wait(); //waits for data to be synced before closing
            //!!need to test and make sure this actually waits for the data to be synced
        }



        private async Task GetDataAsync() 
        {/*!!causes: "Exception thrown: 'System.InvalidOperationException' in System.Net.Http.dll
An exception of type 'System.InvalidOperationException' occurred in System.Net.Http.dll but was not handled in user code
An invalid request URI was provided. The request URI must either be an absolute URI or BaseAddress must be set."
          */
            try
            {
                var responseStudents = await client.GetAsync("Students");
                students = await responseStudents.Content.ReadAsAsync<List<Student>>();
                foreach (Student i in students) //DEBUG
                {
                    Trace.WriteLine("Name: " + i.Name + " StudentID: " + i.StudentID + " SchoolID: " + i.SchoolID + " Balance: " + i.Balance + " MedicalInfo: " + i.MedicalInfo);
                }

                var responseSchools = await client.GetAsync("Schools");
                schools = await responseSchools.Content.ReadAsAsync<List<School>>();
                foreach (School i in schools) //DEBUG
                {
                    Trace.WriteLine("Name: " + i.Name + " ID: " + i.ID);
                }
                    
                var responseFood = await client.GetAsync("FoodItems");
                foodItems = await responseFood.Content.ReadAsAsync<ObservableCollection<FoodItem>>();
                foreach (FoodItem i in foodItems) //DEBUG
                {
                    Trace.WriteLine("Name: " + i.Name + " ID: " + i.ID + " SchoolID: " + i.SchoolID + " Cost: " + i.Cost + " Description: " + i.Description);
                }
            }
            catch
            {
                MessageBox.Show("Failed to connect to the database", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }

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
        private void GenerateTransaction(string StudentID, string FoodID, string foodName, double cost)
        { //if this needs to be async the json stuff will (likely) need to change
            Student student = GetStudentByID(StudentID);
            if (student == null)
            {
                MessageBox.Show("Student not found, please ensure the student number was entered correctly.", "Unknown student", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }
            lastTransaction = new Transaction(StudentID, FoodID, foodName, cost, student.Name, ThisSchool.ID, ThisSchool.Name); //sets last transaction so it can be undone

            //add new transaction to the GUI list and the JSON list
            guiTransactions.Add(lastTransaction);
            unsyncedTransactions.Add(lastTransaction);

            //serialize and overwrite
            string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
            File.WriteAllText(transactionsJsonPath, SerializeJsonString); //don't need to worry about overwriting on this line, unsyncedTransactions has all the old transactions in it already
            Trace.WriteLine(File.ReadAllText(transactionsJsonPath)); //DEBUG

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

        }

        private async Task SyncTransactionsAsync()
        {
            List<string> syncedTransactionIDs = new List<string>(); //stores the IDs of the transactions that have been synced
            //List<Transaction> syncedTransactions = new List<Transaction>();
            bool successfullySynced = true;

            foreach (Transaction i in unsyncedTransactions)
            {
                try
                {
                    //if successfully synced, add to list of synced transactions
                    //var response = await client.PostAsync<Transaction>("Transactions", i,); //need MediaTypeFormatter as 3rd argument
                    string jsonString = JsonSerializer.Serialize(i);
                    var httpContent = new StringContent(jsonString);
                    var response = await client.PostAsync("Transactions", httpContent); //!!controller's PostTransaction() method takes a Transaction object as an argument, this is sending a json-serialized string of the Transaction, this may cause problems but need to test it
                    if (response.IsSuccessStatusCode)
                    {
                        syncedTransactionIDs.Add(i.ID);
                        //syncedTransactions.Add(i);
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
            
            //make sure synced and unsynced transactions lists match, if not, remove synced from unsynced 
            if (successfullySynced)
            {
                File.Delete(transactionsJsonPath);

                MessageBox.Show(
                "Syncing is complete, the program will now be closed",
                "Syncing complete",
                MessageBoxButton.OK,
                MessageBoxImage.Information); //this dialog box may not be necessary
            }
            else
            {
                //remove the transactions that were synced from the unsynced transactions list (compare by ID to avoid potential bugs)
                int testInt = unsyncedTransactions.ToList().RemoveAll(x => !syncedTransactionIDs.Any(y => y.Equals(x.ID))); //testInt is for DEBUG
                Trace.WriteLine(testInt); //DEBUG
                //unsyncedTransactions.ToList().RemoveAll(x => !syncedTransactions.Any(y => y.ID.Equals(x.ID)));
                string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
                File.WriteAllText(transactionsJsonPath, SerializeJsonString); //overwrites the JSON with the updated list of unsynced transactions
                MessageBox.Show(
                        "Syncing failed and will be completed later. The program will now be closed",
                        "Syncing failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
            }
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
                //if the selected item ends up being null the program probably loaded incorrectly, restart the program?
            }
        }

        /**<summary>Keeps the focus on the box to enter a student number</summary>
         */
        private void GotKeyboardFocus_dataGridFoodItems(object sender, KeyboardFocusChangedEventArgs e)
        {
            Keyboard.Focus(txtEnterStudentNum);
        }


        private void OnEnterKeyDownHandler_StudentNum(object sender, KeyEventArgs e)
        {
            //might need to check if the program is busy before attempting to make a new transaction
            //there should be something that prevents scanning things within a few hundred miliseconds of eachother

            if ((e.Key == Key.Return) || (e.Key == Key.Enter))
            {
                Trace.WriteLine("you entered: " + txtEnterStudentNum.Text); //DEBUG
                string studentID = txtEnterStudentNum.Text;
                txtEnterStudentNum.Text = "";
                FoodItem selectedItem = dataGridFoodItems.SelectedItem as FoodItem;
                if (selectedItem != null)
                {
                    string foodID = selectedItem.ID;
                    string foodName = selectedItem.Name;
                    double cost = selectedItem.Cost;
                    Student student = GetStudentByID(studentID);
                    //Trace.WriteLine(foodID); //DEBUG
                    GenerateTransaction(studentID, foodID, foodName, cost); //this automatically updates lastTransaction
                    if (student != null)
                    {
                        string medicalInfo = student.MedicalInfo;
                        txtMedicalInfo.Text = medicalInfo;
                    }
                }
                else
                {
                    MessageBox.Show("No item is selected, please select an item in the box on the left before entering a student number.", "No selected item", MessageBoxButton.OK, MessageBoxImage.Information);
                }

            }
        }

        
    }
}
