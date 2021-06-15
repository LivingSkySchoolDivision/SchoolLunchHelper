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

//debug:
using System.Diagnostics;
using System.Collections.ObjectModel; 

namespace lunch_project
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

        private Transaction lastTransaction;
        private string transactionsJsonPath = "TransactionsLog.json"; //this path will need to change
        //may need to add isProgramBusy field, maybe there's a built-in method

        private static School _ThisSchool = new School("school1", "1");
        public static School ThisSchool { get { return _ThisSchool; } } 


        public MainWindow()
        {
            InitializeComponent(); //this has to happen before anything to do with the GUI

            //DEBUG START (this will be replaced with getting data from EF Core)
            schools = new();
            foodItems = new();
            guiTransactions = new();
            students = new();
            unsyncedTransactions = new();
            foodItems.Add(new FoodItem("pizza", 1.11, "test description test description test description test description test description test description test description test description test description", "school1"));
            foodItems.Add(new FoodItem("soup", 2.22, ""));
            students.Add(new Student("1111", "student1", "1", 11, "no medical info (1)"));
            students.Add(new Student("2222", "student2", "2", 22, "no medical info (2)"));
            schools.Add(new School("school1", "1"));
            schools.Add(new School("school2", "2"));
            schools.Add(new School("school3", "3"));
            Trace.WriteLine(foodItems[0].Description); //DEBUG
            //DEBUG END

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
                    if(!(info.Length == 0)) //if file is not empty (if it's empty nothing needs to happen, the program can continue as normal)
                    {
                        //!!do something with the unreadable info
                    }
                }
                foreach (Transaction i in unsyncedTransactions) //DEBUG
                {
                    Trace.WriteLine("deserialized-> studentID: " + i.StudentID + " studentName: " + i.StudentName + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID);
                }

                //everything should have been loaded by the time it gets here - may want to check that none of the collections are null

            }


        }


        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            dataGridTransactions.DataContext = guiTransactions;
            dataGridTransactions.ItemsSource = guiTransactions;
            dataGridFoodItems.DataContext = foodItems;
            dataGridFoodItems.ItemsSource = foodItems;

            buttonUndoTransaction.IsEnabled = false;
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
            if(lastTransaction != null)
            {
                guiTransactions.Remove(lastTransaction); 
                unsyncedTransactions.Remove(lastTransaction);
                string SerializeJsonString = JsonSerializer.Serialize(unsyncedTransactions);
                File.WriteAllText(transactionsJsonPath, SerializeJsonString); //overwrites the JSON with the updated list of transactions
                lastTransaction = null; 
                foreach( Transaction i in guiTransactions) //DEBUG
                {
                Trace.WriteLine("remaining transaction->  name: " + i.StudentName + " ID: " + i.StudentID + " cost: " + i.Cost + " item: " + i.FoodName + " foodID: " + i.FoodID + " schoolName: " + i.SchoolName + " schoolID: " + i.SchoolID); //DEBUG
                }
                
            }
            buttonUndoTransaction.IsEnabled = false; //disable the button after undo
            Keyboard.Focus(txtEnterStudentNum); //keep the cursor in the enter student number box
        }


        private void Click_buttonSync(object sender, RoutedEventArgs e)
        {
            //should add "sync and exit the program?" dialog box

            //code to sync to the database

            //clear the JSON (!!THIS NEEDS TO BE IN AN IF THAT CHECKS THAT EVERYTHING WAS SYNCED TO THE DATABASE)
            File.Delete(transactionsJsonPath);
            //need to make sure the program can't do anything else while syncing is in progress, maybe have a loading bar window open
            MessageBox.Show("Syncing is complete, the program will now be closed", "Syncing complete", MessageBoxButton.OK, MessageBoxImage.Information); 
            this.Close(); //close the program after syncing
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
                if(selectedItem != null)
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
