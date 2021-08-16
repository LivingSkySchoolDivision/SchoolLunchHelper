using Data.Models;
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
using System.Windows.Shapes;

namespace StudentManager
{
    /// <summary>
    /// Interaction logic for AddStudentDialog.xaml
    /// A dialog box to add a new student to the database.
    /// </summary>
    public partial class AddStudentDialog : Window
    {
        /**<summary>Constructor for AddStudentDialog.</summary>
         * <param name="owner">The window's owner window.</param>
         */
        public AddStudentDialog(MainWindow owner)
        {
            InitializeComponent();
            Owner = owner;
            
        }
        

        private async void btnAddNewStudent_Click(object sender, RoutedEventArgs e)
        {
            bool valid = true;
            if (string.IsNullOrWhiteSpace(txtName.Text))
            {
                txtName.Focus();
                txtName.BorderBrush = Brushes.Red;
                txtNameInvalid.Visibility = Visibility.Visible;
                valid = false;
            }

            Decimal balance = 0;
            if (string.IsNullOrWhiteSpace(txtBalance.Text))
            {
                txtBalance.Focus();
                txtBalance.BorderBrush = Brushes.Red;
                txtBalanceInvalid.Visibility = Visibility.Visible;
                valid = false;
                
            }
            else if ((!Decimal.TryParse(txtBalance.Text, out balance)) || (balance < 0M))
            {
                txtBalance.Focus();
                txtBalance.BorderBrush = Brushes.Red;
                txtBalanceInvalid.Visibility = Visibility.Visible;
                valid = false;
            }

            if (string.IsNullOrWhiteSpace(txtStudentID.Text) || (await MainWindow.StudentExists(txtStudentID.Text) ?? true))
            {
                txtStudentID.Focus();
                txtStudentID.BorderBrush = Brushes.Red;
                txtStudentIDInvalid.Visibility = Visibility.Visible;
                valid = false;
            }

            if (valid)
            {
                Student newStudent;
                balance = decimal.Round(balance, 2);
                if (txtMedicalInfo.Text != null)
                {
                    newStudent = new Student(txtStudentID.Text, txtName.Text, (string)cbSchoolID.SelectedItem, balance, txtMedicalInfo.Text);
                }
                else
                {
                    newStudent = new Student(txtStudentID.Text, txtName.Text, (string)cbSchoolID.SelectedItem, balance, "");
                }

                MainWindow.unsyncedStudents.Add(newStudent);
                Close();
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            txtBalance.Clear();
            txtName.Clear();
            txtMedicalInfo.Clear();
            txtStudentID.Clear();

            txtName.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
            txtBalance.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
            txtStudentID.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));

            txtBalanceInvalid.Visibility = Visibility.Hidden;
            txtNameInvalid.Visibility = Visibility.Hidden;
            txtStudentIDInvalid.Visibility = Visibility.Hidden;
            Hide();
            return;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            cbSchoolID.ItemsSource = MainWindow.schoolIDs;
            cbSchoolID.SelectedIndex = 0;
        }
    }
}
