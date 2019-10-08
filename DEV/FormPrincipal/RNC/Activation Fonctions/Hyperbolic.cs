using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace RNC.Activation_Fonctions
{
    public class Hyperbolic : IActivateFunction
    {
        public double Fonction(double x)
        {
            if (x < -10.0)
                return -1.0;

            if (x > 10.0)
                return 1.0;

            return Math.Tanh(x); 
        }

        public double derivee(double x)
        {
            return   (1 - x) * (1 + x); // derivative of tanh
         
        }
    }
}
