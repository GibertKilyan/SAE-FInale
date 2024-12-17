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

namespace TstSAE
{
    /// <summary>
    /// Logique d'interaction pour Menu.xaml
    /// </summary>
    public partial class Menu : Window
    {
        bool lancer;
        public Menu()
        {
            InitializeComponent();
        }

        private void butRegleJeu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.OPTION = MainWindow.OPTION - 1;
            this.DialogResult = false;
        }

        private void butMonde1_Click(object sender, RoutedEventArgs e)
        {
           this.DialogResult = true;
        }
    }
}
