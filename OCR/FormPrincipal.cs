using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Text;

using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Globalization;
using System.Xml.Serialization;

using OCR.Properties;
using OCR.RNN;
using OCR;
using OCR.Tools;

using RNC;
using RNC.Reseau;
using RNC.Couches;
using RNC.Donnees;


using ToolsImages.Conversion;
using ToolsImages.Structures;
using ToolsImages;

using System.Diagnostics;

namespace OCR
{
    public delegate void DelegateAddObject(int i, Object s);

    public partial class FormPrincipal : Form
    {
        public DelegateAddObject _DelegateAddObject;
        public Definitions definition = new Definitions();
        public Matrix regionMatrix = null;

        private Graphics DisplayGraphics;

        Archive archive = new Archive();

        private ImageResize imageResize;
        private Boolean encours = false;
        private Test test;

        private int TypeReso = 0;

        private double NbPalierBackpropagation = Convert.ToDouble(OCR.Properties.Settings.Default.NbPalierBackpropagation);
        private double Eta = Convert.ToDouble(OCR.Properties.Settings.Default.Eta);
        private double MinimumEta = Convert.ToDouble(OCR.Properties.Settings.Default.MinimumEta);
        private double EtaInitial = Convert.ToDouble(OCR.Properties.Settings.Default.EtaInitial);

        private string imagesApprentissage = OCR.Properties.Settings.Default.ImageApprentissage;
        private string imagesTest = OCR.Properties.Settings.Default.ImageTest;

        private string imagesTestCaracteresImprimes = OCR.Properties.Settings.Default.ImagesTestCaracteresImrpimes;

        private string LabelApprentissage = OCR.Properties.Settings.Default.LabelApprentissage;
        private string LabelTest = OCR.Properties.Settings.Default.LabelTest;

        private string CheminMnist = OCR.Properties.Settings.Default.PathMnist;
        private string CheminPoliceMajuscules = OCR.Properties.Settings.Default.PathPolicesMajuscules;
        private string CheminPoliceMinuscules = OCR.Properties.Settings.Default.PathPolicesMinuscules;
        private string CheminPoliceChiffres = OCR.Properties.Settings.Default.PathPolicesChiffres;
        private string CheminPoliceAutres = OCR.Properties.Settings.Default.PathPolicesAutres;

        private BackgroundWorker bgw1;
        private BackgroundWorker bgw2;
        private Stopwatch stopWatch = new Stopwatch();

        private int nextIndex = 0;
        private int precIndex = 0;
        private String label = "";
        private Boolean MnistReady = false;
        private Boolean ParametrageReady = false;

        private double[][] trainMatrix ;
        private double[][] testMatrix ;

        private Bitmap image;
        private static string[] fonts = new string[] { "Arial", "Courier", "Tahoma", "Times New Roman", "Verdana" };
        private bool[] regularFonts = new bool[fonts.Length];
        private bool[] italicFonts = new bool[fonts.Length];

        string familyName;
        string familyList = "";
        FontFamily[] fontFamilies;
        FontFamily fontFamily = new FontFamily("Arial");

        private Font font;
        private RectangleF rectF;
        private SolidBrush solidBrush;

        Bitmap BitmapNormalisation = null;
        Bitmap bmpResize = null;

        InstalledFontCollection installedFontCollection = new InstalledFontCollection();

        private String PathParametrage = OCR.Properties.Settings.Default.Parametrage;

       // Definitions.trainImages = null;
        
       // trainImages = null;
       // testImages = null;
               
        //ReseauNeurone rnn;
        //MnistDatabase mnistDatabase;
        //Apprentissage apprentissage;

        private String fichierImages; 


        private int INeurone = 0;
        private int IPoids = 0;

        private String lbl = "Label = ";

        private Polices polices = null;


        public FormPrincipal()
        {
            InitializeComponent();

            _DelegateAddObject = new DelegateAddObject(this.AddObject);

            btnArreter.Enabled = false;

            bgw1 = new BackgroundWorker();
            bgw1.DoWork += new DoWorkEventHandler(bgw1_DoWork);
            bgw1.ProgressChanged += new ProgressChangedEventHandler(bgw1_ProgressChanged);
            bgw1.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw1_RunWorkerCompleted);

            bgw1.WorkerReportsProgress = true;
            bgw1.WorkerSupportsCancellation = true;

            bgw2 = new BackgroundWorker();
            bgw2.DoWork += new DoWorkEventHandler(bgw2_DoWork);
            bgw2.ProgressChanged += new ProgressChangedEventHandler(bgw2_ProgressChanged);
            bgw2.RunWorkerCompleted += new RunWorkerCompletedEventHandler(bgw2_RunWorkerCompleted);

            bgw2.WorkerReportsProgress = true;
            bgw2.WorkerSupportsCancellation = true;

            font = new Font(fontFamily, 8, FontStyle.Regular, GraphicsUnit.Point);
            rectF = new RectangleF(10, 10, 32, 32);
            solidBrush = new SolidBrush(Color.Black);

            // Get the array of FontFamily objects.
            //fontFamilies = installedFontCollection.Families;

            //int count = fontFamilies.Length;
            //for (int j = 0; j < count; ++j)
            //{
            //    if ( fontFamilies[j].Name.Length > 0)
            //    { 
            //        Polices police = new Polices();

            //        police.libelle = fontFamilies[j].Name;
            //        police.actif = false;

            //        lstPolices.Add(police);

            //        //familyName = fontFamilies[j].Name;
            //        //familyList = familyList + familyName;
            //        //familyList = familyList + ",  ";
            //    }
            //}

