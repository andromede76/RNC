using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.Threading;
using System.Threading.Tasks;

namespace RNC.Couches
{
    public class CoucheSimple : CoucheBase
    {
        protected int kernelsize;

        public CoucheSimple()
        {
        }

        public CoucheSimple(String label, CoucheBase couchePrecedente)
        {
            Label = label;
            CouchePrecedente = couchePrecedente;
            poids = null;
            nombreFeatureMaps = couchePrecedente.nombreFeatureMaps;
            kernelsize = 2;

            Size fmSize = Size.Empty;
            fmSize.Width = (int)Math.Floor((double)couchePrecedente.featureMapSize.Width / 2);
            fmSize.Height = (int)Math.Floor((double)couchePrecedente.featureMapSize.Height / 2);

            featureMapSize = fmSize;
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

            neurones = new List<Neurone>();
            poids = new List<Poids>();

            if (CouchePrecedente != null)
            {
                NombreNeurones = nombreFeatureMaps * featureMapSize.Width * featureMapSize.Height;
                NombrePoids = 1;

                for (int i = 0; i < NombreNeurones; i++)
                {
                    String lb = String.Format("Layer {0} , Neuron {1}", Label, i);
                    neurones.Add(new Neurone(lb));
                }

                rdm = new Random();

                for (int j = 0; j < NombrePoids; j++)
                {
                    String lb = String.Format("Layer {0}, Poids {1}", Label, j);
                    double initPoids = 0.05 * (2.0 * rdm.NextDouble() - 1.0);
                    poids.Add(new Poids(lb, initPoids));
                }

                int[] kernelTemplate = CreateKernelTemplate(kernelsize, CouchePrecedente.featureMapSize.Width);

                for (int indexFeatureMap = 0; indexFeatureMap < nombreFeatureMaps; indexFeatureMap++)
                {
                    for (int hauteur = 0; hauteur < featureMapSize.Height; hauteur++)
                    {
                        for (int largeur = 0; largeur < featureMapSize.Width; largeur++)
                        {
                            IndexPoids = 0;
                            Neurone n = neurones[largeur + hauteur * featureMapSize.Width + indexFeatureMap * featureMapSize.Width * featureMapSize.Height];

                            int nombreConnection = kernelsize * kernelsize; 
                            n.nombreConnections = nombreConnection;

                            for (int k = 0; k < kernelsize * kernelsize; k++)
                            {
                                int iNeurons = kernelsize * largeur + (kernelsize * hauteur * CouchePrecedente.featureMapSize.Width) + kernelTemplate[k];
                                int connID = k;
                                n.AddConnection((uint)iNeurons, (uint)IndexPoids, (uint)connID);
                              
                            }
                        
                         
                        }
                    }
                }

            }
        }

        private int[] CreateKernelTemplate(int size, int fmWidth)
        {
            int[] kernelTemplate = new int[size * size];

            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    kernelTemplate[i + j * size] = i + j * fmWidth;
                }
            }
            return kernelTemplate;
        }

    }
}
