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

namespace LunchManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private HttpClient client; //the application shares one http client
        private ObservableCollection<Transaction> transactions;
        private ObservableCollection<Student> students;
        private ObservableCollection<FoodItem> foodItems;
        private School thisSchool;
        private Window loadingWindow;

        public MainWindow()
        {
            InitializeComponent();

            transactions = new();
            students = new();
            foodItems = new();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
            loadingWindow.Show();
            IsEnabled = false;

            MessageBox.Show("test"); //DEBUG
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

            await GetDataAsync();

            //set up data binding
            dataGridFoodTypes.DataContext = foodItems;
            dataGridFoodTypes.ItemsSource = foodItems;

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
                MessageBox.Show("Failed to connect to the database, closing the program", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

        }

        /**<summary>Loads the students, schools, and food items into the main window's lists.</summary>
         */
        private async Task GetDataAsync()
        {
            try
            {
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
                
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the database, please check you internet connection. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }
        }

        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataGridFoodTypes.BeginEdit();
            if (dataGridFoodTypes.SelectedItem != null)
            {
                Trace.WriteLine("You selected: " + dataGridFoodTypes.SelectedItem.ToString()); //DEBUG
            }
            
        }

        private void dataGridFoodTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void btnEditFoodItem_Click(object sender, RoutedEventArgs e)
        {
            dataGridFoodTypes.BeginEdit();
        }
    }
}
