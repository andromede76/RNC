using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC
{
    public class Neurone
    {
        public String Label;
        public Double Sortie;
        public List<Dentrite> dentrite;
        public int nombreConnections;

        public Neurone()
        {
            Initialize();
            Label = "";
            Sortie = 0.0;
            dentrite = new List<Dentrite>();

        }

        public Neurone(String label)
        {
            Initialize();
            Label = label;
            Sortie = 0.0;
            dentrite = new List<Dentrite>();

        }

        public void AddConnection(uint iNeurone, uint iPoids)
        {
            Dentrite den = new Dentrite(iNeurone, iPoids,0);
            dentrite.Add(den);  

        }

        public void AddConnection(uint iNeurone, uint iPoids,uint iConnection)
        {
            Dentrite den = new Dentrite(iNeurone, iPoids,iConnection);
            dentrite.Add(den);

        }

        public void AddConnection(Dentrite den)
        {
            dentrite.Add(den); 
            
        }

        private void Initialize()
        {

        }

    }
}
