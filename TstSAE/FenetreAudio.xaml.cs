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
    /// Logique d'interaction pour FenetreAudio.xaml
    /// </summary>
    public partial class FenetreAudio : Window

    {      
        private MediaPlayer mediaPlayer;
        public double VolumeValue;
        public FenetreAudio(double VolumeBase)
        {
            InitializeComponent();
            VolumeSlider.Value = VolumeBase;
            VolumeSlider.ValueChanged += VolumeSlider_ValueChanged;
        }

        private void VolumeSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            VolumeValue = VolumeSlider.Value;
        }

        private void ButOK_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;            
        }

        private void ButFermer_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; // Annuler le dialogue
           
        }
    }
}
    

