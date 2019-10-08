using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC
{
    public class Poids
    {
        public string Label;
        public double Value;

        public Poids()
        {
            Label = "";
            Value = 0.0;
        }

        public Poids(string label, double value)
        {
           this.Label = label ;
           this.Value =  value ;
        }
    }
}
