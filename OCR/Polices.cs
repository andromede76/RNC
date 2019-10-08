using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.IO;

namespace OCR
{
    [Serializable()]
    public class Polices:List<Police>
    {

        public void Enregistrer(String chemin)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Polices));
            StreamWriter ecrivain = new StreamWriter(chemin);
            serializer.Serialize(ecrivain, this);
            ecrivain.Close();

        }

        public static Polices Charger(string chemin)
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(Polices));
            StreamReader lecteur = new StreamReader(chemin);
            Polices p = (Polices)deserializer.Deserialize(lecteur);
            lecteur.Close();

            return p;
        }
    }
}
