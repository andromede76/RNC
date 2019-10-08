using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Activation_Fonctions
{
    public interface IActivateFunction
    {
        double Fonction(double x);

        double derivee(double x);
    }
}
