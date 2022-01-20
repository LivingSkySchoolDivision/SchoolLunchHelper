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

namespace CardScannerUI
{
    /// <summary>
    /// Interaction logic for LoadingBox.xaml
    /// </summary>
    public partial class LoadingBox : Window
    {
        /**<summary>Constructor for LoadingBox.</summary>
         * <param name="owner">The LoadingBox's owner window.</param>
         */
        public LoadingBox(Window owner)
        {
            InitializeComponent();
            Owner = owner;
        }
    }
}