            //XmlSerializer xs = new XmlSerializer(typeof(List<Polices>));
            //using (StreamWriter wr = new StreamWriter(Path.Combine(PathParametrage,"polices.xml")))
            //{
            //    xs.Serialize(wr, lstPolices);
            //}

            if (File.Exists(Path.Combine(PathParametrage, "polices.xml")))
            {
                polices = Polices.Charger(Path.Combine(PathParametrage, "polices.xml"));
            }
            
            //XmlSerializer xs = new XmlSerializer(typeof(List<Polices>));
            //using (StreamReader rd = new StreamReader(Path.Combine(PathParametrage, "polices.xml")))
            //{
            //    Polices p = xs.Deserialize(rd) as Polices;
            //    Console.WriteLine("Id : {0}", p.Id);
            //    Console.WriteLine("Nom : {0} {1}", p.FirstName, p.LastName);
            //}


        }

        private void AddObject(int iCondition, object value)
        {
            switch (iCondition)
            {
                case 2:
                   Double nb = Math.Round(Convert.ToDouble(value), 4);

                   string result = string.Format("{0:0.0%}", nb.ToString());
                    lblPourcentage.Text = result;
                    break;
                case 3:
                    //Double nbExecution = Math.Round(Convert.ToDouble(value), 4);
                   // string execution = string.Format("{0:0.0%}", nbExecution.ToString());

                    lstExecution.Items.Add((string)value);
                    break;
                case 5:
                    lblImageCourante.Text = (string)value;
                    break;

                case 6:
                    lstTest.Items.Add((string)value);
                    break; 
                    
                case 7:
                    lblresultat.Text = (string)value;
                    break;

                case 8:
                    lblChiffreReconnu.Text = (String)value;
                    break;

                case 9:
                    Double[] dbl = (Double[])value;

                    lbl0.Text = "0 : " +  dbl[0].ToString();
                    lbl1.Text = "1 : " + dbl[1].ToString();
                    lbl2.Text = "2 : " + dbl[2].ToString();
                    lbl3.Text = "3 : " + dbl[3].ToString();
                    lbl4.Text = "4 : " + dbl[4].ToString();
                    lbl5.Text = "5 : " + dbl[5].ToString();
                    lbl6.Text = "6 : " + dbl[6].ToString();
                    lbl7.Text = "7 : " + dbl[7].ToString();
                    lbl8.Text = "8 : " + dbl[8].ToString();
                    lbl9.Text = "9 : " + dbl[9].ToString();

                    break;
 
                default:
                    break;
            }
        }

        #region Structure Reseau 
        /// <summary>
        /// Creation reseau convolutionel pour le chiffres
        /// </summary>
        /// <returns></returns>
        private Boolean CreateReseauConvolutionel()
        {
            //String Label;
            //int INeurone = 0;
          
            Definitions.rnn = new ReseauNeurone(this.NbPalierBackpropagation,this.Eta,this.MinimumEta,this.EtaInitial);
                                               
            Definitions.rnn.Couches = new List<CoucheBase>();
            Definitions.rnn.NombreCouches = 5;

            CoucheEntree coucheEntree = new CoucheEntree("Couche entrée", new Size(29,29));
            Definitions.rnn.inputDesignedPatternSize = new Size(29, 29);
            coucheEntree.Initialize();

            Definitions.rnn.Couches.Add(coucheEntree);

            CoucheSupConvolution coucheConvolution = new CoucheSupConvolution("01", coucheEntree, new Size(13, 13), 6, 5);
            coucheConvolution.Initialize();
            Definitions.rnn.Couches.Add(coucheConvolution);

            coucheConvolution = new CoucheSupConvolution("02", coucheConvolution, new Size(5, 5), 50, 5);
            coucheConvolution.Initialize();
            Definitions.rnn.Couches.Add(coucheConvolution);

            CoucheFullConnecte coucheFullConnecte = new CoucheFullConnecte("03", coucheConvolution, 100);
            coucheFullConnecte.Initialize();
            Definitions.rnn.Couches.Add(coucheFullConnecte);

            CoucheSortie coucheSortie = new CoucheSortie("04", coucheFullConnecte, 10);
            coucheSortie.Initialize();
            Definitions.rnn.Couches.Add(coucheSortie);
           
            return true;
        }

