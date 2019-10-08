using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Reseau
{
    public class ErreurListe:List<double>
    {
        public ErreurListe()
        {
        }

        public ErreurListe(int capacite):base(capacite)
        {
        }

        public ErreurListe(IEnumerable<double> collection):base(collection)
        {

        }

    }
}
