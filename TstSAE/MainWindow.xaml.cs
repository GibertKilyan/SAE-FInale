using System.ComponentModel;
using System.Diagnostics;
using System.Media;
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
        private BitmapImage mondeJeu, fondDepart, abeillesImage;
        private BitmapImage[] bobDroiteMarteau, bobGaucheMarteau, spikeManImages, marteauImages;
        private BitmapImage bobAccroupiDroite, bobAccroupiGauche;
        private Image[] lesBoucliers;
        private Image bob, spikeM, abeilleHaut, marteau, monde;
        private List<Image> lesSpikeMan = new List<Image>();
        private List<Image> lesAbeillesHaut = new List<Image>();

        //Timer//
        private DispatcherTimer minuteurJeu, chronoJeu, tempsAttente, tempsAccroupi, timerAcceleration;

        //Indices pour animation//
        private int indiceSpikeMan, indiceBob, indiceMarteau;
        private int indiceBobMax = 7, indiceMarteauMax = 4, indiceSpikeManMax = 40;
        bool indiceAccroupi = false;

        //Indices qui vont avec les Timer//
        private int tmps = 0, indiceCooldown;

        //indice collision//
        private int nbScore = 0, nbBouclier = 3;

        //Nombre Ennemis//
        private int nbSpikeMan = 3, nbHautAbeilles = 2;

        //booléen//
        private bool gauche = false, droite = false, accroupi = false, enDeplacement = false, regardDroite = false;
        private bool cooldown = false, pause = false;
        private bool lancer = false, deplacementMarteau = false;

        //Vitesse//
        public static readonly int PASDEBOB = 5, PASMARTEAU = 8;
        public static readonly double incrementVitesse = 1;
        private double vitesseSpikeMan = 3;
        double vitesseEnnemiMax = 8, vitesseAbeille = 8;

        //HitBox//
        public static readonly int HITBOXMARTEAUGAUCHE = 70, HITBOXABEILLEHAUT = 120, MARTEAUHITBOXBOB = 20;

        //Random//
        private static Random alea;

        //variable pour que le menu potion puisse s'ouvrir et se fermer à l'infini//
        public static int REGLEDUJEU;
                   
        //musique//
        private static MediaPlayer musique;
        private static MediaPlayer sonDegats;
        //position monde//
        public static readonly int HAUTEURBOBMONDE = 440, HAUTEURSPIKEMAN = 257, MARTEAUHORSCANVA = 3000;
        public static readonly int HAUTEURALEATOIRE = -300, GAUCHEDUCANVAALEATOIRE = -1000, GAUCHECANVAALEATOIRE2 = -100,DROITEDUCANVAALEATOIRE = 2200, HAUTCANVA = 0;

        //Autre variable//
        private int resteBouclier = 1;

        public MainWindow()
        {
            //initialisation des images qui n'ont pas besoin d'animation//
            mondeJeu = new BitmapImage(new Uri("pack://application:,,,/background/monde1.png"));
            fondDepart = new BitmapImage(new Uri("pack://application:,,,/background/fond_départ.png"));
            bobAccroupiDroite = new BitmapImage(new Uri("pack://application:,,,/Bob/accroupi/bob_droite_accroupi_marteau.png"));
            bobAccroupiGauche = new BitmapImage(new Uri("pack://application:,,,/Bob/accroupi/bob_gauche_accroupi_marteau.png"));
            abeillesImage = new BitmapImage(new Uri("pack://application:,,,/ennemis/abeille_haut.png"));
            alea = new Random();

            InitializeComponent();
            InitBobImage();
            InitImageEnnemis();
            InitMusique();
            InitMarteauImage();
        }
        
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //charge la fonction Menu qui correspond a la window Menu//
            FenetreDemarrage();
        }
        
        //Menu//
        private void FenetreDemarrage()
        {
            this.Hide();
            Menu Menu = new Menu();
            bool? result = Menu.ShowDialog();

            if (result == true) //si dans le menu la fonction monde est appuyer alors charge le monde 1
            {
                InitialisationMonde();
                this.Show();
            }
            else // si le bouton règle du jeu est appuyer alors charge les règle du jeu
            {
                if (REGLEDUJEU == -1)
                    Regles();
            }
        
        }
        
        private void Regles()
        {
            Regles Regles = new Regles();
            bool? regles = Regles.ShowDialog();
            if (regles == true)
                FenetreDemarrage();
            if (regles == false)
            {
                REGLEDUJEU = 0; // recharge le menu quand le bouton ok est cliquer
                FenetreDemarrage();
            }
        }

        //musique//
        private void InitMusique()

        {
            {
                musique = new MediaPlayer();
                musique.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory +
               "Son/musique-jeu.mp3"));

                musique.Volume = 1;
                musique.Play();
            }
        }
        //son de degat//
        private void InitSon()
        {
            sonDegats = new MediaPlayer();
            sonDegats.Open(new Uri(AppDomain.CurrentDomain.BaseDirectory +
           "Son/sondegat.mp3"));
            sonDegats.Volume = 1;
            sonDegats.Play();

        }
        

        //initialisation du monde//
        public void InitialisationMonde()
        {
            chronoAccroupi();
            TimerCooldown();
            tempsEnJeu();
            initVie();
            jeuTimer();
            InitialiserTimerAcceleration();

            //image de fond du monde
            monde = new Image();
            monde.Source = mondeJeu;
            monde.Width = mondeJeu.Width;
            monde.Height = mondeJeu.Height;
            CanvaFond.Background = new ImageBrush(mondeJeu);

            //image de bob + place sur le canva 
            bob = new Image();
            bob.Source = bobDroiteMarteau[0];
            bob.Width = bobDroiteMarteau[0].Width;
            bob.Height = bobDroiteMarteau[0].Height;
            CanvaFond.Children.Add(bob);
            Canvas.SetLeft(bob, CanvaFond.ActualWidth / 2); //bob au milieu du canva 
            Canvas.SetTop(bob, CanvaFond.ActualHeight - HAUTEURBOBMONDE);

            //images de spikeman + place sur le canva 
            for (int i = 0; i < nbSpikeMan; i++)
            {
                spikeM = new Image();
                spikeM.Source = spikeManImages[0];
                spikeM.Width = spikeManImages[21].Width;
                spikeM.Height = spikeManImages[21].Height;

                lesSpikeMan.Add(spikeM);
                CanvaFond.Children.Add(spikeM);
                Canvas.SetLeft(spikeM, alea.Next(GAUCHEDUCANVAALEATOIRE, GAUCHECANVAALEATOIRE2));
                Canvas.SetTop(spikeM, CanvaFond.ActualHeight - HAUTEURSPIKEMAN);
                
            }
            //image du marteau + place sur le canva qui sera changer plus tard au moment du lancer + cacher la marteau
            marteau = new Image();
            marteau.Source = marteauImages[0];
            marteau.Width = marteauImages[0].Width;
            marteau.Height = marteauImages[0].Height;
            CanvaFond.Children.Add(marteau);
            marteau.Visibility = Visibility.Hidden;

            //images abeilles qui viennent du ciel + place sur la canva 
            for (int i = 0; i < nbHautAbeilles; i++)
            {
                abeilleHaut = new Image();
                abeilleHaut.Source = abeillesImage;
                abeilleHaut.Width = abeillesImage.Width;
                abeilleHaut.Height = abeillesImage.Height;

                lesAbeillesHaut.Add(abeilleHaut);
                CanvaFond.Children.Add(abeilleHaut);
                Canvas.SetLeft(abeilleHaut, alea.Next(GAUCHEDUCANVAALEATOIRE, DROITEDUCANVAALEATOIRE)); //placer a gauche aléatoirement entre 1000 avant le canva et 1000 après
                Canvas.SetTop(abeilleHaut, alea.Next(HAUTEURALEATOIRE, HAUTCANVA)); //placer en haut aléatoirement entre le haut du canva et -300 au dessus du canva
            }
        }
        //initialisation image de bob pour animations de marche//
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
                //on stock ici 20 fois la même image dans les 20première case puis 20 fois une autre l'objectif est de faire une animation mais pas top rapide dans le jeu
                //seul chose du code qui n'est normalement pas optimisé 
                if (i <= 20)
                {
                    spikeManImages[i] = new BitmapImage(new Uri("pack://application:,,,/ennemis/spikeman_depart.png"));
                }
                else
                {
                    spikeManImages[i] = new BitmapImage(new Uri("pack://application:,,,/ennemis/spikeman_marche.png"));
                }
            }
        }

        //initialisation vie//
        private void initVie()
        {
            //stock les 3vie présent sur la mainWindow dans un tableau
            lesBoucliers = new Image[3];
            lesBoucliers[0] = bouclier1;
            lesBoucliers[1] = bouclier2;
            lesBoucliers[2] = bouclier3;
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

        //timer pour le jeu (déplacement bob, ennemis...//
        private void jeuTimer()
        {
            minuteurJeu = new DispatcherTimer();
            minuteurJeu.Interval = TimeSpan.FromMilliseconds(16);
            minuteurJeu.Tick += jeu;
            minuteurJeu.Start();
        }

        //timer spikeman pour augmenter la vitesse au fil du temps// 
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
            if (vitesseSpikeMan == vitesseEnnemiMax)
                timerAcceleration.Stop();
        }

        //chrono afficher en haut a gauche//
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

        //Ce chrono est appeler quand on s'accroupie il fait en sorte de pouvoir rester accroupie maximum 300milisecond avant d'être automatiquement relevé//
        private void chronoAccroupi()
        {
            tempsAccroupi = new DispatcherTimer();
            tempsAccroupi.Interval = TimeSpan.FromMilliseconds(300);
            tempsAccroupi.Tick += arretAccroupi;
            tempsAccroupi.Start();
        }
        private void arretAccroupi(object? sender, EventArgs e)
        {
            indiceAccroupi = true ; 
            if (indiceAccroupi == true)
                indiceAccroupi = false;
            accroupi = false;
        }
        //Ce chrono est appeler quand on appuye sur la touche pour s'accroupir il met un cooldown sur la touche ce qui l'empeche de se reaccroupir directement et donc rester accroupie a l'infini
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

        //abeilles fonçannt vers Bob//
        private void DeplacerAbeilleVersBob(Image ennemi)
        {
            //récupère la position de l'ennemi
            double posAbeilleX = Canvas.GetLeft(ennemi);
            double posAbeilleY = Canvas.GetTop(ennemi);

            //récupère la position de Bob
            double posBobX = Canvas.GetLeft(bob);
            double posBobY = Canvas.GetTop(bob);

            // Calculer la direction vers Bob (X et Y)
            double directionX = posBobX - posAbeilleX;
            double directionY = posBobY - posAbeilleY;

            //Normalise le vecteur de direction
            double distance = Math.Sqrt(directionX * directionX + directionY * directionY);

            if (distance > 0)
            {
                directionX /= distance;
                directionY /= distance;

                // Déplacer l'ennemi
                Canvas.SetLeft(ennemi, posAbeilleX + directionX * vitesseAbeille);
                Canvas.SetTop(ennemi, posAbeilleY + directionY * vitesseAbeille);
            }
        }

        //jeu//
        private void jeu(object? sender, EventArgs e)
        {
            //Bob déplacement et rectangle
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
                 if (indiceBob == indiceBobMax)
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
                 if (indiceBob == indiceBobMax)
                 {
                    indiceBob = 0;
                 }
                 bob.Source = bobDroiteMarteau[indiceBob];
                 newPosBob = posBob + PASDEBOB;
             }
            Canvas.SetLeft(bob, newPosBob);

            //marteau
            double posmart = Canvas.GetLeft(marteau);
            double newposmart = posmart;

            System.Drawing.Rectangle rMarteau = new System.Drawing.Rectangle((int)Canvas.GetLeft(marteau) + HITBOXMARTEAUGAUCHE,
            (int)Canvas.GetTop(marteau),
            (int)marteau.Width,
            (int)marteau.Height);

            if (lancer == true)
            {
                marteau.Visibility = Visibility.Visible;
                deplacementMarteau = true;
                indiceMarteau++;
                if (indiceMarteau == indiceMarteauMax)
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

            //SpikeMan
            for (int i = 0; i < lesSpikeMan.Count; i++)
            {
                double posEnnemi = Canvas.GetLeft(lesSpikeMan[i]);

                indiceSpikeMan++;
                if (indiceSpikeMan == indiceSpikeManMax)
                {
                    indiceSpikeMan = 0;
                }
                lesSpikeMan[i].Source = spikeManImages[indiceSpikeMan];

                double newPosEnnemi = posEnnemi + vitesseSpikeMan;
                Canvas.SetLeft(lesSpikeMan[i], newPosEnnemi);

                if (Canvas.GetLeft(lesSpikeMan[i]) > CanvaFond.ActualWidth)
                {
                    Canvas.SetLeft(lesSpikeMan[i], alea.Next(GAUCHEDUCANVAALEATOIRE, GAUCHECANVAALEATOIRE2));
                }
                System.Drawing.Rectangle rSpikeMan = new System.Drawing.Rectangle((int)Canvas.GetLeft(lesSpikeMan[i]),
                (int)Canvas.GetTop(lesSpikeMan[i]),
                (int)lesSpikeMan[i].Width,
                (int)lesSpikeMan[i].Height);


                if (rSpikeMan.IntersectsWith(RBob))
                {
                    Canvas.SetLeft(lesSpikeMan[i], alea.Next(GAUCHEDUCANVAALEATOIRE, GAUCHECANVAALEATOIRE2));
                    if (nbBouclier >= resteBouclier)
                    {
                        lesBoucliers[nbBouclier - 1].Visibility = Visibility.Hidden;
                        nbBouclier--;
                        InitSon();
                    }
                    else
                    {
                        FinDuJeuMonde();
                    }
                }

                if (rSpikeMan.IntersectsWith(rMarteau))
                {
                    
                    Canvas.SetLeft(lesSpikeMan[i], alea.Next(GAUCHEDUCANVAALEATOIRE, GAUCHECANVAALEATOIRE2));
                    Canvas.SetLeft(marteau, MARTEAUHORSCANVA);
                    lancer = false;
                    deplacementMarteau = false;
                    marteau.Visibility = Visibility.Hidden;
                    nbScore = nbScore + 1;
                    blockScore.Text = "Score : " + nbScore;
                }
            }

            //abeilles 
            for (int i = 0; i < lesAbeillesHaut.Count; i++)
            {
                DeplacerAbeilleVersBob(lesAbeillesHaut[i]);

                System.Drawing.Rectangle rAbeilleHaut = new System.Drawing.Rectangle((int)Canvas.GetLeft(lesAbeillesHaut[i]),
                (int)Canvas.GetTop(lesAbeillesHaut[i]) - HITBOXABEILLEHAUT,
                (int)lesAbeillesHaut[i].Width,
                (int)lesAbeillesHaut[i].Height);

                if (rAbeilleHaut.IntersectsWith(RBob) && accroupi == false)
                {
                    Canvas.SetTop(lesAbeillesHaut[i], alea.Next(-300, 0));
                    Canvas.SetLeft(lesAbeillesHaut[i], alea.Next(-1000, 2200));
                    if (nbBouclier >= 1)
                    {
                        lesBoucliers[nbBouclier - 1].Visibility = Visibility.Hidden;
                        nbBouclier--;
                        InitSon();
                    }
                    else
                    {
                        FinDuJeuMonde();
                    }
                }
                if (rAbeilleHaut.IntersectsWith(RBob) && accroupi == true)
                {
                    Canvas.SetTop(lesAbeillesHaut[i], alea.Next(HAUTEURALEATOIRE, HAUTCANVA));
                    Canvas.SetLeft(lesAbeillesHaut[i], alea.Next(-GAUCHEDUCANVAALEATOIRE, DROITEDUCANVAALEATOIRE));                    
                    nbScore = nbScore + 1;
                    blockScore.Text = "Score : " + nbScore;
                }

                //on lance les timer pour le cooldown de l'accroupi et pou le temps qu'il reste accroupi
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
                 Pause();
            }      
        }

        //recupère les clique de la souris
        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (Mouse.LeftButton == MouseButtonState.Pressed && !deplacementMarteau && regardDroite == false && pause == false)
            {

                lancer = true;
                deplacementMarteau = true;
                marteau.Visibility = Visibility.Visible;
                Canvas.SetLeft(marteau, Canvas.GetLeft(bob));
                Canvas.SetTop(marteau, Canvas.GetTop(bob) + bob.ActualWidth - MARTEAUHITBOXBOB);
            }
        }
        //pause//
        private void Pause()
        {
            if (pause == true)
            {
                minuteurJeu.Stop();
                chronoJeu.Stop();
                pause = false;
            }
            else if (pause == false)
            {
                minuteurJeu.Start();
                chronoJeu.Start();
                pause = true;
            }
        }

        //fin du jeu//
        private void FinDuJeuMonde()
        {
            pause = true;
            Pause();
            MessageBoxResult result;
            result = MessageBox.Show("Vous avez perdu, voulez vous rejouer ? ", "rejouer", MessageBoxButton.YesNo);

            if (result == MessageBoxResult.Yes)
            {
                //on reinitialise tout et on relance le monde
                bouclier1.Visibility = Visibility.Visible;
                bouclier2.Visibility = Visibility.Visible;
                bouclier3.Visibility = Visibility.Visible;

                minuteurJeu.Stop();
                chronoJeu.Stop();
                CanvaFond.Children.Clear();
                
                nbBouclier = 3;
                nbScore = 0;
                vitesseSpikeMan = 4;
                tmps = 0;

                blockTemps.Text = "Temps : " + TimeSpan.FromSeconds(tmps);
                blockScore.Text = "Score : " + nbScore;

                droite = false;
                gauche = false;
                accroupi = false;
                lancer = false;
                deplacementMarteau = false;
                lesSpikeMan.Clear();
                lesAbeillesHaut.Clear();
                pause = false;
                InitialisationMonde();
            }
            if (result == MessageBoxResult.No)
            {
                this.Close();
            }
        }
    }
}