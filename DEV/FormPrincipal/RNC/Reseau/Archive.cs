using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace RNC.Reseau
{
    public class Archive
    {
        public void Serialisation(object rnn)
        {
            String nomfichier = "rnc_" + DateTime.Now.Year.ToString() + "_" + DateTime.Now.Month.ToString() + "_" + DateTime.Now.Day.ToString() + "_" + 
                 DateTime.Now.Hour.ToString() + "H" + DateTime.Now.Minute.ToString() + ".xml";

          
            XmlSerializer xs = new XmlSerializer(rnn.GetType());
            using (StreamWriter wr = new StreamWriter(nomfichier))
            {
                xs.Serialize(wr, rnn);
            }
        }
        public ReseauNeurone deserialise(String fichier)
        {
            XmlSerializer xs = new XmlSerializer(typeof(ReseauNeurone));
            using (StreamReader rd = new StreamReader(fichier))
            {
                return xs.Deserialize(rd) as ReseauNeurone;
              
            }
        }
    }
}
