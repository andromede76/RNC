using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using RNC.Couches;
using RNC.Neurones;

namespace RNC.Reseau
{
    public class ReseauNeurone
    {
        public double TauxApprentissage;
        public List<CoucheBase> Couches;

        public int NombreCouches;
        public Size inputDesignedPatternSize;

        public int etat; 

        private double NbPalierBackpropagation;
        private double Eta;
        private double MinimumEta;
        private double EtaInitial;

        public ReseauNeurone()
        {
        }
        public ReseauNeurone(double _NbPalierBackpropagation, double _Eta, double _MinimumEta, double _EtaInitial)
        {
            TauxApprentissage = .001;
            Couches = new List<CoucheBase>();

            this.NbPalierBackpropagation = _NbPalierBackpropagation;
            this.Eta = _Eta;
            this.MinimumEta = _MinimumEta;
            this.EtaInitial = _EtaInitial;

        }

        public void Calculate(double[] INVector, double[] CibleVector, double[] CourantVector, NeuroneSortieListe pNeuronOutputs)
        {
            int count = 0;
            var premiereCouche = Couches.First();

            if (Couches.Count > 1) 
            {
                foreach (var neuron in premiereCouche.neurones)
                {
                    if (count < premiereCouche.neurones.Count)
                    {
                        //neuron.Sortie = 9;
                        neuron.Sortie = INVector[count];
                        count++;
                    }
                }


                // Calcul sur les couches cachées
                for (int i = 1; i < Couches.Count; i++)
                    Couches[i].Calculate();

                // Charge la sortie 
                if (CourantVector != null)
                {
                    var lit = Couches[Couches.Count - 1];

                    for (int i = 0; i < lit.neurones.Count; i++)
                        CourantVector[i] = lit.neurones[i].Sortie;
                }


                if (pNeuronOutputs != null)
                {
                    pNeuronOutputs.Clear();
                    pNeuronOutputs.Capacity = Couches.Count;

                    foreach (CoucheBase cc in Couches)
                    {
                        var coucheSortie = new NeuroneSortie(cc.neurones.Count);
                        for (int i = 0; i < cc.neurones.Count; i++)
                           coucheSortie.Add(cc.neurones[i].Sortie);

                        pNeuronOutputs.Add(coucheSortie);

                    }
                }

            }

        }
        /// <summary>
        /// Commence avec la dernière couche
        /// </summary>
        /// <param name="actualOutput"></param>
        /// <param name="desiredOutput"></param>
        /// <param name="count"></param>
        /// <param name="neuroneSorties"></param>
        public void Backpropagate(double[] actualOutput, double[] desiredOutput, int count, 
             NeuroneSortieListe pMemorizedNeuronOutputs,int nombreBackPropagation)
        {
            int nombreNeurones = 0;

            // Au moins 2 couches 
            if (Couches.Count < 2)
                return;

            if ((actualOutput == null) || (desiredOutput == null))
                return;

            if (nombreBackPropagation % NbPalierBackpropagation == 0)
            {
                double eta = this.EtaInitial;
                eta *= this.Eta; 
                if (eta < this.MinimumEta)
                    eta = this.MinimumEta;

                this.TauxApprentissage = eta;

            }

            int taille = Couches.Count;
            nombreNeurones = Couches[Couches.Count - 1].neurones.Count;

            var dErr_wrt_dXLast = new ErreurListe(nombreNeurones);
            var differentials = new List<ErreurListe>(taille);

            // Calcul sur la dernière couche

            for (int i = 0; i < nombreNeurones; i++)
            {
                dErr_wrt_dXLast.Add(actualOutput[i] - desiredOutput[i]);
            }

            for (int i = 0; i < taille - 1; i++)
            {
                var m_differentials = new ErreurListe(Couches[i].neurones.Count);
                for (int k = 0; k < Couches[i].neurones.Count; k++)
                    m_differentials.Add(0.0);
                differentials.Add(m_differentials);
            }
            differentials.Add(dErr_wrt_dXLast);

            bool Memorise = (pMemorizedNeuronOutputs != null);

            for (int j = taille - 1; j > 0; j--)
            {
                if (Memorise != false)
                {
                    Couches[j].Backpropagate(differentials[j], differentials[j - 1],
                    pMemorizedNeuronOutputs[j], pMemorizedNeuronOutputs[j - 1], TauxApprentissage);
                }
                else
                {
                    Couches[j].Backpropagate(differentials[j], differentials[j - 1],
                   null, null, TauxApprentissage);
                }


               
            }

            differentials.Clear();

        }

       

        public void miseAjourPoids(double[] desiredOutput, double eta, double alpha)
        {
             double[] oGrads;
             double[] hGrads;

             // NombreCouches;            

             int nbNeurone = this.Couches[Couches.Count - 1].neurones.Count;
             oGrads = new double[nbNeurone];

             //// 1. Calcul sortie gradients
             for (int i = 0; i < oGrads.Length; ++i)
             {
                 double derivative = (1 - this.Couches[Couches.Count - 1].neurones[i].Sortie) * (1 + this.Couches[Couches.Count - 1].neurones[i].Sortie); // derivative of tanh
                 //double derivative = (1 -  outputs[i]) * (1 + outputs[i]); // derivative of tanh
                 oGrads[i] = derivative * (desiredOutput[i] - this.Couches[Couches.Count - 1].neurones[i].Sortie);
             }

             nbNeurone = this.Couches[Couches.Count - 2].neurones.Count;
             hGrads = new double[nbNeurone];
                           
            // 2. Calcule gradients couche cachée
             for (int i = 0; i < hGrads.Length; ++i)
             {
                 double derivative = (1 - this.Couches[Couches.Count - 1].neurones[i].Sortie) * (1 + this.Couches[Couches.Count - 1].neurones[i].Sortie);
                 double sum = 0.0;

             }
           

        }

        public void CalculateTest(double[] INVector, double[] CibleVector, double[] CourantVector, NeuroneSortieListe pNeuronOutputs)
        {
            int count = 0;
            var premiereCouche = Couches.First();

            if (Couches.Count > 1)
            {
                foreach (var neuron in premiereCouche.neurones)
                {
                    if (count < premiereCouche.neurones.Count)
                    {
                        //neuron.Sortie = 9;
                        neuron.Sortie = INVector[count];
                        count++;
                    }
                }

                // Bug 
                Couches[1].CouchePrecedente = premiereCouche;


                // Calcul sur les couches cachées
                for (int i = 1; i < Couches.Count; i++)
                {
                    Couches[i].Calculate();
                    if (i < 4)
                       Couches[i + 1].CouchePrecedente = Couches[i];
                }

                // Charge la sortie 
                if (CourantVector != null)
                {
                    var lit = Couches[Couches.Count - 1];

                    for (int i = 0; i < lit.neurones.Count; i++)
                        CourantVector[i] = lit.neurones[i].Sortie;
                }


                if (pNeuronOutputs != null)
                {
                    pNeuronOutputs.Clear();
                    pNeuronOutputs.Capacity = Couches.Count;

                    foreach (CoucheBase cc in Couches)
                    {
                        var coucheSortie = new NeuroneSortie(cc.neurones.Count);
                        for (int i = 0; i < cc.neurones.Count; i++)
                            coucheSortie.Add(cc.neurones[i].Sortie);

                        pNeuronOutputs.Add(coucheSortie);

                    }
                }

            }

        }
    }
}
