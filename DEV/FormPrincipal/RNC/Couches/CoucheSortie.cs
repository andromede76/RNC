using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RNC.Couches
{
    public class CoucheSortie:CoucheBase
    {
        public CoucheSortie()
        {
        }
        public CoucheSortie(string label, CoucheBase couchePrecedente, int nombreSortie)
        {
            Label = label;
            int count = nombreSortie;
            CouchePrecedente = couchePrecedente;
            NombreNeurones = 0;
            NombrePoids = 0;
            featureMapSize = new Size(1, count);
            nombreFeatureMaps = 1;

            poids = null;
            neurones = null;

        }

        public override void Initialize()
        {
            floatingPointWarning = false;
            CreationCouche();
        }

        protected override void CreationCouche()
        {
            int IndexPoids;
            var rdm = new Random();


            if (NombreNeurones > 0 || neurones != null)
            {
                neurones = null;
                NombreNeurones = 0;
            }

            if (NombrePoids > 0 || poids != null)
            {
                poids = null;
                NombrePoids = 0;
            }

            if (CouchePrecedente != null)
            {
                NombreNeurones = nombreFeatureMaps * featureMapSize.Width * featureMapSize.Height;
                neurones = new List<Neurone>();
                poids = new List<Poids>();

                for (int i = 0; i < NombreNeurones; i++)
                {
                    String lb = String.Format("Layer {0}, Neuron {1}", Label, i);
                    neurones.Add(new Neurone(lb));
                }

                rdm = new Random();
                NombrePoids = NombreNeurones * (CouchePrecedente.NombreNeurones + 1);

                for (int j = 0; j < NombrePoids; j++)
                {
                    String lb = String.Format("Layer {0}, Poids {1}", Label, j);
                    double initPoids = 0.05 * (2.0 * rdm.NextDouble() - 1.0);
                    poids.Add(new Poids(lb, initPoids));
                }

                IndexPoids = 0;

                for (int j = 0; j < NombreNeurones; j++)
                {
                    Neurone n = neurones[j];
                    int connCount = CouchePrecedente.NombreNeurones + 1;
                    n.nombreConnections = connCount;
                    n.AddConnection((uint)0xffffffff, (uint)IndexPoids); // Biais  

                    for (int i = 0; i < CouchePrecedente.NombreNeurones; i++)
                    {
                        n.AddConnection((uint)i, (uint)IndexPoids++, (uint)i + 1);
                    }
                }
            }
        }
    }
}
