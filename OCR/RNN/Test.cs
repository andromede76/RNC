using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using RNC;
using RNC.Reseau;
using RNC.Donnees;
using RNC.Neurones;

using OCR;
using OCR.Tools;

namespace OCR.RNN
{
    public class Test : ForwardPropagation
    {
        #region Parametres

        public uint erreurReconnaissances;
        private HiPerfTimer m_HiPerfTime;
        private MnistDatabase BaseApprentissage;
        private int nombre;
        FormPrincipal _form;

        int Entree = 0;
        int Sortie = 0;
        int Taille = 0;

        #endregion 


        public Test(ReseauNeurone neuronet, MnistDatabase baseTest, FormPrincipal frm)
        {
            _RNN = neuronet;
            BaseApprentissage = baseTest;
            nombre = BaseApprentissage.imageCount;
            erreurReconnaissances = 0;
            _form = frm;
            m_HiPerfTime = new HiPerfTimer();
            
        }

        public Test(ReseauNeurone neuronet, MnistDatabase baseTest, FormPrincipal frm, int entree,int sortie,int taille)
        {
            _RNN = neuronet;
            BaseApprentissage = baseTest;
            nombre = BaseApprentissage.imageCount;
            erreurReconnaissances = 0;
            _form = frm;
            m_HiPerfTime = new HiPerfTimer();

            this.Entree = entree;
            this.Sortie = sortie;
            this.Taille = taille;

        }

        double[] BackpropagateNeuralNet(double[] inputVector, double[] CibleVector,  NeuroneSortieListe _memorizedNeuronOutputs,
          byte Label, FormPrincipal frm)
        {

            _form = frm;

            // double erreur = 0.0;
            int nbreElement = CibleVector.Count();

            //double[] VecteurCourant = new double[10];
            double[] VecteurCourant = new double[nbreElement];


            CalculateNeuralNetTest(inputVector, CibleVector, VecteurCourant, _memorizedNeuronOutputs);

            return VecteurCourant;
            


        }

        public void TestPattern(int pattern)
        {
           // int patternCourant = pattern;
            byte Label;
            int matrice = Entree * Entree;
         
            NeuroneSortieListe _memorizedNeuronOutputs = new NeuroneSortieListe();

            double[] VecteurEntree = new double[matrice];
            double[] VecteurCible = new double[Sortie];
            double[] VecteurCourant = new double[Sortie];

            //double[] VecteurEntree = new double[841];
            //double[] VecteurCible = new double[10];
            //double[] VecteurCourant = new double[10];

            m_HiPerfTime.Start();

            //for (int i = 0; i < 841; i++)
            for (int i = 0; i < matrice; i++)
                {
                    VecteurEntree[i] = 0.0;
                }
            for (int i = 0; i < Sortie; i++)
                {
                    VecteurCible[i] = 0.0;
                    VecteurCourant[i] = 0.0;

                }
                byte[] padImage = new byte[29 * 29];

                //uint iPattern = BaseApprentissage.GetNextPattern();

               // patternCourant = (int)iPattern;
                BaseApprentissage.result[pattern].pixels.CopyTo(padImage, 0);
                Label = BaseApprentissage.result[pattern].label;

                for (int i = 0; i < matrice; i++)
                    VecteurEntree[i] = 1.0;  // 1 Blanc et 0 noir


                for (int hauteur = 0; hauteur < Entree; ++hauteur)
                {
                    for (int largeur = 0; largeur < Entree; ++largeur)
                    {
                        VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[largeur + 28 * hauteur]) / 128.0 - 1.0;
                        //VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[hauteur][largeur]) / 128.0 - 1.0;
                    }
                }

                for (int i = 0; i < Sortie; i++)
                    VecteurCible[i] = -1.0;

                VecteurCible[Label] = 1.0;

                VecteurCourant = BackpropagateNeuralNet(VecteurEntree, VecteurCible, _memorizedNeuronOutputs, Label, _form);

