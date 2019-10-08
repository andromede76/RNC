using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using OCR.Properties;

using RNC;
using RNC.Reseau;
using RNC.Donnees;
using RNC.Neurones;

using OCR;

namespace OCR.RNN
{
    public class Apprentissage : ForwardPropagation
    {
        #region Parametres

        public uint erreurReconnaissances;
        public uint bonneReconnaissances;

        private HiPerfTimer m_HiPerfTime;
        private MnistDatabase BaseApprentissage;
        private int nombre;
        public int nombreBackPropagation;
        private int NombreSorties;
        private int SizePattern;

        private Boolean Backpropagate;
        private Boolean Distorsion;
 
        private double NbPalierBackpropagation = Convert.ToDouble(OCR.Properties.Settings.Default.NbPalierBackpropagation);
        private double Eta = Convert.ToDouble(OCR.Properties.Settings.Default.Eta);
        private double MinimumEta = Convert.ToDouble(OCR.Properties.Settings.Default.MinimumEta);
        private double EtaInitial = Convert.ToDouble(OCR.Properties.Settings.Default.EtaInitial);

        private double[][] TrainMatrix;
     
        FormPrincipal _form;

        private Archive archive;

        double _dMSE = 0;
        double _dMSE200 = 0;
        int _nn;
        int _iEpochsCompleted = 0;

        #endregion 

        public Apprentissage(ReseauNeurone neuronet, double[][] trainMatrix , int nombreSorties, FormPrincipal frm,Boolean distorsion)
        {
            _RNN = neuronet;
            TrainMatrix = trainMatrix;
            SizePattern = Convert.ToInt32(Math.Sqrt(TrainMatrix[0].Length));
           // nombre = baseApprentissage.imageCount;
            NombreSorties = nombreSorties;
            erreurReconnaissances = 0;
            bonneReconnaissances = 0;
            nombreBackPropagation = 0;
            _iEpochsCompleted = 0;
            _form = frm;
            m_HiPerfTime = new HiPerfTimer();
            archive = new Archive();

            this.Distorsion = distorsion;

        }

        public Apprentissage(ReseauNeurone neuronet, MnistDatabase baseApprentissage, int nombreSorties, FormPrincipal frm, Boolean distorsion)
        {
            _RNN = neuronet;
            BaseApprentissage = baseApprentissage;
            nombre = baseApprentissage.imageCount;
            NombreSorties = nombreSorties;
            erreurReconnaissances = 0;
            bonneReconnaissances = 0;
            nombreBackPropagation = 0;
            _iEpochsCompleted = 0;
            _form = frm;
            m_HiPerfTime = new HiPerfTimer();
            archive = new Archive();

            this.Distorsion = distorsion;
            
        }

       //  double[] BackpropagateNeuralNet(double[] inputVector, double[] CibleVector, NeuroneSortieListe pMemorizedNeuronOutputs,
       //     byte Label, FormPrincipal frm)
        double[] BackpropagateNeuralNet(double[] inputVector, double[] CibleVector, NeuroneSortieListe pMemorizedNeuronOutputs,
            FormPrincipal frm,Boolean  distorsion)
        {

            _form = frm; 

           // double erreur = 0.0;
            double[] VecteurCourant = new double[NombreSorties];

            nombreBackPropagation++;

            CalculateNeuralNet(inputVector, CibleVector, VecteurCourant, pMemorizedNeuronOutputs, distorsion);

            return VecteurCourant;
          


        }


