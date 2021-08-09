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

        public ImportCsvDialog(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            loadingWindow = new LoadingBox(this);
        }

        private async void Button_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            if ((openFileDialog.ShowDialog() == true) && File.Exists(openFileDialog.FileName))
            {
                fileName = openFileDialog.FileName;
                tbFileChosen.Text = fileName;

                /*
                //txtEditor.Text = File.ReadAllText(openFileDialog.FileName);
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HasHeaderRecord = false, 
                };
                using (var reader = new StreamReader(openFileDialog.FileName))
                using (var csv = new CsvReader(reader, config))
                {
                    //var records = csv.GetRecords<Foo>();
                }
                */
                /*
                using (var reader = new StreamReader(fileName))
                using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
                {
                    csv.Read();
                    csv.ReadHeader();
                    
                    while (csv.Read())
                    {
                        var record = csv.GetRecord<Foo>();
                        // Do something with the record.
                    }
                }
                */
                loadingWindow.Show();
                IsEnabled = false;
                headers = new ObservableCollection<string>();
                using (TextFieldParser parser = new TextFieldParser(fileName))
                {
                    int countRow = 0;
                    int countColumn = 0;
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
                            if (countRow == 0)
                            {
                                headers.Add(field);
                                Trace.WriteLine("countRow == 0");
                            }
                            Trace.WriteLine("column num = " + countColumn); //DEBUG
                            Trace.WriteLine("field: " + field); //DEBUG
                            countColumn += 1;
                        }
                        countRow += 1;
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
            loadingWindow.Show();
            IsEnabled = false;

            ImportCSV();

            loadingWindow.Hide();
            IsEnabled = true;
        }

        private void ImportCSV()
        {
            using (TextFieldParser parser = new TextFieldParser(fileName))
            {
                int countRow = 0;
                int countColumn = 0;
                int numColumns = headers.Count;
                decimal result;
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
                            if ((cbChooseBalanceColumn.SelectedItem.Equals(headers[countColumn])) && (!decimal.TryParse(field, out result)))
                            {
                                Trace.WriteLine("could not parse decimal: " + field); //DEBUG
                                MessageBox.Show("There was an error importing the file. Make sure the columns and fields are set correctly and try again.", "Failed to import", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }
                            else if (cbChooseMedicalInfoColumn.SelectedItem.Equals(headers[countColumn]))
                            {

                            }
                            Trace.WriteLine("unsynced students count: " + MainWindow.unsyncedStudents.Count); //DEBUG
                            Trace.WriteLine("student-> name=" + MainWindow.unsyncedStudents[0].Name + ", studentNum=" + MainWindow.unsyncedStudents[0].StudentID + ", school=" + MainWindow.unsyncedStudents[0].SchoolID + ", balance=" + MainWindow.unsyncedStudents[0].Balance + ", medical info=" + MainWindow.unsyncedStudents[0].MedicalInfo); //DEBUG
                        }
                        //add row to unsynced students
                        Trace.WriteLine("column num = " + countColumn); //DEBUG
                        Trace.WriteLine("field: " + field); //DEBUG
                        countColumn += 1;
                    }
                    countRow += 1;
                }
                

            }
        }
    }
}
