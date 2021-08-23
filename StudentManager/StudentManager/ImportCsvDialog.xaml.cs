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
        private ObservableCollection<string> _headers = new();
        private LoadingBox loadingWindow;
        private string fileName;
        

        public ObservableCollection<string> headers { get { return _headers; } set { _headers = value; } }

        /**<summary>Constructor for ImportCsvDialog.</summary>
         * <param name="owner">The window's owner window.</param>
         */
        public ImportCsvDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
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
                    loadingWindow.Show();
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
                    loadingWindow.Hide();
                    IsEnabled = true;
                }

                fileName = openFileDialog.FileName;
                tbFileChosen.Text = fileName;

                loadingWindow.Show();
                IsEnabled = false;
                headers = new ObservableCollection<string>();
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

                loadingWindow.Hide();
                IsEnabled = true;

            }
                
        }

        private void btnConfirmImport_Click(object sender, RoutedEventArgs e)
        {
            if (fileName != null)
            {
                loadingWindow.Show();
                IsEnabled = false;

                ImportCSV();

                loadingWindow.Hide();
                IsEnabled = true;
                Close();
            }
            
        }

        /**<summary>Converts the CSV rows to student objects.</summary>
         */
        private void ImportCSV()
        {
            loadingWindow.Show();
            IsEnabled = false;
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
            finally
            {
                loadingWindow.Hide();
                IsEnabled = true;
            }
            
        }

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
    }
}