        public void BackPropagationThead()
        {
            try
            {
                uint compteur = 0;
                double erreur = 100.0;
                double eta = 0.90;  // learning rate - controls the maginitude of the increase in the change in weights. found by trial and error.
                double alpha = 0.04; 

                byte Label = 0;
                double dMSE;

                NeuroneSortieListe _memorizedNeuronOutputs = new NeuroneSortieListe();
              
                double[] VecteurEntree = new double[841];
                double[] VecteurCible = new double[10];
                double[] VecteurCourant = new double[10];

                //archive.Serialisation(_RNN);

                //while (erreur > 0.10 * 0.1 && _iEpochsCompleted < 40)
                while (_iEpochsCompleted < 70)
                {
                    // initialisation 

                    for (int i = 0; i < 841; i++)
                        VecteurEntree[i] = 0.0;

                    for (int i = 0; i < 10; i++)
                    {
                        VecteurCible[i] = 0.0;
                        VecteurCourant[i] = 0.0;

                    }

                    // Boucle 
                    if (Definitions.mnistApprentissageDatabase.indexNextPattern == 0)
                    {
                        m_HiPerfTime.Start();
                        Definitions.mnistApprentissageDatabase.RandomizePatternSequence();
                    }
                    // Randomize pattern 
                    // Pad 28 ==> 29

                    byte[] padImage = new byte[29*29];

                    //byte[][] padImage = new byte[29][];
                    //for (int i = 0; i < padImage.Length; ++i)
                    //    padImage[i] = new byte[29];


                    uint iPattern = BaseApprentissage.GetNextPattern();
                    Definitions.mnistApprentissageDatabase.result[iPattern].pixels.CopyTo(padImage, 0);
                    Label = Definitions.mnistApprentissageDatabase.result[iPattern].label;


                    // Label = BaseApprentissage.

                    for (int i = 0; i < 841; i++)
                        VecteurEntree[i] = 1.0;  // 1 Blanc et 0 noir


                    for (int hauteur = 0; hauteur < 28; ++hauteur)
                    {
                        for (int largeur = 0; largeur < 28; ++largeur)
                        {
                            VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[largeur + 28 * hauteur]) / 128.0 - 1.0;
                            //VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[hauteur][largeur]) / 128.0 - 1.0;
                        }
                    }

                    for (int i = 0; i < 10; i++)
                        VecteurCible[i] = -1.0;

                    VecteurCible[Label] = 1.0;

                    VecteurCourant = BackpropagateNeuralNet(VecteurEntree, VecteurCible, _memorizedNeuronOutputs, _form,this.Distorsion);

                    dMSE = 0.0;
                    for (int i = 0; i < 10; ++i)
                    {
                        dMSE += (VecteurCourant[i] - VecteurCible[i]) * (VecteurCourant[i] - VecteurCible[i]);
                    }
                    dMSE /= 2.0;
                    _dMSE += dMSE;
                    _dMSE200 += dMSE;

                    // Calcul seuil d'erreur 
                    if (dMSE <= (0.10 * Convert.ToDouble(OCR.Properties.Settings.Default.EstimatedCurrentMSE)))
                        Backpropagate = false;
                    else
                        Backpropagate = true;

                    _nn++;
                    int iBestIndex = 0;
                    double maxValue = -99.0;

                    for (int i = 0; i < 10; ++i)
                    {
                        if (VecteurCourant[i] > maxValue)
                        {
                            iBestIndex = i;
                            maxValue = VecteurCourant[i];
                        }
                    }

                    if (iBestIndex != Label)
                    {
                        erreurReconnaissances++;
                    }
                    else
                        bonneReconnaissances++;

                     // make step
                    string s = "";
                    if (_nn >= 200)
                    {
                        _dMSE200 /= 200;
                        erreur = _dMSE200;
                        s = "MSE:" + _dMSE200.ToString();

                        double partielpourcentage = ((float)erreurReconnaissances * 100) / (float)Definitions.mnistApprentissageDatabase.indexNextPattern;

                        _form.Invoke(_form._DelegateAddObject, new Object[] { 2, partielpourcentage });
                      
                        _dMSE200 = 0;
                        _nn = 0;
                    }

                    s = String.Format("{0} Miss Number:{1} Bonne reo {2}", Convert.ToString(Definitions.mnistApprentissageDatabase.indexNextPattern), erreurReconnaissances, bonneReconnaissances);
                    if (_form != null)
                        _form.Invoke(_form._DelegateAddObject, new Object[] { 5, s });

                    if (Definitions.mnistApprentissageDatabase.indexNextPattern >= nombre - 1)
                    {
                        m_HiPerfTime.Stop();
                        _dMSE /= Definitions.mnistApprentissageDatabase.indexNextPattern;
                        Definitions.mnistApprentissageDatabase.indexNextPattern = 0;

                        double pourcentage = ((float)erreurReconnaissances * 100) / (float)Definitions.mnistApprentissageDatabase.imageCount;

                        s = String.Format("Epochs:{0}, Pourcentage:{1}, Temps {2}, erreurs:{3} ",
                        Convert.ToString(_iEpochsCompleted + 1), Convert.ToString(pourcentage), m_HiPerfTime.Duration, erreurReconnaissances.ToString());

                        if (_form != null)
                            _form.Invoke(_form._DelegateAddObject, new Object[] { 3, s });

                        erreurReconnaissances = 0;
                        bonneReconnaissances = 0;
                        _iEpochsCompleted++;
                        Definitions.mnistApprentissageDatabase.indexNextPattern = 0;
                        _dMSE = 0;
                    }
                  
                    compteur++;

                    if (Backpropagate == true)
                        Definitions.rnn.Backpropagate(VecteurCourant, VecteurCible, 0, _memorizedNeuronOutputs, nombreBackPropagation);


                }

                archive.Serialisation(Definitions.rnn);
                

            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

 
        }

