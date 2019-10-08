using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Activation_Fonctions
{
    public class Sigmoid : IActivateFunction
    {
        public double Fonction(double x)
        {
            if (x < -45.0) 
                return 0.0;
            if (x > 45.0) 
                return 1.0;

            return 1.0 / (1.0 + Math.Exp(-x));
        }

        public double derivee(double x)
        {
            return 0.0;
        }
    }
}
