using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
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
using System.Windows.Shapes;
using Data.Models;


namespace StudentManager
{
    /// <summary>
    /// Interaction logic for ImportCsvDialog.xaml
    /// </summary>
    public partial class ImportCsvDialog : Window
    {
        private ObservableCollection<string> _headers;
        private LoadingBox loadingWindow;
        private string fileName;
        private bool acceptImport;

        private string cbStudentNumHeader;
        private string cbBalanceHeader;
        private string cbNameHeader;
        private string cbMedicalInfoHeader;
        private string cbSchoolHeader;
        

        public ObservableCollection<string> headers { get { return _headers; } set { _headers = value; } }

        public string FileName { get { return fileName; } }
        public bool AcceptImport { get { return acceptImport; } set { acceptImport = value; } }


        /**<summary>Constructor for ImportCsvDialog.</summary>
         * <param name="owner">The window's owner window.</param>
         */
        public ImportCsvDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;
            acceptImport = false;
            Trace.WriteLine("headers = new()"); //DEBUG
            headers = new();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
            btnConfirmImport.IsDefault = true;
        }

        private void btnChooseFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if ((openFileDialog.ShowDialog() == true) && (File.Exists(openFileDialog.FileName)))
            {
                if (!openFileDialog.FileName.EndsWith(".csv"))
                {
                    MessageBox.Show("The chosen file is not a CSV. Please choose a CSV file.", "File is not a CSV", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                try
                {
                    //loadingWindow.Show();

                    IsEnabled = false;
                    //try to read as a csv, if it fails we know it is not a CSV
                    var path = openFileDialog.FileName;
                    using (var parser = new TextFieldParser(path))
                    {
                        parser.TextFieldType = FieldType.Delimited;
                        parser.SetDelimiters(",");

                        string[] line;
                        while (!parser.EndOfData)
                        {
                            line = parser.ReadFields();
                        }
                    }

                }
                catch
                {
                    MessageBox.Show("The chosen file cannot be read as a CSV.", "File is not a CSV", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                finally
                {
                    //loadingWindow.Hide();

                    IsEnabled = true;
                }

                fileName = openFileDialog.FileName;
                tbFileChosen.Text = fileName;

                //loadingWindow.Show();

                IsEnabled = false;

                headers = new ObservableCollection<string>();
                Trace.WriteLine("headers = new()"); //DEBUG
                using (TextFieldParser parser = new TextFieldParser(fileName))
                {
                    int countRow = 0;
                    int countColumn = 0;
                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");

                    string[] fields = parser.ReadFields();
                    foreach (string field in fields)
                    {
                        if (countRow == 0)
                        {
                            headers.Add(field);
                            Trace.WriteLine("countRow == 0");
                        }
                        Trace.WriteLine("column num = " + countColumn); //DEBUG
                        Trace.WriteLine("field: " + field); //DEBUG
                        countColumn += 1;
                        Trace.WriteLine("headers count: " + headers.Count); //DEBUG
                    }
 
                }
                Trace.WriteLine("test");
                foreach (string i  in headers)
                {
                    Trace.WriteLine("header: " + i);
                }
                Trace.WriteLine("test2");

                if (!headers.Contains("StudentID"))
                {
                    cbChooseStudentNumColumn.ItemsSource = null;
                    cbChooseStudentNumColumn.ItemsSource = headers;
                    cbChooseStudentNumColumn.SelectedIndex = headers.IndexOf("StudentID");
                }
                else
                {
                    cbChooseStudentNumColumn.ItemsSource = null;
                    cbChooseStudentNumColumn.ItemsSource = headers;
                    cbChooseStudentNumColumn.SelectedIndex = 0;
                }

                if (!headers.Contains("Name"))
                {
                    cbChooseNameColumn.ItemsSource = null;
                    cbChooseNameColumn.ItemsSource = headers;
                    cbChooseNameColumn.SelectedIndex = 0;
                }
                else
                {
                    cbChooseNameColumn.ItemsSource = null;
                    cbChooseNameColumn.ItemsSource = headers;
                    cbChooseNameColumn.SelectedIndex = headers.IndexOf("Name");
                }
                
                if (!headers.Contains("SchoolID"))
                {
                    cbChooseSchoolColumn.ItemsSource = null;
                    cbChooseSchoolColumn.ItemsSource = headers;
                    cbChooseSchoolColumn.SelectedIndex = 0;
                }
                else
                {
                    cbChooseSchoolColumn.ItemsSource = null;
                    cbChooseSchoolColumn.ItemsSource = headers;
                    cbChooseSchoolColumn.SelectedIndex = headers.IndexOf("SchoolID");
                }
                
                if (!headers.Contains("Balance"))
                {
                    cbChooseBalanceColumn.ItemsSource = null;
                    cbChooseBalanceColumn.ItemsSource = headers;
                    cbChooseBalanceColumn.SelectedIndex = 0;
                }
                else
                {
                    cbChooseBalanceColumn.ItemsSource = null;
                    cbChooseBalanceColumn.ItemsSource = headers;
                    cbChooseBalanceColumn.SelectedIndex = headers.IndexOf("Balance");
                }

                if (!headers.Contains("MedicalInfo"))
                {
                    cbChooseMedicalInfoColumn.ItemsSource = null;
                    cbChooseMedicalInfoColumn.ItemsSource = headers;
                    cbChooseMedicalInfoColumn.SelectedIndex = 0;
                }
                else
                {
                    cbChooseMedicalInfoColumn.ItemsSource = null;
                    cbChooseMedicalInfoColumn.ItemsSource = headers;
                    cbChooseMedicalInfoColumn.SelectedIndex = headers.IndexOf("MedicalInfo");
                }

                //loadingWindow.Hide();
                Trace.WriteLine("final headers count: " + headers.Count); //DEBUG

                acceptImport = false;
                IsEnabled = true;

            }
                
        }

        private async void btnConfirmImport_Click(object sender, RoutedEventArgs e)
        {
            if (fileName != null)
            {
                cbStudentNumHeader = cbChooseStudentNumColumn.SelectedItem.ToString();
                cbBalanceHeader = cbChooseBalanceColumn.SelectedItem.ToString();
                cbNameHeader = cbChooseNameColumn.SelectedItem.ToString();
                cbMedicalInfoHeader = cbChooseMedicalInfoColumn.SelectedItem.ToString();
                cbSchoolHeader = cbChooseSchoolColumn.SelectedItem.ToString();

                List<string> chosenHeaders = new List<string>(new string[] {cbBalanceHeader, cbMedicalInfoHeader, cbNameHeader, cbStudentNumHeader, cbSchoolHeader});
                if (chosenHeaders.Count != chosenHeaders.Distinct().Count())
                {
                    Trace.WriteLine("count = " + chosenHeaders.Count + ", distinct headers = " + chosenHeaders.Distinct().Count());
                    Trace.WriteLine("balance = " + cbBalanceHeader); //DEBUG
                    Trace.WriteLine("medicalInfo = " + cbMedicalInfoHeader); //DEBUG
                    Trace.WriteLine("name = " + cbNameHeader); //DEBUG
                    Trace.WriteLine("studentID = " + cbStudentNumHeader); //DEBUG
                    Trace.WriteLine("school = " + cbSchoolHeader); //DEBUG
                    foreach (string i in chosenHeaders)
                    {
                        Trace.WriteLine(i); //StudentID is missing
                    }
                    acceptImport = false;
                    MessageBox.Show("Each column header can only be matched to one field.", "Cannot Import", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }
                
                acceptImport = true;
                await ImportCsvAsync();
                Close();
            }

        }


        private async Task ImportCsvAsync()
        {
            loadingWindow.Show();
            IsEnabled = false;
            try
            {
                await Task.Run(() => ImportCSV());
            }
            catch
            {
                loadingWindow.Hide();
                IsEnabled = true;
                MessageBox.Show("Error importing from the chosen file.", "Import error", MessageBoxButton.OK, MessageBoxImage.Error);
                acceptImport = false;
                return;
            }
            loadingWindow.Hide();
            IsEnabled = true;

        }

        private void ImportCSV()
        {
            if ((headers.Count == 0) || string.IsNullOrWhiteSpace(fileName))
            {
                Trace.WriteLine("headers is empty");
                return;
            }

            if (string.IsNullOrWhiteSpace(cbBalanceHeader) || string.IsNullOrWhiteSpace(cbMedicalInfoHeader) || string.IsNullOrWhiteSpace(cbNameHeader) || string.IsNullOrWhiteSpace(cbStudentNumHeader) || string.IsNullOrWhiteSpace(cbSchoolHeader))
            {
                Trace.WriteLine("one or more of the cb headers were null or whitespace");
                return;
            }

            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                int countRow = 0;
                int countColumn = 0;
                int numColumns = headers.Count;
                decimal newBalance = 0;
                string newStudentID = "";
                string newName = "";
                string newMedicalInfo = "";
                string newSchoolID = "";

                parser.TextFieldType = FieldType.Delimited;
                parser.SetDelimiters(",");
                while (!parser.EndOfData)
                {
                    Trace.WriteLine("row num = " + countRow); //DEBUG

                    //Process row
                    string[] fields = parser.ReadFields();
                    foreach (string field in fields)
                    {
                        //Process fields
                        if (countRow != 0)
                        {
                            Trace.WriteLine("countRow != 0");
                            Trace.WriteLine("range of headers list: 0-" + (headers.Count - 1));
                            Trace.WriteLine("column num: " + countColumn);
                            if (cbBalanceHeader.Equals(headers[countColumn]) && (!decimal.TryParse(field, out newBalance)))
                            {
                                Trace.WriteLine("could not parse decimal: " + field); //DEBUG
                                //display the message box on top of the loading window
                                Application.Current.Dispatcher.Invoke(() =>
                                {
                                    MessageBox.Show("There was an error importing the file. Make sure the columns and fields are set correctly and try again.", "Failed to import", MessageBoxButton.OK, MessageBoxImage.Error);

                                });
                                return;
                            }
                            else if (cbMedicalInfoHeader.Equals(headers[countColumn]))
                            {
                                newMedicalInfo = field;
                            }
                            else if (cbNameHeader.Equals(headers[countColumn]))
                            {
                                newName = field;
                            }
                            else if (cbStudentNumHeader.Equals(headers[countColumn]))
                            {
                                newStudentID = field;
                            }
                            else if (cbSchoolHeader.Equals(headers[countColumn]))
                            {
                                newSchoolID = field;
                            }
                            Trace.WriteLine("unsynced students count: " + MainWindow.unsyncedStudents.Count); //DEBUG
                        }
                        Trace.WriteLine("column num = " + countColumn); //DEBUG
                        Trace.WriteLine("field: " + field); //DEBUG
                        countColumn += 1;
                    }
                    if (countRow != 0)
                    {
                        MainWindow.unsyncedStudents.Add(new Student(newStudentID, newName, newSchoolID, newBalance, newMedicalInfo));
                        Trace.WriteLine("student-> name=" + MainWindow.unsyncedStudents[0].Name + ", studentNum=" + MainWindow.unsyncedStudents[0].StudentID + ", school=" + MainWindow.unsyncedStudents[0].SchoolID + ", balance=" + MainWindow.unsyncedStudents[0].Balance + ", medical info=" + MainWindow.unsyncedStudents[0].MedicalInfo); //DEBUG
                    }
                    countRow += 1;
                    countColumn = 0;
                }
            }
        }

        /**<summary>Converts the CSV rows to student objects.</summary>
            */
        /*
        private void ImportCSV()
        {
            try
            {
                using (TextFieldParser parser = new TextFieldParser(fileName))
                {
                    int countRow = 0;
                    int countColumn = 0;
                    int numColumns = headers.Count;
                    decimal newBalance = 0;
                    string newStudentID = "";
                    string newName = "";
                    string newMedicalInfo = "";
                    string newSchoolID = "";

                    parser.TextFieldType = FieldType.Delimited;
                    parser.SetDelimiters(",");
                    while (!parser.EndOfData)
                    {
                        Trace.WriteLine("row num = " + countRow); //DEBUG

                        //Process row
                        string[] fields = parser.ReadFields();
                        foreach (string field in fields)
                        {
                            //Process fields
                            if (countRow != 0)
                            {
                                Trace.WriteLine("countRow != 0");
                                Trace.WriteLine("range of headers list: 0-" + (headers.Count - 1));
                                Trace.WriteLine("column num: " + countColumn);
                                if (cbChooseBalanceColumn.SelectedItem.Equals(headers[countColumn]) && (!decimal.TryParse(field, out newBalance)))
                                {
                                    Trace.WriteLine("could not parse decimal: " + field); //DEBUG
                                    MessageBox.Show("There was an error importing the file. Make sure the columns and fields are set correctly and try again.", "Failed to import", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                                else if (cbChooseMedicalInfoColumn.SelectedItem.Equals(headers[countColumn]))
                                {
                                    newMedicalInfo = field;
                                }
                                else if (cbChooseNameColumn.SelectedItem.Equals(headers[countColumn]))
                                {
                                    newName = field;
                                }
                                else if (cbChooseStudentNumColumn.SelectedItem.Equals(headers[countColumn]))
                                {
                                    newStudentID = field;
                                }
                                else if (cbChooseSchoolColumn.SelectedItem.Equals(headers[countColumn]))
                                {
                                    newSchoolID = field;
                                }
                                Trace.WriteLine("unsynced students count: " + MainWindow.unsyncedStudents.Count); //DEBUG
                            }
                            Trace.WriteLine("column num = " + countColumn); //DEBUG
                            Trace.WriteLine("field: " + field); //DEBUG
                            countColumn += 1;
                        }
                        if (countRow != 0)
                        {
                            MainWindow.unsyncedStudents.Add(new Student(newStudentID, newName, newSchoolID, newBalance, newMedicalInfo));
                            Trace.WriteLine("student-> name=" + MainWindow.unsyncedStudents[0].Name + ", studentNum=" + MainWindow.unsyncedStudents[0].StudentID + ", school=" + MainWindow.unsyncedStudents[0].SchoolID + ", balance=" + MainWindow.unsyncedStudents[0].Balance + ", medical info=" + MainWindow.unsyncedStudents[0].MedicalInfo); //DEBUG
                        }
                        countRow += 1;
                        countColumn = 0;
                    }


                }
            }
            catch
            {
                MessageBox.Show("Error importing from the chosen file.", "Import error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }
        */


        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;

            tbFileChosen.Text = "No file chosen";
            headers = new();
            cbChooseStudentNumColumn.ItemsSource = null;
            cbChooseNameColumn.ItemsSource = null;
            cbChooseMedicalInfoColumn.ItemsSource = null;
            cbChooseBalanceColumn.ItemsSource = null;
            cbChooseSchoolColumn.ItemsSource = null;

            Hide();
        }
    

        private void Window_ContentRendered(object sender, EventArgs e)
        {
            tbFileChosen.Text = "No file chosen";
            headers = new();
            Trace.WriteLine("headers = new() - Window_ContentRendered()"); //DEBUG
            cbChooseStudentNumColumn.ItemsSource = null;
            cbChooseNameColumn.ItemsSource = null;
            cbChooseMedicalInfoColumn.ItemsSource = null;
            cbChooseBalanceColumn.ItemsSource = null;
            cbChooseSchoolColumn.ItemsSource = null;
            acceptImport = false;
        }
    }
}
