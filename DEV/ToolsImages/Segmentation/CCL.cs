using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using System.Drawing.Imaging;
using ToolsImages.Segmentation;

namespace ToolsImages.Structures
{
    public class CCL
    {
        #region Variables
        public Matrix matrix;
        public Bitmap Img = null;

        public Matrix regionMatrix = null;
        public Matrix FinalMatrix = null;
        List<Structure> structures = null;
        //List<int> equivalentRegions = null;
       // List<Structure> structures = null;
        SetList equivalentRegions = null;

        ArrayList listRegion = null;

        Dictionary<string, Structure> rect = null;
        Dictionary<string, Structure> FinalRect = null;

        #endregion 

        #region Constructeur
        public CCL(Bitmap bmp)
        {
            matrix = new Matrix(bmp.Width, bmp.Height);
            regionMatrix = new Matrix(bmp.Width, bmp.Height);

            //Img = new Bitmap(bmp, bmp.Width, bmp.Height);
            this.Img = bmp;
        }
        #endregion

        #region Debut Label
        public Matrix DebutLabel()
        {
            BitmapData bmpData = Img.LockBits(new Rectangle(0, 0, Img.Width, Img.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);

            int scanline = bmpData.Stride;
            IntPtr scan0 = bmpData.Scan0;
            int largeur=0;

            unsafe
            {
                byte* p = (byte*)(void*)scan0;

                int val;
                int nOffset = scanline - Img.Width * 3;
                int nWidth = Img.Width * 3;

                for (int y = 0; y < Img.Height; ++y)
                {
                    //for (int x = 0; x < Img.Width; ++x)
                    //{
                    //    val = (int)p[0];
                    //    p[0] = (byte)val;

                    //    matrix.Values[x, y] = val;

                    //    matrix.Values[x, y] = val >= 1 ? 255 : 0;

                    //    ++p;
                    //}


                    for (int x = 0; x < nWidth; ++x)
                    {
                        val = (int)p[0];

                        if (val < 155)
                            p[0] = (byte)val;

                        if (x % 3 == 0)
                        {
                            if (val < 155)
                                val = 0;
                            else
                                val = 1;
                            matrix.Values[largeur, y] = val;
                            largeur++;
                        }
                        
                    
                            //matrix.Values[x, y] = val >= 1 ? 255 : 0;
                        ++p;
                    }
                    p += nOffset;
                    largeur = 0;
                }
            }

            Img.UnlockBits(bmpData);
           // return (Image)Img;
            Img.Dispose();
        
            return matrix;

            //for (int x = 0; x < Img.Width; x++)
            //{
            //    for (int y = 0; y < Img.Height; y++)
            //    {
            //        //matrix.Values[x, y] = pgm.Pixels[x, y] >= 1 ? 255 : 0;
            //    }
            //}

        }

        #endregion 

        #region Marque Region
        public Boolean MarqueRegionAller(out SetList equivalentRegions)
        {
            string str = "";
            int RegionCourante = 1;
            equivalentRegions = null;

            try
            {

                rect = new Dictionary<string, Structure>();
                FinalRect = new Dictionary<string, Structure>();

                equivalentRegions = new SetList();

                //equivalentRegions = new List<int>();
                structures = new List<Structure>();
                listRegion = new ArrayList();

                for (int y = 0; y < matrix.Hauteur; y++)
                {
                    for (int x = 0; x < matrix.Largeur; x++)
                    {
                        if (y == 128 && x == 313)
                            str = "";
                        // Si on est sur une forme
                        if (matrix.Values[x, y] == 0)
                        {
                            List<Cellule>  voisinage = null; 
                            voisinage = matrix.GetVoisinageAller(x, y);

                            //int matchNombre = 0;

                            //foreach (Cellule elem in voisinage)
                            //{
                            //    if (elem.Value > 0)
                            //        matchNombre++;
                            //}


                           int matchNombre = voisinage.Count(cellule => cellule.Value == 0);

                            // Si pas de pixels labelises
                            if (matchNombre == 0)
                            {
                                regionMatrix.Values[x, y] = RegionCourante;

                                equivalentRegions.Add(RegionCourante, new Set() { RegionCourante });
                                //equivalentRegions.Add(RegionCourante);
                                //structures.Add(new Structure(x, y, RegionCourante));
                                rect.Add(RegionCourante.ToString(), new Structure(x, y));

                                //equivalentRegions.Add(RegionCourante, new Set() { RegionCourante });
                                RegionCourante++;
                            }
                            else if (matchNombre == 1)
                            {
                                Cellule oneCell = voisinage.First(cellule => cellule.Value == 0);
                                regionMatrix.Values[x, y] = regionMatrix.Values[oneCell.X, oneCell.Y]; ;
                            }
                            else if (matchNombre > 1)
                            {
                                List<int> distinctRegions = voisinage.Select(cellule => regionMatrix.Values[cellule.X, cellule.Y]).Distinct().ToList();
                                while (distinctRegions.Remove(0)) ;

                                if (distinctRegions.Count == 1) //step5
                                {
                                    regionMatrix.Values[x, y] = distinctRegions[0];
                                }
                                else if (distinctRegions.Count > 1)
                                {
                                    // Equivalence tout de suite 

                                    int firstRegion = distinctRegions[0];
                                    regionMatrix.Values[x, y] = firstRegion;

                                    foreach (int region in distinctRegions)
                                    {
                                        if (!equivalentRegions[firstRegion].Contains(region))
                                        {
                                            equivalentRegions[firstRegion].Add(region);

                                        }

                                    }

                                    //foreach (Cellule voisi in voisinage)
                                    //{
                                    //    if (voisi.Value == 0)
                                    //        regionMatrix.Values[voisi.X, voisi.Y] = firstRegion;

                                    //}

                                }
                            }

                        }
                    }
                }

                return true;
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return false;
            }
            finally
            {

            }
        }
        #endregion 

        #region MergeRegions
        public Matrix MergeRegions(Matrix regionMatrix, SetList equivalentRegions, out Vector newRegionSets)
        {
            //merge equivalent regions using union-find algorithm
            UnionFind unionFind = new UnionFind(equivalentRegions);

            //normalize the region-numbers (assign sequential region-numbers starting from 1)
            List<int> regionNumbers = new List<int>();
            regionNumbers.AddRange(unionFind.Roots.Values);

            newRegionSets = new Vector();
            if (regionNumbers.Count > 0)
            {
                int index = 1, max = regionNumbers.Max();
                while (index <= max && regionNumbers.Count > 0)
                {
                    int min = regionNumbers.Min();
                    newRegionSets.Add(min, index);
                    index += 1;

                    while (regionNumbers.Remove(min)) ;
                }
            }

            //assign the new region-numbers to region-matrix
            for (int y = 0; y < regionMatrix.Hauteur; y++)
            {
                for (int x = 0; x < regionMatrix.Largeur; x++)
                {
                    if (regionMatrix.Values[x, y] > 0)
                    {
                        regionMatrix.Values[x, y] = newRegionSets[unionFind.Roots[regionMatrix.Values[x, y]]];

                        listRegion.Add(regionMatrix.Values[x, y]);
                    }
                }
            }

            //do logging
           // AddToLog("-----union-find-result:" + Environment.NewLine + unionFind.ToString());
          //  AddToLog("-----new-region-sets:" + Environment.NewLine + newRegionSets.ToString());

            return regionMatrix;
        }

        #endregion 

        #region Marque Region Retour
        public Matrix MarqueRegionRetour()
        {
            for (int y = regionMatrix.Hauteur - 1; y >= 0; y--)
            {
                for (int x = regionMatrix.Largeur - 1; x >= 0; x--)
                {
                    // Si on est sur une forme
                    if (regionMatrix.Values[x, y] > 0)
                    {
                        //if (y == 1)
                        //    Console.Write("1");

                        List<Cellule> voisinage = matrix.GetVoisinageRetour(x, y);



                        // Tout est déjà labellisé
                        List<int> distinctRegions = voisinage.Select(cellule => regionMatrix.Values[cellule.X, cellule.Y]).Distinct().ToList();
                        while (distinctRegions.Remove(0)) ;

                        if (distinctRegions.Count > 0)
                        {
                            int firstRegion = distinctRegions[0];
                            regionMatrix.Values[x, y] = firstRegion;

                            
                        }
                    }
                }
            }

            return regionMatrix;

        }
        #endregion

        #region Mise a jour rectangle
        public Boolean MiseAJourRegion()
        {
            //for (int y = 0; y < regionMatrix.Hauteur; y++)
            //{
            //    for (int x = 0; x < regionMatrix.Largeur; x++)
            //    {
            //        if (regionMatrix.Values[x, y] != 0)
            //            listRegion.Add(regionMatrix.Values[x, y]);
            //    }
            //}

            listRegion.Sort();

            for (int i = 0; i < listRegion.Count; i++)
            {
                for (int j = i + 1; j < listRegion.Count; j++)
                    if (listRegion[i].ToString() == listRegion[j].ToString())
                    {
                        listRegion.Remove(listRegion[j]);
                        j--;
                    }
                    else
                        break;
            }

            foreach (KeyValuePair<string, Structure> entry in rect)
            {
                for (int i = 0; i < listRegion.Count; i++)
                {
                    if (listRegion[i].ToString() == entry.Key)
                    {
                        FinalRect.Add(entry.Key, new Structure(entry.Value.X, entry.Value.Y));
                        break;
                    }
                }
            }

            return true;

        }
        #endregion 

        #region Position Rectangle 
        public Dictionary<string, Structure> PositionRectangle()
        {
            int memX = 0;
            int memY = 0;
            String memEntry = "";
            ArrayList lstX = new ArrayList();
            ArrayList lstY = new ArrayList();

            Dictionary<string, Structure> FinalPosition = new Dictionary<string, Structure>();

            foreach (KeyValuePair<string, Structure> entry in FinalRect)
            {
                lstX.Clear();
                lstY.Clear();

                Structure structure = entry.Value;
               
                memEntry = entry.Key;
                for (int x = 0; x < regionMatrix.Largeur; x++)
                {
                    for (int y = 0; y < regionMatrix.Hauteur; y++)
                    {
                        if (regionMatrix.Values[x, y] == Convert.ToInt16(memEntry))
                        {
                            lstX.Add(x);
                            lstY.Add(y);
                        }
                    }
                }

                lstX.Sort();
                lstY.Sort();

                FinalPosition.Add(memEntry, 
                    new Structure(Convert.ToInt16(lstX[0]), 
                        Convert.ToInt16(lstY[0]),
                        Convert.ToInt16(lstX[lstX.Count - 1]) - Convert.ToInt16(lstX[0]),
                        Convert.ToInt16(lstY[lstY.Count - 1]) - Convert.ToInt16(lstY[0])));

            }
            return FinalPosition;
        }
        #endregion 

        #region Position Region
        public Dictionary<string, Structure> DefinePositionRegion()
        {
            int gauche = 0;
            int droit = 0;
            int pos = 0;
            int hauteur = 0;
            int largeur = 0;
            int MaxLargeur = 0;
            int memX = 0;
            int memY = 0;
            String memEntry = "";
            int XGauche = 0;
            int XDroit = 0;
            int MaxY = 0;

            Dictionary<string, Structure> Finalrect = new Dictionary<string, Structure>();

            foreach (KeyValuePair<string, Structure> entry in FinalRect)
            {

                if (MaxLargeur > 0)
                    Finalrect.Add(memEntry, new Structure(memX, memY, MaxLargeur, MaxY - memY + 1));

                Structure structure = entry.Value;
                memX = structure.X;
                memY = structure.Y;
                memEntry = entry.Key;
                hauteur = 0;
                largeur = 0;
                MaxLargeur = 0;
                MaxY = 0;

                for (int y = structure.Y; y < regionMatrix.Hauteur; y++)
                {
                    if (largeur > MaxLargeur)
                        MaxLargeur = largeur;

                    gauche = 0;
                    XGauche = 0;

                    if (regionMatrix.Values[structure.X, y] == Convert.ToInt16(entry.Key))
                        MaxY = y;


                    pos = structure.X + gauche - 1;
                    while (pos >= 0 && (regionMatrix.Values[pos, y] == Convert.ToInt16(entry.Key) || regionMatrix.Values[pos, y] == 0))
                    {
                        if (regionMatrix.Values[pos, y] == Convert.ToInt16(entry.Key))
                        {
                            XGauche = pos;
                            // if (y > MaxY)
                            MaxY = y;
                        }
                        gauche--;
                        pos = structure.X + gauche;
                    }

                    droit = 0;
                    XDroit = 0;

                    pos = structure.X + droit + 1;
                    while (pos <= regionMatrix.Largeur - 1 && (regionMatrix.Values[pos, y] == Convert.ToInt16(entry.Key) || regionMatrix.Values[pos, y] == 0))
                    {
                        if (regionMatrix.Values[pos, y] == Convert.ToInt16(entry.Key))
                        {
                            XDroit = pos;
                            // if (y > MaxY)
                            MaxY = y;
                        }
                        droit++;
                        pos = structure.X + droit;
                    }


                    if (XDroit > 0 && XGauche == 0)
                    {
                        largeur = XDroit - structure.X + 1;
                    }
                    else if (XDroit == 0 && XGauche > 0)
                    {
                        largeur = structure.X - XGauche + 1;
                    }
                    else if (XDroit > 0 && XGauche > 0)
                    {
                        largeur = XDroit - XGauche + 1;
                    }
                    else
                    {
                        largeur = 1;
                    }

                    hauteur++;

                }


            }

            if (MaxLargeur > 0)
                Finalrect.Add(memEntry, new Structure(memX, memY, MaxLargeur, MaxY - memY + 1));

            // En fonction du nombre de regions 
            // TDG 
            int nombreRegion = Finalrect.Count;



            return Finalrect;
        }

        #endregion 
    }
}
