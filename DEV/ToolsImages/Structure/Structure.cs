using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsImages.Structures
{
    
        public class Structure
        {
            public int X { get; set; }
            public int Y { get; set; }
            public int Largeur { get; set; }
            public int Hauteur { get; set; }
            public int ligne { get; set; }
            public int mot { get; set; }
            public int caractere { get; set; }
            public int Valeur { get; set; }
            public int TDG { get; set; }

            public Structure(int x, int y)
            {
                this.X = x;
                this.Y = y;


            }

            public Structure(int x, int y, int largeur, int hauteur)
            {
                this.X = x;
                this.Y = y;
                this.Largeur = largeur;
                this.Hauteur = hauteur;

            }


        }
    }

