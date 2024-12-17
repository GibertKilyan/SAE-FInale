using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace TstSAE
{
    public partial class MainWindow : Window
    {
        public static BitmapImage MD1, MD2, accueil;
        public DispatcherTimer minuteur;
        private static Random alea;
        public static int OPTION {  get; set; }
        
        
        //variable//
        public static readonly double PASDEBOB = 4, PASSPIKEMAN = 3, PASABEILLE = 5;
        BitmapImage[] bobDroiteMarteau, bobGaucheMarteau;

        //pause//
        bool pause = false;

        //temps dans le jeu//
        public DispatcherTimer temps;
        int tmps = 0;

        //Bob//
        public static BitmapImage bobAccroupiDroite, bobAccroupiGauche;
        Image Bob;
        bool gauche, droite, accroupi = false, enDeplacement = false, regardDroite = false;
        int indiceBob;

        //spikeMan//
        int indiceSpikeMan;
        BitmapImage[] spikeManImages;
        int nbSpikeMan = 3;
        List<Image> lesSpikeMan = new List<Image>();
        double[] positionInitialSpikeMan;
        Image spikeM;

        //Abeille haut//
        int nbHautAbeilles = 2;
        Image abeilleHaut;
        List<Image> lesAbeillesHaut = new List<Image>();
        BitmapImage[] abeillesImages;

        //marteau//
        public BitmapImage[] Marteaugauche;
        bool lancer = false, deplacementMarteau = false;
        int indiceMarteau;
        public static readonly double PASMARTEAU = 7;
        Image marteau;

        //score//
        int nbScore = 0;

        //vie//
        int nbVie = 3;
        Image[] lesvies;

        //position monde 1//
        public static readonly int HAUTEURBOBMONDE1 = 418, MILIEUMONDE1 = 600, HAUTEURSPIKEMAN = 240, HAUTEURABEILLE = 440, SPAWNENNEMIS = 200;

        //position monde 2//
        public static readonly int HAUTEURBOBMONDE2 = 470, MILIEUMONDE2 = 600;
        public MainWindow()
        {
            MD1 = new BitmapImage(new Uri("pack://application:,,,/background/monde1.png"));
            MD2 = new BitmapImage(new Uri("pack://application:,,,/background/monde2.jpg"));
            accueil = new BitmapImage(new Uri("pack://application:,,,/background/fond_départ.png"));
            bobAccroupiDroite = new BitmapImage(new Uri("pack://application:,,,/Bob/accroupi/bob_droite_accroupi_marteau.png"));
            bobAccroupiGauche = new BitmapImage(new Uri("pack://application:,,,/Bob/accroupi/bob_gauche_accroupi_marteau.png"));
            alea = new Random();

            InitializeComponent();
            InitBobImage();
            InitMarteauImage();
            InitImageEnnemis();
        }
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Menu();
        }
        private void Menu()
        {
            this.Hide();
            Menu Menu = new Menu();
            bool? result = Menu.ShowDialog();
            if (result == true)
            {
                Monde1();
                this.Show();
            }
            else
            {
                if (OPTION == -1)
                    regles();
                if (OPTION == 0)
                {
                    Application.Current.Shutdown();
                }
            }
        }
        private void regles()
        {
            Regles Regles = new Regles();
            bool? regles = Regles.ShowDialog();
            if (regles == true)
                Menu();
            if (regles == false)
            {
                OPTION = 0;
                Menu();
            }
        }

        //initialisation des mondes//
        public void Monde1()
        {
            tempsEnJeu();
            initVie();
            jeuTimer();

            Image monde1 = new Image();
            monde1.Source = MD1;
            monde1.Width = MD1.Width;
            monde1.Height = MD1.Height;

            CanvaFond.Background = new ImageBrush(MD1);

            Bob = new Image();
            Bob.Source = bobDroiteMarteau[0];
            Bob.Width = bobDroiteMarteau[0].Width;
            Bob.Height = bobDroiteMarteau[0].Height;

            CanvaFond.Children.Add(Bob);
            Canvas.SetLeft(Bob, MILIEUMONDE1);
            Canvas.SetTop(Bob, CanvaFond.ActualHeight - HAUTEURBOBMONDE1);

            positionInitialSpikeMan = new double[nbSpikeMan];
            for (int i = 0; i < nbSpikeMan; i++)
            {
                spikeM = new Image();
                spikeM.Source = spikeManImages[0];
                spikeM.Width = spikeManImages[21].Width;
                spikeM.Height = spikeManImages[21].Height;

                double nouveauX;

                do
                {
                    nouveauX = alea.Next(-1000, -100);
                }
                while (PositionValide(nouveauX, lesSpikeMan, spikeM.Width));

                lesSpikeMan.Add(spikeM);
                CanvaFond.Children.Add(spikeM);
                Canvas.SetLeft(spikeM, nouveauX);
                Canvas.SetTop(spikeM, CanvaFond.ActualHeight - HAUTEURSPIKEMAN);
                
            }
            marteau = new Image();
            marteau.Source = Marteaugauche[0];
            marteau.Width = Marteaugauche[0].Width;
            marteau.Height = Marteaugauche[0].Height;

            CanvaFond.Children.Add(marteau);
            Canvas.SetLeft(marteau, MILIEUMONDE1);
            Canvas.SetTop(marteau, CanvaFond.ActualHeight - HAUTEURBOBMONDE1);
            marteau.Visibility = Visibility.Hidden;

            for (int i = 0; i < nbHautAbeilles; i++)
            {
                abeilleHaut = new Image();
                abeilleHaut.Source = abeillesImages[0];
                abeilleHaut.Width = abeillesImages[0].Width;
                abeilleHaut.Height = abeillesImages[0].Height;

                double nouveauX;

                do
                {
                    nouveauX = alea.Next(0, (int)CanvaFond.ActualWidth - (int)abeilleHaut.Width);
                }
                while (PositionValide(nouveauX, lesAbeillesHaut, abeilleHaut.Width));

                lesAbeillesHaut.Add(abeilleHaut);
                CanvaFond.Children.Add(abeilleHaut);
                Canvas.SetLeft(abeilleHaut, nouveauX);
                Canvas.SetTop(abeilleHaut, alea.Next(-300, 0));
            }
        }
        //initialisation image de bob//
        private void InitBobImage()
        {
            bobDroiteMarteau = new BitmapImage[7];
            for (int i = 0; i < 7; i++)

            {
                bobDroiteMarteau[i] = new BitmapImage(new Uri($"pack://application:,,,/BOB/Bob_droite/bob_droite_marteau{i + 1}.png"));
            }

            bobGaucheMarteau = new BitmapImage[7];
            for (int i = 0; i < 7; i++)

            {
                bobGaucheMarteau[i] = new BitmapImage(new Uri($"pack://application:,,,/BOB/Bob_gauche/bob_gauche_marteau{i + 1}.png"));
            }
        }
        //initialisation ennemis//
        private void InitImageEnnemis()
        {
            spikeManImages = new BitmapImage[40];
            for (int i = 0; i < spikeManImages.Length; i++)
            {
                if (i <= 20)
                {
                    spikeManImages[i] = new BitmapImage(new Uri("pack://application:,,,/ennemis/spikeman_depart.png"));
                }
                else
                {
                    spikeManImages[i] = new BitmapImage(new Uri("pack://application:,,,/ennemis/spikeman_marche.png"));
                }
            }

            abeillesImages = new BitmapImage[40];
            for(int i = 0 ;i < abeillesImages.Length;i++)
            {
                if (i <= 20)
                {
                    abeillesImages[i] = new BitmapImage(new Uri("pack://application:,,,/ennemis/abeille_haut.png"));
                }
                else
                {
                    abeillesImages[i] = new BitmapImage(new Uri("pack://application:,,,/ennemis/abeille_bas.png"));
                }
            }
        }
        //verification du chevauchement ennemis//
        private bool PositionValide(double newX, List<Image> ennemis, double enemisWidth)
        {
            foreach (var ennemi in ennemis)
            {
                double xExistant = Canvas.GetLeft(ennemi);
                if (Math.Abs(xExistant - newX) < enemisWidth)
                {
                    return true;
                }
            }
            return false;
        }

        //initialisation vie//
        private void initVie()
        {
            lesvies = new Image[3];
            lesvies[0] = vie1;
            lesvies[1] = vie2;
            lesvies[2] = vie3;
        }

        //initialisation marteau//
        private void InitMarteauImage()
        {
            
            Marteaugauche = new BitmapImage[4];
            for (int i = 0; i < 4; i++)
            {
                Marteaugauche[i] = new BitmapImage(new Uri($"pack://application:,,,/Bob/marteau_gauche/marteau_inv{i}.png"));
            }
        }

        //timer//
        private void jeuTimer()
        {
            minuteur = new DispatcherTimer();
            minuteur.Interval = TimeSpan.FromMilliseconds(30);
            minuteur.Tick += jeu;
            minuteur.Start();
        }
        //chrono//
        private void tempsEnJeu()
        {
            temps = new DispatcherTimer();
            temps.Interval = TimeSpan.FromSeconds(1);
            temps.Tick += chrono;
            temps.Start();
        }
        private void chrono(object? sender, EventArgs e)
        {
            blockTemps.Text = "Temps : " + TimeSpan.FromSeconds(tmps);
            tmps += 1;
        }
        private void chronoAccroupi()
        {

        }
        //ennemis vers joueur//
        private void DeplacerAbeilleVersBob(Image ennemi)
        {
            double posAbeilleX = Canvas.GetLeft(ennemi);
            double posAbeilleY = Canvas.GetTop(ennemi);
            double posBobX = Canvas.GetLeft(Bob);
            double posBobY = Canvas.GetTop(Bob);

            // Calculer la direction vers Bob (X et Y)
            double directionX = posBobX - posAbeilleX;
            double directionY = posBobY - posAbeilleY;

            // Normaliser le vecteur de direction
            double distance = Math.Sqrt(directionX * directionX + directionY * directionY);
            double vitesseEnnemi = 8; // Définir la vitesse des ennemis

            if (distance > 0)
            {
                directionX /= distance;
                directionY /= distance;

                // Déplacer l'ennemi
                Canvas.SetLeft(ennemi, posAbeilleX + directionX * vitesseEnnemi);
                Canvas.SetTop(ennemi, posAbeilleY + directionY * vitesseEnnemi);
            }
        }

        //jeu//
        private void jeu(object? sender, EventArgs e)
        {
            double posBob = Canvas.GetLeft(Bob);
            double posSpike = Canvas.GetLeft(spikeM);
            double newPosBob = posBob;
            double newPosSpike = posSpike;

            System.Drawing.Rectangle RBob = new System.Drawing.Rectangle((int)Canvas.GetLeft(Bob) + 70,
            (int)Canvas.GetTop(Bob),
            (int)Bob.Width,
            (int)Bob.Height);

            if (accroupi == true)
            {
                enDeplacement = false;
                if (regardDroite == true)
                    Bob.Source = bobAccroupiDroite;
                if (regardDroite == false)
                    Bob.Source = bobAccroupiGauche;
                else
                    Bob.Source = bobAccroupiDroite;
            }
            if (accroupi == false)
            {
                enDeplacement = false;
                if (regardDroite == true)
                    Bob.Source = bobDroiteMarteau[indiceBob];
                if (regardDroite == false)
                    Bob.Source = bobGaucheMarteau[indiceBob];
                else
                    Bob.Source = bobDroiteMarteau[indiceBob];
            }

            if (gauche == true && (posBob + PASDEBOB > 0) && pause == false )
             {
                 enDeplacement = true;
                 indiceBob++;
                 if (indiceBob == 7)
                 {
                     indiceBob = 0;
                 }
                 Bob.Source = bobGaucheMarteau[indiceBob];
                 newPosBob = posBob - PASDEBOB;
             }

             if (droite == true && (posBob + PASDEBOB) < (CanvaFond.ActualWidth - Bob.ActualWidth) && pause == false)
             {
                 enDeplacement = true;
                 indiceBob++;
                 if (indiceBob == 7)
                 {
                    indiceBob = 0;
                 }
                 Bob.Source = bobDroiteMarteau[indiceBob];
                 newPosBob = posBob + PASDEBOB;
             }
            Canvas.SetLeft(Bob, newPosBob);

            double posmart = Canvas.GetLeft(marteau);
            double newposmart = posmart;

            System.Drawing.Rectangle rMarteau = new System.Drawing.Rectangle((int)Canvas.GetLeft(marteau) + 70,
            (int)Canvas.GetTop(marteau),
            (int)marteau.Width,
            (int)marteau.Height);

            if (lancer == true)
            {
                marteau.Visibility = Visibility.Visible;
                deplacementMarteau = true;
                indiceMarteau++;
                if (indiceMarteau == 4)
                {
                    indiceMarteau = 0;
                }
                marteau.Source = Marteaugauche[indiceMarteau];
                newposmart = posmart - PASMARTEAU;
                Canvas.SetLeft(marteau, newposmart);

                if (Canvas.GetLeft(marteau) > CanvaFond.ActualWidth || Canvas.GetLeft(marteau) + marteau.Width < 0)
                {
                    lancer = false;
                    deplacementMarteau = false;
                    marteau.Visibility = Visibility.Hidden;
                }
            }

            for (int i = 0; i < lesSpikeMan.Count; i++)
            {
                double posEnnemi = Canvas.GetLeft(lesSpikeMan[i]);

                indiceSpikeMan++;
                if (indiceSpikeMan == 40)
                {
                    indiceSpikeMan = 0;
                }
                lesSpikeMan[i].Source = spikeManImages[indiceSpikeMan];

                double newPosEnnemi = posEnnemi + PASSPIKEMAN;
                Canvas.SetLeft(lesSpikeMan[i], newPosEnnemi);

                if (Canvas.GetLeft(lesSpikeMan[i]) > CanvaFond.ActualWidth)
                {
                    Canvas.SetLeft(lesSpikeMan[i], alea.Next(-1000, -100));
                }
                System.Drawing.Rectangle rSpikeMan = new System.Drawing.Rectangle((int)Canvas.GetLeft(lesSpikeMan[i]),
                (int)Canvas.GetTop(lesSpikeMan[i]),
                (int)lesSpikeMan[i].Width,
                (int)lesSpikeMan[i].Height);


                if (rSpikeMan.IntersectsWith(RBob))
                {
                    Canvas.SetLeft(lesSpikeMan[i], alea.Next(-1000, -100));
                    if (nbVie >= 1)
                    {
                        lesvies[nbVie - 1].Visibility = Visibility.Hidden;
                        nbVie--;
                    }
                    else
                    {
                        finDuJeuMonde1();
                    }
                }

                if (rSpikeMan.IntersectsWith(rMarteau))
                {
                    Canvas.SetLeft(lesSpikeMan[i], alea.Next(-1000, -100));
                    Canvas.SetLeft(marteau, Canvas.GetLeft(Bob));
                    lancer = false;
                    deplacementMarteau = false;
                    marteau.Visibility = Visibility.Hidden;
                    nbScore = nbScore + 1;
                    blockScore.Text = "Score : " + nbScore;
                }
            }

            for (int i = 0; i < lesAbeillesHaut.Count; i++)
            {
                DeplacerAbeilleVersBob(lesAbeillesHaut[i]);

                System.Drawing.Rectangle rAbeilleHaut = new System.Drawing.Rectangle((int)Canvas.GetLeft(lesAbeillesHaut[i]),
                (int)Canvas.GetTop(lesAbeillesHaut[i]) - 120,
                (int)lesAbeillesHaut[i].Width,
                (int)lesAbeillesHaut[i].Height);

                if (rAbeilleHaut.IntersectsWith(RBob) && accroupi == false)
                {
                    Canvas.SetTop(lesAbeillesHaut[i], alea.Next(-300, 0));
                    Canvas.SetLeft(lesAbeillesHaut[i], alea.Next(-1000, -100));
                    if (nbVie >= 1)
                    {
                        lesvies[nbVie - 1].Visibility = Visibility.Hidden;
                        nbVie--;
                    }
                    else
                    {
                        finDuJeuMonde1();
                    }
                }
                if (rAbeilleHaut.IntersectsWith(RBob) && accroupi == true)
                {
                    Canvas.SetTop(lesAbeillesHaut[i], -40);
                    Canvas.SetLeft(lesAbeillesHaut[i], alea.Next(-1000, -100));
                    Canvas.SetTop(marteau, Canvas.GetTop(Bob) + Bob.ActualWidth - 20);
                    nbScore = nbScore + 1;
                    blockScore.Text = "Score : " + nbScore;
                }
            }
        }
        //bouton//
        private void Window_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Q)
            {
                gauche = false;
                enDeplacement = false;
            }

            if (e.Key == Key.D)
            {
                droite = false;
                enDeplacement = false;
            }
            
        }
        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            
                if (e.Key == Key.Q)
                {
                    gauche = true;
                    enDeplacement = true;
                    regardDroite = false;
                    accroupi = false;
                }

                if (e.Key == Key.D)
                {
                    droite = true;
                    enDeplacement = true;
                    regardDroite = true;
                    accroupi = false;
                }

                if (e.Key == Key.Space)
                {
                    accroupi = !accroupi; 
                }

                if (e.Key == Key.Escape)
                {
                    if (pause == true)
                    {
                        minuteur.Start();
                        temps.Start();
                        pause = false;
                    }
                    else if (pause == false)
                    {
                        minuteur.Stop();
                        temps.Stop();
                        pause = true;
                    }
                }

            if (e.Key == Key.F && !deplacementMarteau)
            {
                
                lancer = true;
                deplacementMarteau = true;
                marteau.Visibility = Visibility.Visible; 
                 
                 
            }
        }
            //fin du jeu//
        private void finDuJeuMonde1()
        {
            pause = true;
            MessageBoxResult result;
            result = MessageBox.Show("Vous avez perdu, voulez vous rejouer ? ", "rejouer", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                vie1.Visibility = Visibility.Visible;
                vie2.Visibility = Visibility.Visible;
                vie3.Visibility = Visibility.Visible;

                minuteur.Stop();
                temps.Stop();
                CanvaFond.Children.Clear();

                nbVie = 3;
                nbScore = 0;
                tmps = 0;
                blockTemps.Text = "Temps : " + TimeSpan.FromSeconds(tmps);
                blockScore.Text = "Score : " + nbScore;

                droite = false;
                gauche = false;
                pause = false;
                accroupi = false;
                lesSpikeMan.Clear();
                lesAbeillesHaut.Clear();
                Monde1();
            }
            if (result == MessageBoxResult.No)
            {
                this.Close();
            }
        }
    }
}