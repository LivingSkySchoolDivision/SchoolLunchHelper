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
        private HttpClient client;
        private string apiUri;
        private static ObservableCollection<Student> _students;
        private LoadingBox loadingWindow;
        private ImportCsvDialog importCsvDialog;
        private static List<Student> _unsyncedStudents;

        public static ObservableCollection<Student> students { get { return _students; } set { _students = value; } }
        public static List<Student> unsyncedStudents { get { return _unsyncedStudents; } set { _unsyncedStudents = value; } }
        

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

            List<string> schoolIDs = new();
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

            loadingWindow.Hide();
            IsEnabled = true;
        }

        private async Task<ObservableCollection<Student>> GetStudentsAsync()
        {
            ObservableCollection<Student> newStudentsCollection = new();
            try
            {
                var responseFood = await client.GetAsync("api/Students/School/" + (String)cbSchool.SelectedItem);
                Trace.WriteLine(await responseFood.Content.ReadAsStringAsync()); //DEBUG
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

        private async void dataGridStudents_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            //!!
        }

        private async void cbSchool_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            await RefreshStudents();
        }

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

        private void btnImportCsv_Click(object sender, RoutedEventArgs e)
        {
            importCsvDialog.ShowDialog();
        }
    }
}
