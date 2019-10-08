using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using RNC.Reseau;
using RNC.Couches;
using RNC.Donnees;

using OCR.Properties;
using OCR.RNN;



namespace OCR
{
    public class Definitions
    {
        public static MnistImage[] trainImages ;
        public static MnistImage[] testImages ;

        public static ReseauNeurone rnn;
        public static MnistDatabase mnistApprentissageDatabase;
        public static MnistDatabase mnistTestDatabase;

        public static Apprentissage apprentissage;
        public static Test test;

        public static int IndexPatternCourant;
        
    }
}
