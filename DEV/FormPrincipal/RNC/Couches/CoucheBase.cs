using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

using System.Xml;
using System.Xml.Serialization;

using  RNC.Reseau;
using RNC.Neurones;

using RNC.Activation_Fonctions;

namespace RNC.Couches
{
    [Serializable(), XmlInclude(typeof(CoucheSupConvolution)),
    XmlInclude(typeof(CoucheEntree)),
    XmlInclude(typeof(CoucheFullConnecte)),
    XmlInclude(typeof(CoucheSortie)),
    XmlInclude(typeof(CoucheConvolution)),
    XmlInclude(typeof(CoucheSimple))
    ]
    public class CoucheBase
    {
        public List<Poids> poids;
        public List<Neurone> neurones;

        public Hyperbolic hyperbolic;

        public string Label;

        public CoucheBase CouchePrecedente;

        public Size featureMapSize;
        public int nombreFeatureMaps;
        public bool floatingPointWarning;

        public int NombreNeurones;
        public int NombrePoids;

        //private SigmoidFunction m_sigmoid;
        // relu 
        // HyperTangente

        public CoucheBase()
        {
            Label = "";
            CouchePrecedente = null;

            poids = new List<Poids>();
            neurones = new List<Neurone>(); 


        }

        public CoucheBase(string label, CoucheBase couchePrecedente)
        {
            this.Label = label ;
            this.CouchePrecedente = couchePrecedente;

            poids = new List<Poids>();
            neurones = new List<Neurone>();


        }

        public virtual void Initialize()
        {
            floatingPointWarning = false;
            CreationCouche();

           
        }


        //public void Initialize()
        //{
        //    this.poids.Clear();
        //    this.neurones.Clear(); 

        //}

        public void Calculate()
        {
            double sum = 0;
            hyperbolic = new Hyperbolic();

            foreach (var nn in neurones)
            {
                foreach (var connect in nn.dentrite)
                {
                    if (connect == nn.dentrite[0])
                        sum = poids[(int)connect.PoidstIndex].Value;
                    else
                    {
                        sum += poids[(int)connect.PoidstIndex].Value * (CouchePrecedente.neurones[(int)connect.NeuroneIndex].Sortie);
                    }
                }

                nn.Sortie = hyperbolic.Fonction(sum);
            }

          

        }

        public void Backpropagate(
            ErreurListe dErr_wrt_dXn ,
            ErreurListe dErr_wrt_dXnm1,
            NeuroneSortie CoucheCouranteSortie,
            NeuroneSortie CourantPrecedenteSortie,
            double etaTauxApprentissage
            )
        {
            try
            {
                double sortie;
                double oldValue;
                double newValue;

                ErreurListe dErr_wrt_dYn = new ErreurListe(neurones.Count);

                double[] dErr_wrt_dWn = new double[poids.Count];
                for (int i = 0; i < poids.Count; i++)
                    dErr_wrt_dWn[i] = 0.0;

                bool memorise = (CoucheCouranteSortie != null) && (CourantPrecedenteSortie != null);

                for (int i = 0; i < neurones.Count; i++)
                {
                    if (memorise != false)
                        sortie = CoucheCouranteSortie[i];
                    else
                        sortie = neurones[i].Sortie;

                        dErr_wrt_dYn.Add(hyperbolic.derivee(sortie) * dErr_wrt_dXn[i]);
                }


                //////////////
                uint indexNeurone;

                int _Index = 0;
                foreach (Neurone nn in neurones)
                {
                    foreach (Dentrite dd in nn.dentrite)
                    {
                        indexNeurone = dd.NeuroneIndex;

                        if (indexNeurone == 0xffffffff)
                            sortie = 1.0;

                        else
                        {
                            if (memorise != false)
                                sortie = CourantPrecedenteSortie[(int)indexNeurone];
                            else
                                sortie = CouchePrecedente.neurones[(int)indexNeurone].Sortie;
                          
                        }
                        dErr_wrt_dWn[dd.PoidstIndex] += dErr_wrt_dYn[_Index] * sortie;

                       
                    }
                    _Index++;
                }

                _Index = 0;
                int nIndex = 0;

                foreach (Neurone neuron in neurones)
                {
                    foreach (Dentrite dd in neuron.dentrite)
                    {
                        indexNeurone = dd.NeuroneIndex;

                        if (indexNeurone != 0xffffffff)
                        {
                            nIndex = (int)indexNeurone;
                            dErr_wrt_dXnm1[nIndex] += dErr_wrt_dYn[_Index] * poids[(int)dd.PoidstIndex].Value;
                        }
                 
                    }

                    _Index++;

                }


                // Mise a jour des poids 
                for (int j = 0; j < poids.Count; ++j)
                {
                    oldValue = poids[j].Value;
                    newValue = oldValue - etaTauxApprentissage * dErr_wrt_dWn[j];
                    poids[j].Value = newValue; 

                }


            
 
            }
            catch (Exception err)
            {
                MessageBox.Show(err.Message);
            }

        }

        protected virtual void CreationCouche()
        {

        }
    }
}