                int iBestIndex = 0;
                double maxValue = -99.0;

                for (int i = 0; i < Sortie; ++i)
                {
                    if (VecteurCourant[i] > maxValue)
                    {
                        iBestIndex = i;
                        maxValue = VecteurCourant[i];
                    }
                }

               
                string s = "";
                s = "lblChiffreReconnu : " + iBestIndex.ToString();
             
                if (_form != null)
                    _form.Invoke(_form._DelegateAddObject, new Object[] { 8, s });

                if (_form != null)
                    _form.Invoke(_form._DelegateAddObject, new Object[] { 9, VecteurCourant });


            }

     



        public void TestThread()
        {
            int patternCourant = 0;
            byte Label;
            int reconnu =0;
            int nonreconnu = 0;
            int matrice = Taille * Taille;

            FileLog.SetLogFile(".\\Log", "Test_");

            FileLog.Info("Début du Test");

            NeuroneSortieListe _memorizedNeuronOutputs = new NeuroneSortieListe();

            double[] VecteurEntree = new double[matrice];
            double[] VecteurCible = new double[Sortie];
            double[] VecteurCourant = new double[Sortie];

            m_HiPerfTime.Start();

            while (patternCourant < nombre-1)
            {

                for (int i = 0; i < matrice; i++)
                {
                    VecteurEntree[i] = 0.0;
                }
                for (int i = 0; i < Sortie; i++)
                {
                    VecteurCible[i] = 0.0;
                    VecteurCourant[i] = 0.0;

                }
                byte[] padImage = new byte[29 * 29];

                uint iPattern = BaseApprentissage.GetNextPattern();

                patternCourant = (int)iPattern;
                BaseApprentissage.result[patternCourant].pixels.CopyTo(padImage, 0);
                Label = BaseApprentissage.result[patternCourant].label;

                for (int i = 0; i < matrice; i++)
                    VecteurEntree[i] = 1.0;  // 1 Blanc et 0 noir


                for (int hauteur = 0; hauteur < Entree; ++hauteur)
                {
                    for (int largeur = 0; largeur < Entree; ++largeur)
                    {
                        VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[largeur + 28 * hauteur]) / 128.0 - 1.0;
                        //VecteurEntree[1 + largeur + 29 * (hauteur + 1)] = (double)((int)(byte)padImage[hauteur][largeur]) / 128.0 - 1.0;
                    }
                }

                for (int i = 0; i < Sortie; i++)
                    VecteurCible[i] = -1.0;

                VecteurCible[Label] = 1.0;

                VecteurCourant = BackpropagateNeuralNet(VecteurEntree, VecteurCible,_memorizedNeuronOutputs,  Label, _form);

                int iBestIndex = 0;
                double maxValue = -99.0;

                for (int i = 0; i < Sortie; ++i)
                {
                    if (VecteurCourant[i] > maxValue)
                    {
                        iBestIndex = i;
                        maxValue = VecteurCourant[i];
                    }
                }

                string s = "";
                if (iBestIndex != Label)
                {

                    nonreconnu++;

                    s = "Pattern No:" + patternCourant.ToString() + " Valeur reconnue:" + iBestIndex.ToString() + " Valeur actuelle:" + Label.ToString();
                    FileLog.Info(s);
                    if (_form != null)
                        _form.Invoke(_form._DelegateAddObject, new Object[] { 6, s });


                }
                else
                    reconnu++;

                s = "Image No:" + patternCourant.ToString() + " Nombre bonne reco:" + reconnu.ToString() + " Mauvaise reco:" + nonreconnu.ToString();

                if (_form != null)
                    _form.Invoke(_form._DelegateAddObject, new Object[] { 7, s });
                //else
                //{
                //    s = patternCourant.ToString() + ", Mis Nums:" + reconnu.ToString();
                //    if (_form != null)
                //        _form.Invoke(_form._DelegateAddObject, new Object[] { 7, s });
                //}

            }

        }
    }
}
