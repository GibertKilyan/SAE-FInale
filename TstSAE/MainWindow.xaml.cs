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
        //Variable Image//
        private BitmapImage monde, menu;
        private BitmapImage[] bobDroiteMarteau, bobGaucheMarteau, spikeManImages, abeillesImages, marteauImages;
        private BitmapImage bobAccroupiDroite, bobAccroupiGauche;
        private Image[] lesVies;
        private Image bob, spikeM, abeilleHaut, marteau;
        private List<Image> lesSpikeMan = new List<Image>();
        private List<Image> lesAbeillesHaut = new List<Image>();

        //Timer//
        private DispatcherTimer minuteurJeu, chronoJeu, tempsAttente, tempsAccroupi, timerAcceleration;

        //Indices pour animation//
        private int indiceSpikeMan, indiceBob, indiceAccroupi, indiceMarteau;

        //Indices qui vont avec les Timer//
        private int tmps = 0, indiceCooldown;

        //indice collision//
        private int nbScore = 0, nbVie = 3;

        //Nombre Ennemis//
        private int nbSpikeMan = 3, nbHautAbeilles = 2;

        //booléen//
        private bool gauche = false, droite = false, accroupi = false, enDeplacement = false, regardDroite = false;
        private bool cooldown = false, pause = false;
        private bool lancer = false, deplacementMarteau = false;

        //Vitesse//
        public static readonly int PASDEBOB = 4, PASMARTEAU = 7, PASSPIKEMAN = 3, PASABEILLE = 5;
        public static readonly double incrementVitesse = 0.5;
        private double vitesseSpikeMan = 1;

        //Random//
        private static Random alea;

        //variable pour que le menu potion puisse s'ouvrir et se fermer à l'infini//
        public static int OPTION;
                   
        //musique//
        private static MediaPlayer musique;

        //position monde//
        public static readonly int HAUTEURBOBMONDE = 440, HAUTEURSPIKEMAN = 257;

        public MainWindow()
        {
            monde = new BitmapImage(new Uri("pack://application:,,,/background/monde1.png"));
            menu = new BitmapImage(new Uri("pack://application:,,,/background/fond_départ.png"));
            bobAccroupiDroite = new BitmapImage(new Uri("pack://application:,,,/Bob/accroupi/bob_droite_accroupi_marteau.png"));
            bobAccroupiGauche = new BitmapImage(new Uri("pack://application:,,,/Bob/accroupi/bob_gauche_accroupi_marteau.png"));
            alea = new Random();

            InitializeComponent();
            InitBobImage();
            InitMarteauImage();
            InitImageEnnemis();
            InitMusique();
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
            }
        
        }
        private void InitMusique()

        {
            {
                musique = new MediaPlayer();
                musique.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory +
               "Son/musique-jeu.mp3"));
                
                musique.Volume = 100;
                musique.Play();
            }
        }
        private void RelanceMusique(object? sender, EventArgs e)
        {
            musique.Position = TimeSpan.Zero;
            musique.Play();
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
            chronoAccroupi();
            TimerCooldown();
            tempsEnJeu();
            initVie();
            jeuTimer();
            InitialiserTimerAcceleration();
            

            Image monde1 = new Image();
            monde1.Source = monde;
            monde1.Width = monde.Width;
            monde1.Height = monde.Height;

            CanvaFond.Background = new ImageBrush(monde);

            bob = new Image();
            bob.Source = bobDroiteMarteau[0];
            bob.Width = bobDroiteMarteau[0].Width;
            bob.Height = bobDroiteMarteau[0].Height;

            CanvaFond.Children.Add(bob);
            Canvas.SetLeft(bob, CanvaFond.ActualWidth / 2);
            Canvas.SetTop(bob, CanvaFond.ActualHeight - HAUTEURBOBMONDE);

            for (int i = 0; i < nbSpikeMan; i++)
            {
                spikeM = new Image();
                spikeM.Source = spikeManImages[0];
                spikeM.Width = spikeManImages[21].Width;
                spikeM.Height = spikeManImages[21].Height;

                lesSpikeMan.Add(spikeM);
                CanvaFond.Children.Add(spikeM);
                Canvas.SetLeft(spikeM, alea.Next(-1100, -100));
                Canvas.SetTop(spikeM, CanvaFond.ActualHeight - HAUTEURSPIKEMAN);
                
            }
            marteau = new Image();
            marteau.Source = marteauImages[0];
            marteau.Width = marteauImages[0].Width;
            marteau.Height = marteauImages[0].Height;

            CanvaFond.Children.Add(marteau);
            Canvas.SetLeft(marteau, 600);
            Canvas.SetTop(marteau, CanvaFond.ActualHeight - HAUTEURBOBMONDE);
            marteau.Visibility = Visibility.Hidden;

            for (int i = 0; i < nbHautAbeilles; i++)
            {
                abeilleHaut = new Image();
                abeilleHaut.Source = abeillesImages[0];
                abeilleHaut.Width = abeillesImages[0].Width;
                abeilleHaut.Height = abeillesImages[0].Height;

                lesAbeillesHaut.Add(abeilleHaut);
                CanvaFond.Children.Add(abeilleHaut);
                Canvas.SetLeft(abeilleHaut, alea.Next(-1000, 2200));
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

            abeillesImages = new BitmapImage[1];
            abeillesImages[0] = new BitmapImage(new Uri("pack://application:,,,/ennemis/abeille_haut.png"));
        }
        //initialisation vie//
        private void initVie()
        {
            lesVies = new Image[3];
            lesVies[0] = vie1;
            lesVies[1] = vie2;
            lesVies[2] = vie3;
        }

        //initialisation marteau//
        private void InitMarteauImage()
        {          
            marteauImages = new BitmapImage[4];
            for (int i = 0; i < 4; i++)
            {
                marteauImages[i] = new BitmapImage(new Uri($"pack://application:,,,/Bob/marteau_gauche/marteau_inv{i}.png"));
            }
        }

        //timer//
        private void jeuTimer()
        {
            minuteurJeu = new DispatcherTimer();
            minuteurJeu.Interval = TimeSpan.FromMilliseconds(30);
            minuteurJeu.Tick += jeu;
            minuteurJeu.Start();
        }
        //timer spike man// 
        private void InitialiserTimerAcceleration()
        {
            timerAcceleration = new DispatcherTimer();
            timerAcceleration.Interval = TimeSpan.FromSeconds(10);
            timerAcceleration.Tick += acceleration;
            timerAcceleration.Start();
        }
        private void acceleration(object? sender, EventArgs e)
        {
            vitesseSpikeMan += incrementVitesse;
        }
        //chrono//
        private void tempsEnJeu()
        {
            chronoJeu = new DispatcherTimer();
            chronoJeu.Interval = TimeSpan.FromSeconds(1);
            chronoJeu.Tick += chrono;
            chronoJeu.Start();
        }
        private void chrono(object? sender, EventArgs e)
        {
            blockTemps.Text = "Temps : " + TimeSpan.FromSeconds(tmps);
            tmps += 1;
        }
        private void chronoAccroupi()
        {
            tempsAccroupi = new DispatcherTimer();
            tempsAccroupi.Interval = TimeSpan.FromMilliseconds(300);
            tempsAccroupi.Tick += arretAccroupi;
            tempsAccroupi.Start();
        }
        private void arretAccroupi(object? sender, EventArgs e)
        {
            indiceAccroupi += 1;
            if (indiceAccroupi == 1)
                indiceAccroupi = 0;
            accroupi = false;
        }
        private void TimerCooldown()
        {
            tempsAttente = new DispatcherTimer();
            tempsAttente.Interval = TimeSpan.FromMilliseconds(500);
            tempsAttente.Tick += TempsAttente;
            tempsAttente.Start();
        }
        private void TempsAttente(object? sender, EventArgs e)
        {
            indiceCooldown += 1;
            if (indiceCooldown == 1)
                indiceCooldown = 0;
            cooldown = false;
        }
        

    //ennemis vers joueur//
    private void DeplacerAbeilleVersBob(Image ennemi)
        {
            double posAbeilleX = Canvas.GetLeft(ennemi);
            double posAbeilleY = Canvas.GetTop(ennemi);
            double posBobX = Canvas.GetLeft(bob);
            double posBobY = Canvas.GetTop(bob);

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
            double posBob = Canvas.GetLeft(bob);
            double posSpike = Canvas.GetLeft(spikeM);
            double newPosBob = posBob;
            double newPosSpike = posSpike;

            System.Drawing.Rectangle RBob = new System.Drawing.Rectangle((int)Canvas.GetLeft(bob) + 70,
            (int)Canvas.GetTop(bob),
            (int)bob.Width,
            (int)bob.Height);

            if (accroupi == true)
            {
                enDeplacement = false;
                if (regardDroite == true)
                    bob.Source = bobAccroupiDroite;
                if (regardDroite == false)
                    bob.Source = bobAccroupiGauche;
                else
                    bob.Source = bobAccroupiDroite;
            }
            if (accroupi == false)
            {
                enDeplacement = false;
                if (regardDroite == true)
                    bob.Source = bobDroiteMarteau[indiceBob];
                if (regardDroite == false)
                    bob.Source = bobGaucheMarteau[indiceBob];
                else
                    bob.Source = bobDroiteMarteau[indiceBob];
            }

            if (gauche == true && (posBob + PASDEBOB > 0) && pause == false )
             {
                 enDeplacement = true;
                 indiceBob++;
                 if (indiceBob == 7)
                 {
                     indiceBob = 0;
                 }
                 bob.Source = bobGaucheMarteau[indiceBob];
                 newPosBob = posBob - PASDEBOB;
             }

             if (droite == true && (posBob + PASDEBOB) < (CanvaFond.ActualWidth - bob.ActualWidth) && pause == false)
             {
                 enDeplacement = true;
                 indiceBob++;
                 if (indiceBob == 7)
                 {
                    indiceBob = 0;
                 }
                 bob.Source = bobDroiteMarteau[indiceBob];
                 newPosBob = posBob + PASDEBOB;
             }
            Canvas.SetLeft(bob, newPosBob);

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
                marteau.Source = marteauImages[indiceMarteau];
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

                double newPosEnnemi = posEnnemi + PASSPIKEMAN*vitesseSpikeMan;
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
                        lesVies[nbVie - 1].Visibility = Visibility.Hidden;
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
                    Canvas.SetLeft(marteau, Canvas.GetLeft(bob));
                    lancer = false;
                    deplacementMarteau = false;
                    marteau.Visibility = Visibility.Hidden;
                    nbScore = nbScore + 1;
                    blockScore.Text = "Score : " + nbScore;
                    if (nbScore % 10 == 0 && nbScore > 0)
                    {
                        vitesseSpikeMan += incrementVitesse;
                    }

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
                    Canvas.SetLeft(lesAbeillesHaut[i], alea.Next(-1000, 2200));
                    if (nbVie >= 1)
                    {
                        lesVies[nbVie - 1].Visibility = Visibility.Hidden;
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
                    Canvas.SetLeft(lesAbeillesHaut[i], alea.Next(-1000, 2200));                    
                    nbScore = nbScore + 1;
                    blockScore.Text = "Score : " + nbScore;
                }
                if (accroupi == false)
                {
                    tempsAccroupi.Stop();
                }
                if (cooldown == false)
                {
                    tempsAttente.Stop();
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
            
                if (e.Key == Key.Q && pause == false)
                {
                    gauche = true;
                    enDeplacement = true;
                    regardDroite = false;
                    accroupi = false;
                }

                if (e.Key == Key.D && pause == false)
                {
                    droite = true;
                    enDeplacement = true;
                    regardDroite = true;
                    accroupi = false;
                }

                if (e.Key == Key.Space && cooldown == false && pause == false)
                {
                    accroupi = true; 
                    cooldown = true;
                    tempsAttente.Start();
                    tempsAccroupi.Start();
                }

                if (e.Key == Key.Escape)
                {
                    if (pause == true)
                    {
                        minuteurJeu.Start();
                        chronoJeu.Start();
                        pause = false;
                    }
                    else if (pause == false)
                    {
                        minuteurJeu.Stop();
                        chronoJeu.Stop();
                        pause = true;
                    }
                }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && !deplacementMarteau && regardDroite == false && pause == false)
            {

                lancer = true;
                deplacementMarteau = true;
                marteau.Visibility = Visibility.Visible;
                Canvas.SetTop(marteau, Canvas.GetTop(bob) + bob.ActualWidth - 20);
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

                minuteurJeu.Stop();
                chronoJeu.Stop();
                CanvaFond.Children.Clear();

                nbVie = 3;
                nbScore = 0;
                vitesseSpikeMan = 0;
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