using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Neurones
{
    public class NeuroneSortieListe:List<NeuroneSortie>
    {
       public NeuroneSortieListe()
       {
       
       }
        public NeuroneSortieListe(int capacity)
            : base(capacity)
        {
            
        }
        public NeuroneSortieListe(IEnumerable<NeuroneSortie> collection)
            : base(collection)
        {
            
        }
    }
}
