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
using Data.Models;

namespace LunchManager
{
    /// <summary>
    /// Interaction logic for AddNewFoodItemHelper.xaml
    /// </summary>
    public partial class AddNewFoodItemHelper : Window
    {
        private School thisSchool;

        public AddNewFoodItemHelper(Window owner, School thisSchool)
        {
            InitializeComponent();
            Owner = owner;
            this.thisSchool = thisSchool;
        }

        private void btnCreateNewFoodType_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFoodName.Text))
            {
                txtFoodName.Focus();
                txtFoodName.BorderBrush = Brushes.Red;
            }
            Decimal cost;
            if (string.IsNullOrWhiteSpace(txtFoodCost.Text))
            {
                txtFoodCost.Focus();
                txtFoodCost.BorderBrush = Brushes.Red;
                txtPriceInvalid.Visibility = Visibility.Visible;
                return;
            }
            else if ((!Decimal.TryParse(txtFoodCost.Text, out cost)) || (cost < 0M))
            {
                txtFoodCost.Focus();
                txtFoodCost.BorderBrush = Brushes.Red;
                txtPriceInvalid.Visibility = Visibility.Visible;
                return;
            }
            FoodItem newFoodItem;
            if (txtFoodDescription.Text != null) 
            {
                newFoodItem = new FoodItem(txtFoodName.Text, cost, txtFoodDescription.Text, thisSchool.ID);
            }
            else
            {
                newFoodItem = new FoodItem(txtFoodName.Text, cost, "", thisSchool.ID);
            }
            
            MainWindow.unsyncedFoodItems.Add(newFoodItem);
            MainWindow.foodItems.Add(newFoodItem);
            this.Close();
            txtFoodCost.Clear();
            txtFoodName.Clear();
            txtFoodDescription.Clear();
            txtFoodName.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
            txtFoodCost.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
            txtPriceInvalid.Visibility = Visibility.Hidden;

        }

        private void txtFoodName_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtFoodName.Text))
            {
                txtFoodName.BorderBrush = Brushes.Red;
            }
            else
            {
                txtFoodName.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
            }
        }

        private void txtFoodCost_TextChanged(object sender, TextChangedEventArgs e)
        {
            /*
            if (string.IsNullOrWhiteSpace(txtFoodName.Text))
            {
                txtFoodCost.BorderBrush = Brushes.Red;
            }
            else
            {
                txtFoodCost.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
            }
            */
            decimal cost;
            if (string.IsNullOrWhiteSpace(txtFoodCost.Text))
            {
                txtFoodCost.Focus();
                txtFoodCost.BorderBrush = Brushes.Red;
                txtPriceInvalid.Visibility = Visibility.Visible;
                return;
            }
            else if ((!Decimal.TryParse(txtFoodCost.Text, out cost)) || (cost < 0M))
            {
                txtFoodCost.Focus();
                txtFoodCost.BorderBrush = Brushes.Red;
                txtPriceInvalid.Visibility = Visibility.Visible;
                return;
            }
            else
            {
                txtFoodCost.BorderBrush = new SolidColorBrush(Color.FromArgb(100, 171, 173, 179));
                txtPriceInvalid.Visibility = Visibility.Hidden;
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            e.Cancel = true;
            Hide();
            return;
        }
    }
}
