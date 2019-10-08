using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Neurones
{
    public class NeuroneSortie:List<double>
    {
         public NeuroneSortie()
        { 
        
        }
        public NeuroneSortie(int capacity)
            : base(capacity)
        {
            
        }
        public NeuroneSortie(IEnumerable<double> collection)
            : base(collection)
        {
            
        }

    }
}