        private Boolean CreateReseauConvolutionelLettres()
        {
            try
            {
                Definitions.rnn = new ReseauNeurone(this.NbPalierBackpropagation, this.Eta, this.MinimumEta, this.EtaInitial);

                Definitions.rnn.Couches = new List<CoucheBase>();
                Definitions.rnn.NombreCouches = 5;

                CoucheEntree coucheEntree = new CoucheEntree("Couche entrée", new Size(32, 32));
                Definitions.rnn.inputDesignedPatternSize = new Size(32, 32);
                coucheEntree.Initialize();

                Definitions.rnn.Couches.Add(coucheEntree);

                CoucheSupConvolution coucheConvolution = new CoucheSupConvolution("01", coucheEntree, new Size(13, 13), 6, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);
                
                coucheConvolution = new CoucheSupConvolution("03", coucheConvolution, new Size(5, 5), 60, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                CoucheFullConnecte coucheFullConnecte = new CoucheFullConnecte("04", coucheConvolution, 200);
                coucheFullConnecte.Initialize();
                Definitions.rnn.Couches.Add(coucheFullConnecte);

               // CoucheFullConnecte coucheFullConnecte1 = new CoucheFullConnecte("05", coucheFullConnecte, 100);
               // coucheFullConnecte1.Initialize();
               // Definitions.rnn.Couches.Add(coucheFullConnecte1);
                
                CoucheSortie coucheSortie = new CoucheSortie("06", coucheFullConnecte, 26);
                coucheSortie.Initialize();
                Definitions.rnn.Couches.Add(coucheSortie);


                /*

                Definitions.rnn.Couches = new List<CoucheBase>();
                Definitions.rnn.NombreCouches = 8;

                CoucheEntree coucheEntree = new CoucheEntree("Couche entrée", new Size(32, 32));
                Definitions.rnn.inputDesignedPatternSize = new Size(32, 32);
                coucheEntree.Initialize();

                Definitions.rnn.Couches.Add(coucheEntree);

                CoucheConvolution coucheConvolution = new CoucheConvolution("01", coucheEntree, 10, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                CoucheSimple coucheSimple = new CoucheSimple("02", coucheConvolution);
                coucheSimple.Initialize();
                Definitions.rnn.Couches.Add(coucheSimple);

                coucheConvolution = new CoucheConvolution("03", coucheSimple, 50, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                coucheSimple = new CoucheSimple("04", coucheConvolution);
                coucheSimple.Initialize();
                Definitions.rnn.Couches.Add(coucheSimple);

                CoucheFullConnecte coucheFullConnecte = new CoucheFullConnecte("05", coucheSimple, 200);
                coucheFullConnecte.Initialize();
                Definitions.rnn.Couches.Add(coucheFullConnecte);

                CoucheFullConnecte coucheFullConnecte1 = new CoucheFullConnecte("06", coucheFullConnecte, 100);
                coucheFullConnecte1.Initialize();
                Definitions.rnn.Couches.Add(coucheFullConnecte1);

                CoucheSortie coucheSortie = new CoucheSortie("07", coucheFullConnecte1, 26);
                coucheSortie.Initialize();
                Definitions.rnn.Couches.Add(coucheSortie);
                 * */

                return true;
                

            }
            catch (Exception err)
            {
                return false;
            }
        }

        /// <summary>
        /// Prévoir parametrage XML
        /// </summary>
        /// <param name="sortie"></param>
        /// <returns></returns>
        private Boolean CreateReseauConvolutionelLettres(int NombreSortie)
        {
            try
            {
                Definitions.rnn = new ReseauNeurone(this.NbPalierBackpropagation, this.Eta, this.MinimumEta, this.EtaInitial);

                Definitions.rnn.Couches = new List<CoucheBase>();
                Definitions.rnn.NombreCouches = 5;

                CoucheEntree coucheEntree = new CoucheEntree("Couche entrée", new Size(32, 32));
                Definitions.rnn.inputDesignedPatternSize = new Size(32, 32);
                coucheEntree.Initialize();

                Definitions.rnn.Couches.Add(coucheEntree);

                CoucheSupConvolution coucheConvolution = new CoucheSupConvolution("01", coucheEntree, new Size(13, 13), 6, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                coucheConvolution = new CoucheSupConvolution("03", coucheConvolution, new Size(5, 5), 60, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                CoucheFullConnecte coucheFullConnecte = new CoucheFullConnecte("04", coucheConvolution, 100);
                coucheFullConnecte.Initialize();
                Definitions.rnn.Couches.Add(coucheFullConnecte);

                // CoucheFullConnecte coucheFullConnecte1 = new CoucheFullConnecte("05", coucheFullConnecte, 100);
                // coucheFullConnecte1.Initialize();
                // Definitions.rnn.Couches.Add(coucheFullConnecte1);

                CoucheSortie coucheSortie = new CoucheSortie("06", coucheFullConnecte, NombreSortie);
                coucheSortie.Initialize();
                Definitions.rnn.Couches.Add(coucheSortie);
         

                return true;


            }
            catch (Exception err)
            {
                return false;
            }
        }


        private Boolean CreateReseauConvolutionelLettres(int couche3, int couche4, int NombreSortie)
        {
            try
            {
                Definitions.rnn = new ReseauNeurone(this.NbPalierBackpropagation, this.Eta, this.MinimumEta, this.EtaInitial);

                Definitions.rnn.Couches = new List<CoucheBase>();
                Definitions.rnn.NombreCouches = 5;

                CoucheEntree coucheEntree = new CoucheEntree("Couche entrée", new Size(32, 32));
                Definitions.rnn.inputDesignedPatternSize = new Size(32, 32);
                coucheEntree.Initialize();

                Definitions.rnn.Couches.Add(coucheEntree);

                CoucheSupConvolution coucheConvolution = new CoucheSupConvolution("01", coucheEntree, new Size(13, 13), 6, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                coucheConvolution = new CoucheSupConvolution("03", coucheConvolution, new Size(5, 5), couche3, 5);
                coucheConvolution.Initialize();
                Definitions.rnn.Couches.Add(coucheConvolution);

                CoucheFullConnecte coucheFullConnecte = new CoucheFullConnecte("04", coucheConvolution, couche4);
                coucheFullConnecte.Initialize();
                Definitions.rnn.Couches.Add(coucheFullConnecte);

                // CoucheFullConnecte coucheFullConnecte1 = new CoucheFullConnecte("05", coucheFullConnecte, 100);
                // coucheFullConnecte1.Initialize();
                // Definitions.rnn.Couches.Add(coucheFullConnecte1);

                CoucheSortie coucheSortie = new CoucheSortie("06", coucheFullConnecte, NombreSortie);
                coucheSortie.Initialize();
                Definitions.rnn.Couches.Add(coucheSortie);


                return true;


            }
            catch (Exception err)
            {
                return false;
            }
        }
        #endregion

        private void btnDebut_Click(object sender, EventArgs e)
        {
            try
            {
                btnDebut.Enabled  = false;
                btnArreter.Enabled = true;

                if (radDigit.Checked == true)
                {
                    TypeReso = 0;
                    // Charger MNIST 
                    Definitions.mnistApprentissageDatabase = new MnistDatabase();

                    Definitions.mnistApprentissageDatabase.LoadData(Path.Combine(CheminMnist, imagesApprentissage),
                        Path.Combine(CheminMnist, LabelApprentissage));

                    MnistReady = true;

                    // Construire structure reseau
                    CreateReseauConvolutionel();

                    Definitions.apprentissage = new Apprentissage(Definitions.rnn, Definitions.mnistApprentissageDatabase, 10, this, chkDistorsion.Checked);

                    if (chkPoids.Checked == true)
                    {
                        using (var openFileDialog1 = new OpenFileDialog { Filter = "Parametrage Réseau neuronaux convultifs (*.xml)|*.xml", Title = "Ouverture réseau neurones convultifs." })
                        {
                            if (openFileDialog1.ShowDialog() == DialogResult.OK)
                            {

                                Definitions.rnn = archive.deserialise(openFileDialog1.FileName);
                            }
                        }
                    }
                    bgw1.RunWorkerAsync(Definitions.apprentissage);
                }

                if (radMaj.Checked == true)
                {
                    TypeReso = 1;
                    // Charger base
                    ChargerBase();

                    CreateReseauConvolutionelLettres();

                    Definitions.apprentissage = new Apprentissage(Definitions.rnn, trainMatrix, 26, this, chkDistorsion.Checked);

                    bgw1.RunWorkerAsync(Definitions.apprentissage);

                }

                if (radMin.Checked == true)
                {
                    TypeReso = 2;

                    // Charger base
                    ChargerBase();

                    CreateReseauConvolutionelLettres();

                    Definitions.apprentissage = new Apprentissage(Definitions.rnn, trainMatrix, 26, this, chkDistorsion.Checked);

                    bgw1.RunWorkerAsync(Definitions.apprentissage);
                }

                if (radChiffresImprimes.Checked == true)
                {
                    TypeReso = 3;

                    // Charger base
                    ChargerBase();

                    CreateReseauConvolutionelLettres(10);

                    Definitions.apprentissage = new Apprentissage(Definitions.rnn, trainMatrix, 10, this, chkDistorsion.Checked);

                    bgw1.RunWorkerAsync(Definitions.apprentissage);

                }

                if (radMixte_M_C.Checked == true)
                {
                    TypeReso = 4;

                    // Charger base
                    ChargerBase();

                    CreateReseauConvolutionelLettres(100, 250, 36);

                    //CreateReseauConvolutionelLettres(36);

                    Definitions.apprentissage = new Apprentissage(Definitions.rnn, trainMatrix, 36, this, chkDistorsion.Checked);

                    bgw1.RunWorkerAsync(Definitions.apprentissage);

                }


                // Threading ? 
             //   Definitions.apprentissage.BackPropagationThead();

             

               
            }
            catch (Exception ex)
            {
                MnistReady = false;
                btnDebut.Enabled = true;
                btnArreter.Enabled = false;
                MessageBox.Show(ex.Message + " " + ex.Source);
            }
        }

        private Boolean ChargerBase()
        {
            try
            {
                OpenFileDialog openFileDialog1 = new OpenFileDialog();

                openFileDialog1.InitialDirectory = @"d:\mnist\travail\";
                openFileDialog1.RestoreDirectory = true;
                openFileDialog1.Title = "Fichier Images";
                openFileDialog1.DefaultExt = "txt";
                openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

                if (openFileDialog1.ShowDialog() == DialogResult.OK)
                {
                    fichierImages = openFileDialog1.FileName;

                    trainMatrix = null;
                    testMatrix = null;

                    Console.WriteLine("\nGenerating train and test matrices using an 80%-20% split");
                    //MakeTrainAndTest(fichierImages, out trainMatrix, out testMatrix);

                    MakeTrainAndTest(fichierImages, out trainMatrix, out testMatrix);

                    int index = Convert.ToInt32(txtIndex.Text);

                    lblNbreImages.Text = "Nombre d'images: " + trainMatrix.Length.ToString();

                 
                }

                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        private void bgw1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)
            {

                if (worker.CancellationPending)
                {
                    e.Cancel = true;

                }

                if (encours == false)
                {
                    Faire();
                    encours = true;
                }
            }
          
        }

        private void Faire()
        {
            switch (TypeReso)
            {
                case 0:
                  Definitions.apprentissage.BackPropagationThead();
                  break; 
                case 1:
                  Definitions.apprentissage.ApprentissageThead();
                 break;
                case 2:
                  Definitions.apprentissage.ApprentissageThead();
                 break;
                case 3:
                 Definitions.apprentissage.ApprentissageThead();
                 break;
                case 4:
                 Definitions.apprentissage.ApprentissageThead();
                 break;
            }
            
        }
        private void Test()
        {
            Definitions.test.TestThread();
           
        }

        private void bgw1_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }
        private void bgw1_RunWorkerCompleted(object sender,
        RunWorkerCompletedEventArgs e)
        {
            if (e.Error != null)
            {
               // this.txtResult.AppendText(e.Error.Message + Environment.NewLine);

            }
            else if (e.Cancelled)
            {
                Debug.Print ("Annuler");
              //  this.txtResult.AppendText("Tache abandonnée" + Environment.NewLine);
            }
            else
            {
               // this.txtResult.AppendText("Fin de Tache" + Environment.NewLine);
            }

            stopWatch.Stop();

            //Change the status of the buttons on the UI accordingly
            btnDebut.Enabled = true;
            btnArreter.Enabled = false;

        }

        private void bgw2_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker worker = sender as BackgroundWorker;

            while (true)
            {

                if (worker.CancellationPending)
                {
                    e.Cancel = true;

                }

                if (encours == false)
                {
                    Test();
                    encours = true;
                }
            }

        }
        private void bgw2_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
        }

        private void bgw2_RunWorkerCompleted(object sender,
        RunWorkerCompletedEventArgs e)
        {
        }
        public static Bitmap MakeBitmap(MnistImage dImage, int mag)
        {
            // create a C# Bitmap suitable for display in a PictureBox control
            int width = dImage.largeur * mag;
            int height = dImage.hauteur * mag;
            Bitmap result = new Bitmap(width , height);
            Graphics gr = Graphics.FromImage(result);
            for (int i = 0; i < dImage.hauteur; ++i)
            {
                for (int j = 0; j < dImage.largeur; ++j)
                {
                    int pixelColor = dImage.pixels[(i * dImage.hauteur) + j]; // white background, black digits

                    //int pixelColor = 255 - dImage.pixels[i + j]; // white background, black digits
                    //int pixelColor = 255 - dImage.pixels[i][j]; // white background, black digits
                    //int pixelColor = dImage.pixels[i][j]; // black background, white digits
                    Color c = Color.FromArgb(pixelColor, pixelColor, pixelColor); // gray scale
                    //Color c = Color.FromArgb(pixelColor, 0, 0); // red scale
                    SolidBrush sb = new SolidBrush(c);
                    gr.FillRectangle(sb, j * mag, i * mag, mag, mag); // fills bitmap via Graphics object
                }
            }
            return result;
        }
        public Bitmap MakeBitmap(Double[][] dImage, int mag, int index)
        {
            // create a C# Bitmap suitable for display in a PictureBox control

            int X = Convert.ToInt16(Math.Sqrt(dImage[0].Length));
            int Y = Convert.ToInt16(Math.Sqrt(dImage[0].Length));

            int width =  X * mag;
            int height = Y * mag;

            this.lblLabelVisu.Text = ((char)dImage[index][0]).ToString();
                     
        
            Bitmap result = new Bitmap(Convert.ToInt32(width),Convert.ToInt32(height));
            Graphics gr = Graphics.FromImage(result);
            for (int i = 0; i < Math.Sqrt(dImage[index].Length); ++i)
            {
                for (int j = 1; j < Math.Sqrt(dImage[index].Length); ++j)
                {
                    int pixelColor = Convert.ToInt32(dImage[index][(i * X) + j]);
                    //int pixelColor = dImage.pixels[(i * height) + j]; // white background, black digits

                    Color c = Color.FromArgb(pixelColor, pixelColor, pixelColor); // gray scale
                   
                    SolidBrush sb = new SolidBrush(c);
                    gr.FillRectangle(sb, j * mag, i * mag, mag, mag); // fills bitmap via Graphics object
                }
            }
            
            return result;
            //return null;
        }

        //public static Bitmap MakeBitmap(MnistImage dImage, int mag)
        //{
        //    // create a C# Bitmap suitable for display in a PictureBox control
        //    int width = dImage.largeur * mag;
        //    int height = dImage.hauteur * mag;
        //    Bitmap result = new Bitmap(width, height);
        //    Graphics gr = Graphics.FromImage(result);
        //    for (int i = 0; i < dImage.hauteur; ++i)
        //    {
        //        for (int j = 0; j < dImage.largeur; ++j)
        //        {
        //            int pixelColor = 255 - dImage.pixels[i + j]; // white background, black digits
        //            //int pixelColor = 255 - dImage.pixels[i][j]; // white background, black digits
        //            //int pixelColor = dImage.pixels[i][j]; // black background, white digits
        //            Color c = Color.FromArgb(pixelColor, pixelColor, pixelColor); // gray scale
        //            //Color c = Color.FromArgb(pixelColor, 0, 0); // red scale
        //            SolidBrush sb = new SolidBrush(c);
        //            gr.FillRectangle(sb, j * mag, i * mag, mag, mag); // fills bitmap via Graphics object
        //        }
        //    }
        //    return result;
        //}

        private void btnSuivant_Click(object sender, EventArgs e)
        {
            if (MnistReady == false)
            {
                MnistReady = MnisDatabase();
            }

            if (MnistReady == false)
                return;

            if (ParametrageReady == false)
            {
               ParametrageReady = Parametrage();
            }
            if (ParametrageReady == false)
                return;

            nextIndex = Convert.ToInt32(txtIndexCourant.Text);
                
           
            MnistImage currImage = Definitions.mnistTestDatabase.result[nextIndex];
            int mag = 4;

            Bitmap bitMap = MakeBitmap(currImage, mag);
            pctImage.Image = bitMap;
            label = currImage.label.ToString();
            lblLabel.Text = lbl +  label;

            Definitions.test = new Test(Definitions.rnn, Definitions.mnistTestDatabase, this);

            Definitions.test.TestPattern(nextIndex);

           
            nextIndex++;

            txtIndexCourant.Text = nextIndex.ToString();

            precIndex = nextIndex - 1;
            txtIndexPrec.Text = precIndex.ToString(); 

        }

        private void btnArreter_Click(object sender, EventArgs e)
        {
            if (bgw1 != null && bgw1.IsBusy)
            {
                bgw1.CancelAsync();
                return;
            }

        }
      
        private void btnDebutTest_Click(object sender, EventArgs e)
        {
            btnDebutTest.Enabled = false;
            btnFinTest.Enabled = true;
            MnistReady = false;
            String FichierRNN;

            Archive archive = new Archive();

            try
            {

                // Charger parametrage 
                using (var openFileDialog1 = new OpenFileDialog { Filter = "Parametrage Réseau neuronaux convultifs (*.xml)|*.xml", Title = "Ouverture réseau neurones convultifs." })
                {
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        FichierRNN = openFileDialog1.FileName;
                        //var fsIn = openFileDialog1.OpenFile();

                        Definitions.rnn = new ReseauNeurone(this.NbPalierBackpropagation, this.Eta, this.MinimumEta, this.EtaInitial);
                      
                        Definitions.rnn = archive.deserialise(FichierRNN);
                    }
                }

                // Image database 
                if (rdChiffres.Checked == true)
                {

                    if (rbApprentissage.Checked == true)
                    {

                        Definitions.mnistApprentissageDatabase = new MnistDatabase();

                        Definitions.mnistApprentissageDatabase.LoadData(Path.Combine(CheminMnist, imagesApprentissage),
                            Path.Combine(CheminMnist, LabelApprentissage));

                        MnistReady = true;

                    }
                    else
                    {
                        Definitions.mnistTestDatabase = new MnistDatabase();
                        
                        Definitions.mnistTestDatabase.LoadData(Path.Combine(CheminMnist, imagesTest),
                          Path.Combine(CheminMnist, LabelTest));

                        MnistReady = true;

                    }
                }

                if (rdMajuscules.Checked == true)
                {
                    if (rbApprentissage.Checked == true)
                    {
                    }
                    else
                    {
                        ChargerBase();

                        Definitions.mnistTestDatabase = new MnistDatabase();

                       // CreateReseauConvolutionelLettres();

                        //Definitions.apprentissage = new Apprentissage(Definitions.rnn, trainMatrix, 26, this, chkDistorsion.Checked);

                        Definitions.mnistTestDatabase.LoadData(Path.Combine(CheminMnist, imagesTestCaracteresImprimes), Path.Combine(CheminMnist, LabelTest));

                        MnistReady = true;

                    }

                }


                if (rdMinuscules.Checked == true)
                {
                }

                if (MnistReady == false)
                    return;

               

                lstTest.Items.Clear();

                Definitions.test = new Test(Definitions.rnn, Definitions.mnistTestDatabase, this,26,26,32);

                bgw2.RunWorkerAsync(Definitions.test);

               // test.TestThread();

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message + " " + err.Source);
            }

            
        }

        Boolean Parametrage()
        {
            String FichierRNN;

            Archive archive = new Archive();

            try
            {

                // Charger parametrage 
                using (var openFileDialog1 = new OpenFileDialog { Filter = "Parametrage Réseau neuronaux convultifs (*.xml)|*.xml", Title = "Ouverture réseau neurones convultifs." })
                {
                    if (openFileDialog1.ShowDialog() == DialogResult.OK)
                    {
                        FichierRNN = openFileDialog1.FileName;
                        //var fsIn = openFileDialog1.OpenFile();

                        Definitions.rnn = new ReseauNeurone(this.NbPalierBackpropagation, this.Eta, this.MinimumEta, this.EtaInitial);


                        Definitions.rnn = archive.deserialise(FichierRNN);
                    }
                }

            }
            catch(Exception err)
            {
                return false;
            }

            return true;
        }

        Boolean MnisDatabase()
        {
            try
            {
                Definitions.mnistTestDatabase = new MnistDatabase();

                Definitions.mnistTestDatabase.LoadData(Path.Combine(CheminMnist, imagesTest),
                  Path.Combine(CheminMnist, LabelTest));

                MnistReady = true;

                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }

        private void btnProcess_Click(object sender, EventArgs e)
        {
            String Source = txtSourceImg.Text;
            String destination = txtDestination.Text;
            int pixels = 32;

            imageResize = new ImageResize(Source,destination,pixels,"Printed_Caracteres.txt");

            //imageResize.convert(@"D:\MNIST\travail\img001-00001_R.png");

            imageResize.process();
            imageResize.Close();
        }

        private void btnCharger_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();
                        
            openFileDialog1.InitialDirectory = @"d:\mnist\travail\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Fichier Images";
            openFileDialog1.DefaultExt = "txt";
            openFileDialog1.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fichierImages = openFileDialog1.FileName;

                trainMatrix = null;
                testMatrix = null;

                Console.WriteLine("\nGenerating train and test matrices using an 80%-20% split");
                MakeTrainAndTest(fichierImages, out trainMatrix, out testMatrix);

                int index = Convert.ToInt32(txtIndex.Text);

                lblNbreImages.Text = "Nombre d'images: " + trainMatrix.Length.ToString();

                Bitmap bitMap = MakeBitmap(trainMatrix, 4,index);

                pctVisu.SizeMode = PictureBoxSizeMode.StretchImage;
                pctVisu.Image = bitMap;

                //txtIndexPred.Text = index.ToString();
                //txtIndex.Text = index++.ToString();
            }
        }

        static void MakeTrainAndTest(string file, out double[][] trainMatrix, out double[][] testMatrix)
        {
            int numLines = 0;
             FileStream ifs = new FileStream(file, FileMode.Open);
            StreamReader sr = new StreamReader(ifs);
            while (sr.ReadLine() != null)
                ++numLines;
            sr.Close(); ifs.Close();

            int numTrain = (int)(0.99 * numLines);
            int numTest = numLines - numTrain;

            double[][] allData = new double[numLines][];  // could use Helpers.MakeMatrix here
            for (int i = 0; i < allData.Length; ++i)
                allData[i] = new double[1024];               // (y0)(x0, x1, x2, x3,...,x841)

            string line = "";
            string[] tokens = null;
            ifs = new FileStream(file, FileMode.Open);
            sr = new StreamReader(ifs);
            int row = 0;
            while ((line = sr.ReadLine()) != null)
            {
                tokens = line.Split(' ');

                int i1 = (int)Convert.ToChar(tokens[0]);  

                // Le premier le label 
                allData[row][0] = Convert.ToDouble(i1);
                
                for (int i = 1; i < tokens.Count()-2; i++)
                    allData[row][i] = double.Parse(tokens[i]);

                ++row;

            }

            sr.Close(); ifs.Close();

            Helpers.ShuffleRows(allData);

            trainMatrix = Helpers.MakeMatrix(numTrain, 1024);
            testMatrix = Helpers.MakeMatrix(numTest, 1024);

            for (int i = 0; i < numTrain; ++i)
            {
                allData[i].CopyTo(trainMatrix[i], 0);
            }

            for (int i = 0; i < numTest; ++i)
            {
                allData[i + numTrain].CopyTo(testMatrix[i], 0);
            }




        }

        private void button2_Click(object sender, EventArgs e)
        {
            try
            {
                int index = Convert.ToInt32(txtIndex.Text);

                txtIndexPred.Text = index.ToString();

                Bitmap bitMap = MakeBitmap(trainMatrix, 4, ++index);
                pctVisu.Image = bitMap;
                //label = currImage.label.ToString();

               
                txtIndex.Text = index.ToString();

            }
            catch (Exception err)
            {

            }

        }

        private void btnImg_Click(object sender, EventArgs e)
        {
            Bitmap imgNormal = null;
            Bitmap imgItalique = null;
            Char Start = 'A';
            String CheminPolices = "";
            int step = 0;
            string path = "";

            foreach (Control c in Liste.Controls)
            {
                if (c.GetType() == typeof(RadioButton))
                {
                    RadioButton rb = c as RadioButton;
                    if (rb.Checked)
                    {
                        switch ( rb.Name.ToUpper())
                        {
                            case "MAJUSCULES":
                                Start = 'A';
                                CheminPolices = CheminPoliceMajuscules;
                                step = 26;
                                break;

                            case "MINUSCULES":
                                Start = 'a';
                                CheminPolices = CheminPoliceMinuscules;
                                step = 26;
                                break;

                            case "CHIFFRES":
                                Start = '0';
                                CheminPolices = CheminPoliceChiffres;
                                step = 10;
                                break;

                            case "AUTRES":
                                Start = '!';
                                CheminPolices = CheminPoliceAutres;
                                step = 14;
                                break;
                        }
                        //MessageBox.Show(rb.Name); 
                    }
                }
            }


            FileStream fileStream = new FileStream(Path.Combine(CheminPolices,"transco.txt"), FileMode.Append, FileAccess.Write);
            StreamWriter fileWriter = new StreamWriter(fileStream);


            //for (int i = 0; i < fonts.Length; i++)
            for (int i = 0; i < polices.Count; i++)
            {
                if (polices[i].actif == true)
                {
                    for (int j = 0; j < step; j++)
                    {
                        imgNormal = new Bitmap(DrawLetter((char)((int)Start + j), polices[i].libelle, 19, false));
                        imgItalique = new Bitmap(DrawLetter((char)((int)Start + j), polices[i].libelle, 19, true));

                        //imgNormal = new Bitmap(DrawLetter((char)((int)'A' + j), fonts[i], 28, false));
                        //imgItalique = new Bitmap(DrawLetter((char)((int)'A' + j), fonts[i], 28, true));

                        // imgOrigine.Image = imgNormal;

                        //imgNormal.Save(Path.Combine(CheminPolices, ((char)((int)Start + j)).ToString() +  j.ToString() + ".png"));
                        imgNormal.Save(Path.Combine(Path.Combine(CheminPolices,((char)((int)Start + j)).ToString()) , "ImgNormal_" + polices[i].libelle + "_" + j.ToString() + "+" + ((char)((int)Start + j)).ToString() + ".png"));
                        //imgNormal.Save(Path.Combine(CheminPolices, "ImgNormal_" + fonts[i] + "_" + j.ToString() + ".png"));

                        //imgItalique.Save(Path.Combine(CheminPolices, ((char)((int)Start + j)).ToString() + j.ToString() + ".png"));
                        imgItalique.Save(Path.Combine(Path.Combine(CheminPolices, ((char)((int)Start + j)).ToString()), "imgItalique_" + polices[i].libelle + "_" + j.ToString() + "+" + ((char)((int)Start + j)).ToString() + ".png"));
                       // imgItalique.Save(Path.Combine(CheminPolices, "imgItalique_" + fonts[i] + "_" + j.ToString() + ".png"));

                        //imgOrigine.Dispose();
                        // imgNormal.Save(Path.Combine(@"C:\TRAITEMENT\img\police", "ImgNormal_" + j.ToString() + ".bmp" ));
                        // imgItalique.Save(Path.Combine(@"C:\TRAITEMENT\img\police", "imgItalique_" + j.ToString() + ".bmp"));

                       // fileWriter.WriteLine(((char)((int)Start + j)).ToString() + "=" + ((char)((int)Start + j)).ToString());
                     //   fileWriter.Flush();
                        //fileWriter.WriteLine("imgItalique_" + polices[i].libelle + "_" + j.ToString() + "+" + ((char)((int)Start + j)).ToString() + "=" + ((char)((int)Start + j)).ToString());
                       // fileWriter.Flush();
                        
                    }
                }



            }

            fileWriter.Close();

        }

        #region Methode 
        public Bitmap DrawLetter(char c, string fontName, float size, bool italic)
        {
            return DrawLetter(c, fontName, size, italic, true);
        }

        public Bitmap DrawLetter(char c, string fontName, float size, bool italic, bool invalidate)
        {

            //int width = this.ClientRectangle.Width - 2;
            //int height = this.ClientRectangle.Height - 2;

            int width = 32;
            int height = 32;

            Brush blackBrush = new SolidBrush(Color.Black);
            Brush whiteBrush = new SolidBrush(Color.White);

            // free previous image
            if (image != null)
                image.Dispose();

            // create new image
            image = new Bitmap(width, height, PixelFormat.Format24bppRgb);

            // create graphics
            Graphics g = Graphics.FromImage(image);

            // fill rectangle
            g.FillRectangle(whiteBrush, 0, 0, width, height);

            StringFormat sf = new StringFormat();
            sf.LineAlignment = StringAlignment.Center;
            sf.Alignment = StringAlignment.Center;


            // draw letter
            string str = new string(c, 1);
            Font font = new Font(fontName, size, (italic) ? FontStyle.Italic : FontStyle.Regular);
            g.DrawString(str, font, blackBrush, new Rectangle(0, 0, 32, 32),sf);
            //g.DrawString(str, font, blackBrush, new Rectangle(0, 0, width, height));



            g.Dispose();
            font.Dispose();
            blackBrush.Dispose();
            whiteBrush.Dispose();

            if (invalidate)
                Invalidate();

            return image;


        }

        #endregion

        private void btnChargerNorm_Click(object sender, EventArgs e)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.InitialDirectory = @"d:\mnist\travail\";
            openFileDialog1.RestoreDirectory = true;
            openFileDialog1.Title = "Fichier Images";
            openFileDialog1.DefaultExt = "pgn";
            openFileDialog1.Filter = "png (*.png)|*.png|All files (*.*)|*.*";


            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {

                Bitmap image = new Bitmap(openFileDialog1.FileName);

                pctImgNormalisation.SizeMode = PictureBoxSizeMode.Zoom;
                pctImgNormalisation.Image = (Image)image;

            }
        }

        private void btnFilter_Click(object sender, EventArgs e)
        {
            Bitmap bmp = (Bitmap)pctImgNormalisation.Image;

            bmp = ToolsImages.Filtres.Median.MedianFilter(bmp, 3);

            pctImgFilter.SizeMode = PictureBoxSizeMode.Zoom;
            pctImgFilter.Image = bmp;

        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            Dictionary<string, Structure> dic = SetCCL();

            // Normalement un seul element 
            foreach (var pair in dic)
            {
                Structure structure = (Structure)pair.Value;

                int largeur = structure.Largeur;
                int hauteur = structure.Hauteur;

                if (largeur > 0 && hauteur > 0)
                {
                    Rectangle source_rect = new Rectangle(structure.X, structure.Y, largeur, hauteur);
                    Rectangle dest_rect = new Rectangle(0, 0, largeur, hauteur);
                    BitmapNormalisation = new Bitmap(largeur, hauteur);


                    DisplayGraphics = Graphics.FromImage(BitmapNormalisation);
                    DisplayGraphics.DrawImage(pctImgFilter.Image, dest_rect, source_rect, GraphicsUnit.Pixel);

                    pctImgTemp.SizeMode = PictureBoxSizeMode.StretchImage;
                    pctImgTemp.Image = BitmapNormalisation;
                }
            }
        }

        private void btnStep2_Click(object sender, EventArgs e)
        {
            Bitmap saveBitmap = BitmapNormalisation;
            Bitmap tmpBitmap = BitmapNormalisation;

            // Prendre la plus grande largeur
            int max = Outils.MaximumSize(BitmapNormalisation.Height, BitmapNormalisation.Width);

            Size size = new Size();
            size.Height = max;
            size.Width = max;

            bmpResize = Conversion.ResizeBitmap(BitmapNormalisation, max, max);

            pctImgStep2.SizeMode = PictureBoxSizeMode.StretchImage;
            pctImgStep2.Image = bmpResize;
            

        }

        private void btnBordure_Click(object sender, EventArgs e)
        {
            Bitmap saveBitmap = bmpResize;
            Bitmap tmpBitmap = bmpResize;

            bmpResize = BitmapWithBorder(tmpBitmap, tmpBitmap.Height + 4);

            pctImgStep2.SizeMode = PictureBoxSizeMode.StretchImage;
            pctImgBordure.Image = bmpResize;

        }

        private void btnFinal_Click(object sender, EventArgs e)
        {
            Bitmap saveBitmap = bmpResize;
            Bitmap tmpBitmap = bmpResize;

            bmpResize = Conversion.ResizeBitmap(BitmapNormalisation, 32, 32);

            pctImgFinal.SizeMode = PictureBoxSizeMode.Zoom;
            pctImgFinal.Image = bmpResize;

        }

        public Dictionary<string, Structure> SetCCL()
        {
            SetList regionSets = null;
            Vector finalRegionSets = null;

            Bitmap temp = (Bitmap)pctImgNormalisation.Image;
            Bitmap bmap = (Bitmap)temp.Clone();

            CCL ccl = new CCL(bmap);

            ccl.DebutLabel();

            ccl.MarqueRegionAller(out regionSets);
            regionMatrix = ccl.MergeRegions(ccl.regionMatrix, regionSets, out finalRegionSets);

            ccl.MiseAJourRegion();

            return ccl.PositionRectangle();


        }
        private Bitmap BitmapWithBorder(Bitmap originalImage, int Taille)
        {
           
            Bitmap newImage = new Bitmap(Taille, Taille);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.Clear(Color.White);
                int x = (newImage.Width - originalImage.Width) / 2;
                int y = (newImage.Height - originalImage.Height) / 2;
                graphics.DrawImage(originalImage, x, y);
            }

            return newImage;
        }

      

       

      

      

    }
}
