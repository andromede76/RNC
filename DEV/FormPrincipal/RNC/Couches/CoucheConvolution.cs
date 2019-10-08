using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

using System.Threading;
using System.Threading.Tasks;

namespace RNC.Couches
{
    public class CoucheConvolution : CoucheBase
    {
        protected int kernelsize;

        public CoucheConvolution()
        {
        }
        public CoucheConvolution(String label,CoucheBase couchePrecedente,int nombreMaps,int tailleKernel)
        {
            Label = label;
            CouchePrecedente = couchePrecedente;
            poids = null;
            Size fmSize = Size.Empty;
            fmSize.Width = couchePrecedente.featureMapSize.Width - 4;
            fmSize.Height = couchePrecedente.featureMapSize.Height - 4;

            featureMapSize = fmSize;
            nombreFeatureMaps = nombreMaps;
            kernelsize = tailleKernel;
        
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
                NombreNeurones = nombreFeatureMaps * (CouchePrecedente.featureMapSize.Width - 4) 
                    * (CouchePrecedente.featureMapSize.Height - 4);
                NombrePoids = nombreFeatureMaps * (kernelsize * kernelsize * CouchePrecedente.nombreFeatureMaps + 1);

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
                            IndexPoids = indexFeatureMap * ((int)Math.Pow(kernelsize, 2) * CouchePrecedente.nombreFeatureMaps + 1);
                            Neurone n = neurones[largeur + hauteur * featureMapSize.Width + indexFeatureMap * featureMapSize.Width * featureMapSize.Height];

                            int nombreConnection = ((kernelsize * kernelsize * CouchePrecedente.nombreFeatureMaps) + 1);
                            n.nombreConnections = nombreConnection;
                            // 26 connections 
                            n.AddConnection((uint)0xffffffff, (uint)IndexPoids); // Biais  

                            for (int k = 0; k < kernelsize * kernelsize; k++)
                            {
                                for (int l = 0; l < CouchePrecedente.nombreFeatureMaps; l++)
                                {
                                    int iNeurons = (largeur + (hauteur * CouchePrecedente.featureMapSize.Width) + kernelTemplate[k] +
                                      l * CouchePrecedente.featureMapSize.Width * CouchePrecedente.featureMapSize.Height);
                                    int connID = 1 + l + k * CouchePrecedente.nombreFeatureMaps;
                                    n.AddConnection((uint)iNeurons, (uint)IndexPoids++, (uint)connID);


                                }
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
