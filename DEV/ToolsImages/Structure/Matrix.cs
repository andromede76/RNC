using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsImages.Structures
{
     public enum Direction
    {
       N, O, NE, NO, E,SE,S,SO
    }

     public class Matrix
     {
        public int Largeur;
        public int Hauteur;



        public int[,] Values;

        public Matrix(int largeur,int hauteur)
        {
            this.Largeur = largeur;
            this.Hauteur = hauteur;

            this.Values = new int[largeur, hauteur];
        }

        public Cellule GetVoisinage(int x, int y, Direction direction)
        {
            int voisinageX = -1, voisinageY = -1;

            switch (direction)
            {
                case Direction.NO: 
                    voisinageX = x - 1; voisinageY = y -1; 
                    break;
                case Direction.N:
                    voisinageX = x ; voisinageY = y - 1;
                    break;
                case Direction.NE:
                    voisinageX = x+1; voisinageY = y - 1;
                    break;
                case Direction.O:
                    voisinageX = x - 1; voisinageY = y ;
                    break;
                case Direction.E:
                    voisinageX = x + 1; voisinageY = y;
                    break;
                case Direction.SO:
                    voisinageX = x - 1; voisinageY = y+1;
                    break;
                case Direction.S:
                    voisinageX = x; voisinageY = y + 1;
                    break;
                case Direction.SE:
                    voisinageX = x+1; voisinageY = y + 1;
                    break;

            }

            if (voisinageX >= 0 && voisinageX < this.Largeur)
            {
                if (voisinageY >= 0 && voisinageY < this.Hauteur)
                {
                    return new Cellule(voisinageX, voisinageY, this.Values[voisinageX, voisinageY]);
                }
            }
            return null;
        }

        public List<Cellule> GetVoisinageAller(int x, int y)
        {
            List<Cellule> voisinage = new List<Cellule>();

            Cellule cell = GetVoisinage(x, y, Direction.NO);

            if (cell != null)
                voisinage.Add(cell);

            cell = GetVoisinage(x, y, Direction.N);

            if (cell != null)
                voisinage.Add(cell);

            cell = GetVoisinage(x, y, Direction.NE);

            if (cell != null)
                voisinage.Add(cell);

            cell = GetVoisinage(x, y, Direction.O);

            if (cell != null)
                voisinage.Add(cell);


            
           // voisinage.Add(GetVoisinage(x, y, Direction.NO));
          //  voisinage.Add(GetVoisinage(x, y, Direction.N));
           // voisinage.Add(GetVoisinage(x, y, Direction.NE));
          //  voisinage.Add(GetVoisinage(x, y, Direction.O));

            return voisinage;
        }

        public List<Cellule> GetVoisinageRetour(int x, int y)
        {
            Cellule cell = null;
            List<Cellule> voisinage = new List<Cellule>();

            cell = GetVoisinage(x, y, Direction.SO); 
            if (cell != null)
                voisinage.Add(cell);

            cell = GetVoisinage(x, y, Direction.S);
            if (cell != null)
                voisinage.Add(cell);

            cell = GetVoisinage(x, y, Direction.SE);
            if (cell != null)
                voisinage.Add(cell);

            cell = GetVoisinage(x, y, Direction.E);
            if (cell != null)
                voisinage.Add(cell);
            
            //voisinage.Add(GetVoisinage(x, y, Direction.S));
            //voisinage.Add(GetVoisinage(x, y, Direction.SE));
            //voisinage.Add(GetVoisinage(x, y, Direction.E));

           

            return voisinage;
        }

    }
}
