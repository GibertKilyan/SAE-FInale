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
        MainWindow Jeu;
        public Menu(MainWindow Jeu)
        {
            InitializeComponent();
            this.Jeu = Jeu;
        }

        private void butRegleJeu_Click(object sender, RoutedEventArgs e)
        {
            MainWindow.REGLEDUJEU = MainWindow.REGLEDUJEU - 1;
            this.DialogResult = false;
        }

        private void butJouer_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
        }
        
        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            FenetreAudio dialog = new FenetreAudio(Jeu.musique.Volume);
            bool? result = dialog.ShowDialog();

            if (result == true)
            {
                // Mettre à jour le volume à partir de la boîte de dialogue
                Jeu.musique.Volume = dialog.VolumeValue;
                MessageBox.Show($"Volume réglé ", "Volume", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