        public void ApprentissageThead()
        {
            try
            {
                uint compteur = 0;
                double erreur = 100.0;
                double eta = 0.90;  // learning rate - controls the maginitude of the increase in the change in weights. found by trial and error.
                double alpha = 0.04;

                char Label ;
                double dMSE;

                NeuroneSortieListe _memorizedNeuronOutputs = new NeuroneSortieListe();

                Double size = Math.Sqrt(TrainMatrix[0].Length);
                Double taille = TrainMatrix[0].Length;

                double[] VecteurEntree = new double[(int)taille];
                double[] VecteurCible = new double[NombreSorties];
                double[] VecteurCourant = new double[NombreSorties];

                 //   archive.Serialisation(_RNN);

                while ( _iEpochsCompleted < 30)
                //while (erreur > 0.10 * 0.1 && _iEpochsCompleted < 20)
                {
                    // initialisation 

                    for (int i = 0; i < taille; i++)
                        VecteurEntree[i] = 0.0;

                    for (int i = 0; i < NombreSorties; i++)
                    {
                        VecteurCible[i] = 0.0;
                        VecteurCourant[i] = 0.0;

                    }

                    // Boucle 
                    if (Definitions.IndexPatternCourant == 0)
                    {
                        m_HiPerfTime.Start();
                        Helpers.ShuffleRows(TrainMatrix);
                        //Definitions.mnistApprentissageDatabase.RandomizePatternSequence();
                    }
                    // Randomize pattern 
                    // Pad 28 ==> 29

                    byte[] padImage = new byte[SizePattern * SizePattern];

                    //byte[][] padImage = new byte[29][];
                    //for (int i = 0; i < padImage.Length; ++i)
                    //    padImage[i] = new byte[29];

                    uint iPattern = (uint)Definitions.IndexPatternCourant;

                    Label = (char)TrainMatrix[iPattern][0];

                    //uint iPattern = BaseApprentissage.GetNextPattern();
                   // Definitions.mnistApprentissageDatabase.result[iPattern].pixels.CopyTo(padImage, 0);
                    //Label = Definitions.mnistApprentissageDatabase.result[iPattern].label;


                    // Label = BaseApprentissage.

                    for (int i = 0; i < SizePattern * SizePattern; i++)
                        VecteurEntree[i] = 1.0;  // 1 Blanc et 0 noir

                    try
                    {

                        for (int hauteur = 1; hauteur < 32 * 32; ++hauteur)
                            VecteurEntree[hauteur] = TrainMatrix[iPattern][hauteur] / 128.0 - 1.0;
                    }
                    catch (Exception err)
                    {
                        Console.WriteLine(err.Message);
                    }

                    //for (int hauteur = 0; hauteur < 32; ++hauteur)
                    //{
                    //    for (int largeur = 0; largeur < 32; ++largeur)
                    //    {
                    //        VecteurEntree[1 + largeur + 32 * (hauteur + 1)] = TrainMatrix[iPattern][largeur + 32 * hauteur + 1] / 128.0 -1.0 ;
                    //    }
                        
                    //}


                    //for (int hauteur = 0; hauteur < 32; ++hauteur)
                    //{
                    //    for (int largeur = 0; largeur < 32; ++largeur)
                    //    {
                    //        VecteurEntree[1 + largeur + 32 * (hauteur + 1)] = (double)((int)(byte)padImage[largeur + 32 * hauteur]) / 128.0 - 1.0;
                    //        //VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[hauteur][largeur]) / 128.0 - 1.0;
                    //    }
                    //}

                    for (int i = 0; i < NombreSorties; i++)
                        VecteurCible[i] = -1.0;
                    // 90 65 
                    // 122 97 

                    int x = (int)Label;
                    int n = DetermineIndex(x);
                    VecteurCible[n] = 1.0;

                    //Chiffres 48
                    // Majuscules 65 
                    // Minuscules 97
                   // int n = Label - 65; // -97;
                  //  int n = Label - 97; // -97;
                  //  int n = Label - 48; // -97;
                  //  VecteurCible[DetermineIndex(n)] = 1.0;
                  //  VecteurCible[n] = 1.0;

                    VecteurCourant = BackpropagateNeuralNet(VecteurEntree, VecteurCible, _memorizedNeuronOutputs, _form,false);

                    dMSE = 0.0;
                    for (int i = 0; i < NombreSorties; ++i)
                    {
                        dMSE += (VecteurCourant[i] - VecteurCible[i]) * (VecteurCourant[i] - VecteurCible[i]);
                    }
                    dMSE /= 2.0;
                    _dMSE += dMSE;
                    _dMSE200 += dMSE;


                    _nn++;
                    int iBestIndex = 0;
                    double maxValue = -99.0;

                    for (int i = 0; i < NombreSorties; ++i)
                    {
                        if (VecteurCourant[i] > maxValue)
                        {
                            iBestIndex = i;
                            maxValue = VecteurCourant[i];
                        }
                    }

                    if (iBestIndex != n) // Label)
                    {
                        erreurReconnaissances++;
                    }
                    else
                        bonneReconnaissances++;

                    // make step
                    string s = "";
                    if (_nn >= 200)
                    {
                        _dMSE200 /= 200;
                        erreur = _dMSE200;
                        s = "MSE:" + _dMSE200.ToString();

                        double partielpourcentage = (erreurReconnaissances * 100) / Definitions.IndexPatternCourant;

                        _form.Invoke(_form._DelegateAddObject, new Object[] { 2, partielpourcentage });

                        _dMSE200 = 0;
                        _nn = 0;
                    }

                    s = String.Format("{0} Miss Number:{1} Bonne reco {2}", Convert.ToString(Definitions.IndexPatternCourant), erreurReconnaissances, bonneReconnaissances);
                    if (_form != null)
                        _form.Invoke(_form._DelegateAddObject, new Object[] { 5, s });

                    if (Definitions.IndexPatternCourant >= TrainMatrix.Length - 1)
                    {
                        m_HiPerfTime.Stop();
                        _dMSE /= Definitions.IndexPatternCourant;
                        Definitions.IndexPatternCourant = 0;

                        double pourcentage = (erreurReconnaissances * 100) / TrainMatrix.Length;

                        s = String.Format("Epochs:{0}, Pourcentage:{1}, Temps {2}, Miss:{3} ",
                        Convert.ToString(_iEpochsCompleted + 1), Convert.ToString(pourcentage), m_HiPerfTime.Duration, erreurReconnaissances.ToString());

                        if (_form != null)
                            _form.Invoke(_form._DelegateAddObject, new Object[] { 3, s });

                        erreurReconnaissances = 0;
                        bonneReconnaissances = 0;

                        _iEpochsCompleted++;
                        Definitions.IndexPatternCourant = 0;
                        _dMSE = 0;
                    }
                    else
                        Definitions.IndexPatternCourant++;

                    compteur++;

                    Definitions.rnn.Backpropagate(VecteurCourant, VecteurCible, 0, _memorizedNeuronOutputs, nombreBackPropagation);


                }

                archive.Serialisation(_RNN);
                //archive.Serialisation(Definitions.rnn);


            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }


        }

        double Error(double[] cible, double[] sortie) // sum absolute error. could put into NeuralNetwork class.
        {
            double sum = 0.0;
            for (int i = 0; i < cible.Length; ++i)
                sum += Math.Abs(cible[i] - sortie[i]);
            return sum;
        }

        private int DetermineIndex(int ascii)
        {
             //Chiffres 48
                    // Majuscules 65 
                    // Minuscules 97
            if (ascii >= 48 && ascii <= 57)
                return ascii - 48;
            else if (ascii >= 65 && ascii <= 90)
                return ascii - 65;
            else if (ascii >= 97 && ascii <= 122)
                return ascii - 97;
            return 0;
        }

    }
}
