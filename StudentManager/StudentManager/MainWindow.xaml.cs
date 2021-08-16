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
using Microsoft.Win32;
using CsvHelper;
using System.Globalization;

namespace StudentManager
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static HttpClient client;
        private string apiUri;
        private static ObservableCollection<Student> _students;
        private LoadingBox loadingWindow;
        private ImportCsvDialog importCsvDialog;
        private AddStudentDialog addStudentDialog;
        private static List<Student> _unsyncedStudents;
        private static ObservableCollection<string> _schoolIDs;

        public static ObservableCollection<Student> students { get { return _students; } set { _students = value; } }
        public static List<Student> unsyncedStudents { get { return _unsyncedStudents; } set { _unsyncedStudents = value; } }
        public static ObservableCollection<string> schoolIDs { get { return _schoolIDs; } set { _schoolIDs = value; } }
        

        public MainWindow()
        {
            InitializeComponent();
        }

        private async void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
            loadingWindow.Show();
            IsEnabled = false;
            var configFile = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            try
            {
                apiUri = configFile.AppSettings.Settings["apiUri"].Value.ToString();
            }
            catch
            {
                MessageBox.Show("There was an error reading from the configuration file, closing the program.", "Missing configuration file", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();
            }

            ApiHelper.Init(apiUri); //initializes settings for the HttpClient
            client = ApiHelper.ApiClient; //gets the newly initialized HttpClient

            schoolIDs = new();
            List<School> schools = await GetSchoolsAsync();
            if (schools != null && schools.Count > 0)
            {
                foreach (School i in schools)
                {
                    schoolIDs.Add(i.ID);
                }
                cbSchool.ItemsSource = schoolIDs;
                cbSchool.SelectedIndex = 0;
            }
            students = await GetStudentsAsync();
            dataGridStudents.ItemsSource = students;
            unsyncedStudents = new List<Student>();

            importCsvDialog = new ImportCsvDialog(this);
            addStudentDialog = new AddStudentDialog(this);
            btnDelete.IsEnabled = false;

            loadingWindow.Hide();
            IsEnabled = true;
        }

        /**<summary>Retrieves the students from the selected school from the database.</summary>
         * <returns>An ObservableCollection of students from the database.</returns>
         */
        private async Task<ObservableCollection<Student>> GetStudentsAsync()
        {
            ObservableCollection<Student> newStudentsCollection = new();
            try
            {
                var response = await client.GetAsync("api/Students/School/" + (String)cbSchool.SelectedItem);
                Trace.WriteLine(await response.Content.ReadAsStringAsync()); //DEBUG
                newStudentsCollection = await response.Content.ReadAsAsync<ObservableCollection<Student>>();
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

        /**<summary>Retrieves the list of schools from the database.</summary>
         * <returns>The list of schools in the system.</returns>
         */
        private async Task<List<School>> GetSchoolsAsync()
        {
            List<School> getSchools = new();
            try
            {
                var response = await client.GetAsync("api/Schools");
                getSchools = await response.Content.ReadAsAsync<List<School>>();

            }
            catch
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again. The program will now be closed.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                Close();

            }
            return getSchools;
        }

        private async void cbSchool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RefreshStudents();
        }

        /**<summary>Refreshes the student datagrid.</summary>
         */
        private async Task RefreshStudents()
        {
            loadingWindow.Show();
            IsEnabled = false;

            students = await GetStudentsAsync();
            dataGridStudents.ItemsSource = null;
            dataGridStudents.ItemsSource = students;

            loadingWindow.Hide();
            IsEnabled = true;
        }

        private void btnExportCsv_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveDialog = new SaveFileDialog();
            saveDialog.Filter = "CSV (*.csv)|*.csv";
            saveDialog.FileName = "School" + cbSchool.SelectedItem + "-Students-" + DateTime.Now.ToString("MM-dd-yyyy");
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
                        MessageBox.Show("Student data could not be exported, choose a different file location and try again.", "Could not export", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }
                }
                using (var writer = new StreamWriter(saveDialog.FileName))
                {
                    using (var csv = new CsvWriter(writer, CultureInfo.InvariantCulture))
                    {
                        csv.WriteRecords(students);
                    }
                }
                loadingWindow.Hide();
                loadingWindow.SetMessage(LoadingBox.defaultMessage);
                IsEnabled = true;
                MessageBox.Show("Successfully exported student data.", "Export successful", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private async void btnImportCsv_Click(object sender, RoutedEventArgs e)
        {
            importCsvDialog.ShowDialog();
            if (unsyncedStudents.Count > 0)
            {
                var result = MessageBox.Show("Would you like to update the students that already exist in the database with the information in the imported file? Balances will not be updated.", "Update existing students?", MessageBoxButton.YesNoCancel);
                if (result == MessageBoxResult.Yes)
                {
                    await SyncUnsyncedStudentsAsync(true);
                }
                else if (result == MessageBoxResult.No)
                {
                    await SyncUnsyncedStudentsAsync(false);
                }
                else
                {
                    unsyncedStudents = new();
                }
            }
            
        }

        /**<summary>Syncs students to the database with the option to replace the information of students that already 
         * exist. The existing student's balances will not be updated but the rest of their information will be if
         * the option to replace existing students is enabled.</summary>
         * <param name="replaceExisting">True if existing students should have their information updated.</param>
         */
        private async Task SyncUnsyncedStudentsAsync(bool replaceExisting) 
        {
            loadingWindow.Show();
            IsEnabled = false;
            for (int i = unsyncedStudents.Count - 1; i >= 0; i--)
            {
                Trace.WriteLine("trying to sync a student with ID: " + unsyncedStudents[i].StudentID); //DEBUG
                try
                {
                    //var response = await client.PostAsync("api/Students", httpContent);
                    var getResponse = await client.GetAsync("api/Students/" + unsyncedStudents[i].StudentID);
                    //Trace.WriteLine(jsonString); //DEBUG
                    Trace.WriteLine("get response: " + getResponse); //DEBUG
                    if (replaceExisting && (getResponse.IsSuccessStatusCode))
                    {
                        try
                        {
                            var oldStudent = await getResponse.Content.ReadAsAsync<Student>(); //get old balance
                            decimal oldBalance = oldStudent.Balance;
                            Trace.WriteLine("old balance: " + oldBalance + " new balance: " + unsyncedStudents[i].Balance); //DEBUG
                            unsyncedStudents[i].Balance = oldBalance; //set the new balance back to what it was before importing the CSV
                            Trace.WriteLine("updated balance: " + unsyncedStudents[i].Balance); //DEBUG
                            string jsonString = JsonSerializer.Serialize(unsyncedStudents[i]);
                            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");
                            var putResponse = await client.PutAsync("api/Students/" + unsyncedStudents[i].StudentID, httpContent);
                            if (putResponse.IsSuccessStatusCode)
                            {
                                unsyncedStudents.RemoveAt(i);
                            }
                        }
                        catch
                        {
                            Trace.WriteLine("can't reach the database");
                            MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            Close();
                            break; //if there is a server error, no point trying to sync any more entries
                        }

                    }
                    else if (getResponse.StatusCode.Equals(System.Net.HttpStatusCode.NotFound))
                    {
                        Trace.WriteLine("student does not exist");
                        try
                        {
                            decimal initialBalance = unsyncedStudents[i].Balance;
                            unsyncedStudents[i].Balance = 0; //balance will be set by a transaction
                            Transaction transaction = new Transaction(unsyncedStudents[i].StudentID, "0", "Initial balance", (initialBalance * -1), unsyncedStudents[i].Name, "0", "Manager");
                            
                            string jsonString = JsonSerializer.Serialize(unsyncedStudents[i]);
                            var httpContent = new StringContent(jsonString, Encoding.UTF8, "application/json");

                            string jsonString2 = JsonSerializer.Serialize(transaction);
                            var httpContent2 = new StringContent(jsonString2, Encoding.UTF8, "application/json");

                            var postResponse = await client.PostAsync("api/Students", httpContent);
                            Trace.WriteLine(postResponse); //DEBUG
                            if (postResponse.IsSuccessStatusCode)
                            {
                                var transactionResponse = await client.PostAsync("api/Transactions", httpContent2);
                                Trace.WriteLine(transactionResponse);

                                Trace.WriteLine("student response: " + postResponse.ReasonPhrase + "\ntransaction response: " + transactionResponse.ReasonPhrase); //DEBUG
                                if (transactionResponse.IsSuccessStatusCode)
                                {
                                    Trace.WriteLine("sucessfully added student");
                                    unsyncedStudents.RemoveAt(i);
                                }
                                else
                                {
                                    Trace.WriteLine("error adding student");
                                    try
                                    {
                                        await client.DeleteAsync("api/Transactions/" + transaction.ID);
                                    }
                                    catch
                                    {
                                        Trace.WriteLine("can't reach the database");
                                        MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                        Close();
                                        break; //if there is a server error, no point trying to sync any more entries
                                    }
                                    try
                                    {
                                        await client.DeleteAsync("api/Students/" + unsyncedStudents[i].StudentID);
                                    }
                                    catch
                                    {
                                        Trace.WriteLine("can't reach the database");
                                        MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                        Close();
                                        break; //if there is a server error, no point trying to sync any more entries
                                    }
                                }
                            }

                        }
                        catch
                        {
                            Trace.WriteLine("can't reach the database");
                            MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                            Close();
                            break; //if there is a server error, no point trying to sync any more entries
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    Trace.WriteLine("can't reach the database");
                    MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                    Close();
                    break; //if there is a server error, no point trying to sync any more entries
                }
            }
            await RefreshStudents();
            loadingWindow.Hide();
            IsEnabled = true;
            MessageBox.Show("Successfully added students.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private async void dataGridStudents_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Delete)
            {
                e.Handled = true; //stops the default behavior of the delete key so that DeleteSelectedStudents can handle it
                Trace.WriteLine("pressed delete key"); //DEBUG
                await DeleteSelectedStudentsAsync();
            }
        }

        /**<summary>Deletes the students selected in the datagrid.</summary>
         */
        private async Task DeleteSelectedStudentsAsync()
        {
            if (dataGridStudents.SelectedItem == null || students.Count == 0 || dataGridStudents.SelectedItem.Equals(CollectionView.NewItemPlaceholder))
            {
                return;
            }
            else
            {
                string messageBoxMessage = "Are you sure you want to delete: ";
                var grid = dataGridStudents;
                Trace.WriteLine("1"); //DEBUG
                Student deletedStudent;
                foreach (var row in grid.SelectedItems)
                {
                    try
                    {
                        deletedStudent = (Student)row;
                        messageBoxMessage += deletedStudent.Name + ", ";
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
                    loadingWindow.Show();
                    IsEnabled = false;
                    for (int i = students.Count - 1; i >= 0; i--)
                    {
                        if (grid.SelectedItems.Contains(students[i]))
                        {
                            if (!string.IsNullOrWhiteSpace(students[i].StudentID))
                            {
                                try
                                {
                                    var response = await client.DeleteAsync("api/Students/" + students[i].StudentID);
                                    Trace.WriteLine(response.StatusCode.ToString()); //DEBUG
                                }
                                catch (HttpRequestException)
                                {
                                    Trace.WriteLine("can't reach the database");
                                    MessageBox.Show("Cannot connect to the server, your changes will not be saved. Please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
                                    Close();
                                    break; //if there is a server error, no point trying to sync any more entries
                                }

                            }
                            students.RemoveAt(i);
                        }
                    }
                }
            }
            loadingWindow.Hide();
            IsEnabled = true;
        }

        private async void btnRefreshStudents_Click(object sender, RoutedEventArgs e)
        {
            await RefreshStudents();
        }

        private async void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            await DeleteSelectedStudentsAsync();
        }

        private void dataGridStudents_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (dataGridStudents.SelectedItem != null)
            {
                btnDelete.IsEnabled = true;
            }
            else
            {
                btnDelete.IsEnabled = false;
            }
            
        }

        private async void btnAddStudent_Click(object sender, RoutedEventArgs e)
        {
            addStudentDialog.ShowDialog();

            await SyncUnsyncedStudentsAsync(false);
        }

        /**<summary>Checks if a student exists in the database.</summary>
         * <param name="studentID">The ID number of the student.</param>
         * <returns>True if the student exists, false if they do not exist. Null if the database could not be reached.</returns>
         */
        public static async Task<bool?> StudentExists(string studentID)
        {
            try
            {
                var response = await client.GetAsync("api/Students/" + studentID);
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound) //DEBUG
                {
                    return false;
                }
                else if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                else
                {
                    MessageBox.Show("Failed to connect to the server, please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);

                }
            }
            catch (HttpRequestException)
            {
                MessageBox.Show("Failed to connect to the server, please check your internet connection and try again.", "Connection failed", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return null;
        }
    }
}
