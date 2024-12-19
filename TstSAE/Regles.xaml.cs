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
    /// Logique d'interaction pour Regles.xaml
    /// </summary>
    public partial class Regles : Window
    {
        public Regles()
        {
            InitializeComponent();
        }

        private void ButOk_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            MainWindow.REGLEDUJEU = false; // remet la variable a false pour recommencer a l'infini a affachier les règles du jeu
        }
    }
}
