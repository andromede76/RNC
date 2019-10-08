using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace RNC.Couches
{
    public class CoucheEntree:CoucheBase
    {
        public CoucheEntree()
        {
        }
        public CoucheEntree(String label, Size kernel)
        {
            Label = label;
            featureMapSize = kernel;
            nombreFeatureMaps = 1;
        }

        public override void Initialize()
        {
            floatingPointWarning = false;
            CreationCouche();
        }

        protected override void CreationCouche()
        {
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

            // 
            CouchePrecedente = null;
            NombreNeurones = nombreFeatureMaps * featureMapSize.Width * featureMapSize.Height;
            neurones = new List<Neurone>();
            
            poids = null;
            NombrePoids = 0;

            for (int i = 0; i < NombreNeurones; i++)
            {
                String lb = String.Format("Couche {0}, Neuron {1}", i, i);
                neurones.Add(new Neurone(lb));
            }
        }
 
    }
}
