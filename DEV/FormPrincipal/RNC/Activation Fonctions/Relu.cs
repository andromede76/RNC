using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Activation_Fonctions
{
    public class Relu : IActivateFunction
    {
        public double Fonction(double x)
        {
            if (x > 0.0)
                return x;
            else
                return 0.0;
        }

        public double derivee(double x)
        {
            return 0.0;
        }
    }
}
