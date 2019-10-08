using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Collections.Specialized;

using ToolsImages.Conversion;
using ToolsImages.Structures;
using ToolsImages.Filtres;

namespace ToolsImages
{
    public class ImageResize
    {
        private string PathFileSource;
        private string PathDestination;
        private int Pixels;

        private String Fichier;

        private ArrayList lstImage = new ArrayList();
        private ArrayList lstLabel = new ArrayList();

        private Graphics DisplayGraphics;

        Dictionary<string, string> Correspondance = new Dictionary<string, string>();
            
        private StreamWriter Chiffres_Printed;
        private StreamWriter Majuscules_Printed;
        private StreamWriter Minuscules_Printed;
        
        private StreamReader transco;
        private FileStream ifs;

        private ToolsImages.Structures.Matrix regionMatrix = null;
      
        public enum Dimensions
        {
            Width,
            Height
        }
        public enum AnchorPosition
        {
            Top,
            Center,
            Bottom,
            Left,
            Right
        }

        public Boolean Close()
        {
            try
            {
                if (Majuscules_Printed != null)
                    Majuscules_Printed.Close();

                if (Minuscules_Printed != null)
                    Minuscules_Printed.Close();

                if(Chiffres_Printed != null)  
                    Chiffres_Printed.Close();
                                
            
                return true;
            }
            catch (Exception err)
            {
                return false;
            }
        }


        public ImageResize(string PathFileSource, string PathDestination, int pixels,String fichier)
        {
            this.PathFileSource = PathFileSource;
            this.PathDestination = PathDestination;
            this.Pixels = pixels;
            this.Fichier = fichier;

          

        }

        public void ProcessDirectory(string targetDirectory)
        {
            // Process the list of files found in the directory.
            string[] fileEntries = Directory.GetFiles(targetDirectory);
            foreach (string fileName in fileEntries)
                ProcessFile(fileName);

            // Recurse into subdirectories of this directory.
            string[] subdirectoryEntries = Directory.GetDirectories(targetDirectory);
            foreach (string subdirectory in subdirectoryEntries)
                ProcessDirectory(subdirectory);
        }

        public void ProcessFile(string path)
        {
            String str = "";
            String line = "";
           
            Console.WriteLine("Processed file '{0}'.", path);

            String label = Path.GetDirectoryName(path);
            String result = System.IO.Path.GetFileName(label);
            // Table de correspondance

            if (Path.GetFileNameWithoutExtension(path).ToUpper() == "TRANSCO")
            {
                try
                {
                    ifs = new FileStream(path, FileMode.Open);
                    transco = new StreamReader(ifs);

                    while ((line = transco.ReadLine()) != null)
                    {
                        String[] split = line.Split('=');
                        Correspondance.Add(split[0], split[1]);
                    }
                    transco.Close(); ifs.Close();
                }
                catch (Exception err)
                {
                    Console.WriteLine(err.Message);
                }
 
            }
            else
            {

                switch (Path.GetExtension(path).ToUpper())
                {
                    case ".JPEG":
                        break;
                    case ".PNG":

                        Bitmap BitmapNormalisation = null;
                        Bitmap    BitmapResize= null;
                        Bitmap    BitmapBorder= null;
                        Bitmap    BitmapFinal= null;
                        Image    imgPhotoVert= null;
                        Bitmap    bitMapMedian= null;
                        Bitmap    bitMapOtsu= null;
                        Bitmap   bitMapCCL= null;
                        Bitmap bitMapSave = null;

                        try
                        {
                            BitmapNormalisation = null;
                            imgPhotoVert = Image.FromFile(path);
                            //Image imgPhoto = null;
                            Dictionary<string, Structure> dic = null;

                            string repertoire = Path.GetDirectoryName(path);
                            string pattern = Path.GetFileName(repertoire);
                            char caractere = Convert.ToChar(Correspondance[pattern]);

                            string fichierResize = Path.GetFileNameWithoutExtension(path) + "_R.png";

                            // Filtre médian 
                            bitMapMedian = (Bitmap)imgPhotoVert;
                            //  bmp = ToolsImages.Filtres.Median.MedianFilter(bmp, 3);

                            //Otsu 
                            Otsu otsu = new Otsu();
                            bitMapOtsu = (Bitmap)bitMapMedian.Clone();

                            otsu.Convert2GrayScaleFast(bitMapOtsu);
                            int otsuThreshold = otsu.getOtsuThreshold((Bitmap)bitMapOtsu);
                            otsu.threshold(bitMapOtsu, otsuThreshold);

                            bitMapCCL = (Bitmap)bitMapOtsu.Clone();
                            bitMapSave = (Bitmap)bitMapCCL.Clone();

                            // Detection objet CCL
                            dic = SetCCL(bitMapCCL);

                            if (dic.Count == 1)
                            {
                                // Normalement un seul element 
                                foreach (var pair in dic)
                                {
                                    Structure structure = (Structure)pair.Value;

                                    int largeur = structure.Largeur;
                                    int hauteur = structure.Hauteur;

                                    if (largeur > 0 && hauteur > 0)
                                    {
                                        Rectangle source_rect = new Rectangle(structure.X, structure.Y, largeur, hauteur);
                                        Rectangle dest_rect = new Rectangle(0, 0, largeur, hauteur);
                                        BitmapNormalisation = new Bitmap(largeur, hauteur);

                                        DisplayGraphics = Graphics.FromImage(BitmapNormalisation);
                                        DisplayGraphics.DrawImage(bitMapSave, dest_rect, source_rect, GraphicsUnit.Pixel);


                                    }
                                }

                                if (BitmapNormalisation != null)
                                {
                                    // Prendre la plus grande largeur
                                    int max = MaximumSize(BitmapNormalisation.Height, BitmapNormalisation.Width);
                                    BitmapResize = ToolsImages.Conversion.Conversion.ResizeBitmap(BitmapNormalisation, max, max);

                                    ///Bordure 
                                    BitmapBorder = BitmapWithBorder(BitmapResize, BitmapResize.Height + 4);


                                    BitmapFinal = Conversion.Conversion.ResizeBitmap((Bitmap)BitmapBorder, 32, 32);
                                    //Bitmap bmp = Conversion.Conversion.ResizeBitmap((Bitmap)imgPhotoVert, 29, 29);
                                    byte[] test = Conversion.Conversion.ConvertGrayscaleBitmaptoBytes(BitmapFinal);

                                    //int reso = 29 * 29;
                                    int reso = 32 * 32;

                                    for (int i = 0; i < reso; ++i)
                                    {
                                        str += test[i] + " ";
                                    }

                                    int a = (int)caractere;

                                    // Chiffres 48 à 57 
                                    // Majuscules de 65 à 90 
                                    // Minuscules de 97 à 122

                                    if (a >= 97 && a <= 122 || a >= 48 && a <= 57)
                                    {
                                        if (Minuscules_Printed == null)
                                            Minuscules_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Minuscules_Chiffres_1106_Printed.txt"));

                                        Minuscules_Printed.WriteLine(caractere + " " + str);
                                        Minuscules_Printed.Flush();

                                       
                                    }
                                    else
                                    {
                                        if (Majuscules_Printed == null)
                                            Majuscules_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Majuscules_1106_Printed.txt"));

                                        Majuscules_Printed.WriteLine(caractere + " " + str);
                                        Majuscules_Printed.Flush();

                                    }

                                    /*

                                    if (a >= 65 && a <= 90 || a >= 48 && a <= 57)
                                    {
                                        if (Majuscules_Printed == null)
                                            Majuscules_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Majuscules_Printed.txt"));

                                        Majuscules_Printed.WriteLine(caractere + " " + str);
                                        Majuscules_Printed.Flush();
                                    }
                                    else
                                    {
                                        if (Minuscules_Printed == null)
                                            Minuscules_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Minuscules_Printed.txt"));

                                        Minuscules_Printed.WriteLine(caractere + " " + str);
                                        Minuscules_Printed.Flush();

                                    }*/


                                    //if (a >= 48 && a <= 57)
                                    //{
                                    //    if (Chiffres_Printed == null)
                                    //        Chiffres_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Chiffres_Printed.txt"));


                                    //    Chiffres_Printed.WriteLine(caractere + " " + str);
                                    //    Chiffres_Printed.Flush();

                                    //}
                                    //else if (a >= 65 && a <= 90)
                                    //{
                                    //    if (Majuscules_Printed == null)
                                    //        Majuscules_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Majuscules_Printed.txt"));

                                    //    Majuscules_Printed.WriteLine(caractere + " " + str);
                                    //    Majuscules_Printed.Flush();

                                    //}
                                    //else if (a >= 97 && a <= 122)
                                    //{
                                    //    if (Minuscules_Printed == null)
                                    //        Minuscules_Printed = new StreamWriter(Path.Combine(this.PathDestination, "Minuscules_Printed.txt"));

                                    //    Minuscules_Printed.WriteLine(caractere + " " + str);
                                    //    Minuscules_Printed.Flush();
                                    //}

                                  
                                }

                            }

                            if (BitmapNormalisation != null)
                            {
                                BitmapNormalisation.Dispose();
                                BitmapNormalisation = null;
                            }
                            if (BitmapResize != null)
                            {
                                BitmapResize.Dispose();
                                BitmapResize = null;
                            }
                            if (BitmapBorder != null)
                            {
                                BitmapBorder.Dispose();
                                BitmapBorder = null;
                            }
                            if (BitmapFinal != null)
                            {
                                BitmapFinal.Dispose();
                                BitmapFinal = null;
                            }
                            if (DisplayGraphics != null)
                            {
                                DisplayGraphics.Dispose();
                                DisplayGraphics = null;

                            }

                            if (imgPhotoVert != null)
                            {
                                imgPhotoVert.Dispose();
                                imgPhotoVert = null;
                            }
                            if (bitMapMedian != null)
                            {
                                bitMapMedian.Dispose();
                                bitMapMedian = null;
                            }
                            if (bitMapOtsu != null)
                            {
                                bitMapOtsu.Dispose();
                                bitMapOtsu = null;
                            }
                            if (bitMapCCL != null)
                            {
                                bitMapCCL.Dispose();
                                bitMapCCL = null;
                            }
                            if (bitMapSave != null)
                            {
                                bitMapSave.Dispose();
                                bitMapSave = null;
                            }




                        }
                        catch (Exception err)
                        {
                            Console.WriteLine(err.Message + " " + err.Source);
                        }
                        finally
                        {
                           

                        }


                        // Ecrire dans fichier                





                        // imgPhoto.Save(Path.Combine(this.PathDestination, fichierResize), ImageFormat.Png);
                        // imgPhoto.Dispose();

                        break;
                    case ".TIFF":
                        break;
                    default:
                        break;

                }
            }


        }

        public byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }

        public void convert(string fileImage)
        {
            byte[] b = ImageToBinary(fileImage);
            File.WriteAllBytes(@"D:\MNIST\travail\test1.gil", b); // Requires System.IO

           // SaveData(@"D:\MNIST\travail\test.gil",ImageToBinary(fileImage));
         
            
        }
        protected bool SaveData(string FileName, byte[] Data)
        {
            BinaryWriter Writer = null;
            string Name = FileName;

            try
            {
                // Create a new stream to write to the file
                Writer = new BinaryWriter(File.OpenWrite(Name));

                // Writer raw data                
                Writer.Write(Data);
                Writer.Flush();
                Writer.Close();
            }
            catch
            {
                //...
                return false;
            }

            return true;
        }

        public byte[] ImageToBinary(string imagePath)
        {
            FileStream fS = new FileStream(imagePath, FileMode.Open, FileAccess.Read);
            byte[] b = new byte[fS.Length];
            fS.Read(b, 0, (int)fS.Length);
            fS.Close();
            return b;
        }

        public Boolean process()
        {
            try
            {
                ProcessDirectory(this.PathFileSource);
    
                return true;


            }
            catch (Exception err)
            {
                return false;
            }

            



        }

        static Image FixedSize(Image imgPhoto, int Width, int Height)
        {
            int sourceWidth = imgPhoto.Width;
            int sourceHeight = imgPhoto.Height;
            int sourceX = 0;
            int sourceY = 0;
            int destX = 0;
            int destY = 0;

            float nPercent = 0;
            float nPercentW = 0;
            float nPercentH = 0;

            nPercentW = ((float)Width / (float)sourceWidth);
            nPercentH = ((float)Height / (float)sourceHeight);

            //if we have to pad the height pad both the top and the bottom
            //with the difference between the scaled height and the desired height
            if (nPercentH < nPercentW)
            {
                nPercent = nPercentH;
                destX = (int)((Width - (sourceWidth * nPercent)) / 2);
            }
            else
            {
                nPercent = nPercentW;
                destY = (int)((Height - (sourceHeight * nPercent)) / 2);
            }

            int destWidth = (int)(sourceWidth * nPercent);
            int destHeight = (int)(sourceHeight * nPercent);

            Bitmap bmPhoto = new Bitmap(Width, Height, PixelFormat.Format24bppRgb);
            bmPhoto.SetResolution(imgPhoto.HorizontalResolution, imgPhoto.VerticalResolution);

            Graphics grPhoto = Graphics.FromImage(bmPhoto);
            grPhoto.Clear(Color.Red);
            grPhoto.InterpolationMode = InterpolationMode.HighQualityBicubic;

            grPhoto.DrawImage(imgPhoto,
                new Rectangle(destX, destY, destWidth, destHeight),
                new Rectangle(sourceX, sourceY, sourceWidth, sourceHeight),
                GraphicsUnit.Pixel);

            grPhoto.Dispose();
            return bmPhoto;
        }

        public Dictionary<string, Structure> SetCCL(Bitmap img)
        {
            Bitmap temp = null;
            CCL ccl = null;
            Bitmap bmp = (Bitmap)img.Clone();

            try
            {
                ToolsImages.Structures.Matrix regionMatrix = null;
                SetList regionSets = null;
                Vector finalRegionSets = null;
                

                regionSets = new SetList();

               // temp = (Bitmap)img;
              //  Bitmap bmap = (Bitmap)temp.Clone();

                ccl = new CCL(bmp);

                ccl.DebutLabel();

                ccl.MarqueRegionAller(out regionSets);
                regionMatrix = ccl.MergeRegions(ccl.regionMatrix, regionSets, out finalRegionSets);

                ccl.MiseAJourRegion();

                 Dictionary<string, Structure> rr = ccl.PositionRectangle();
              //  bmap.Dispose();

                 return rr;

            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                return null;
            }
            finally
            {
                ccl.Img.Dispose();
                bmp.Dispose();
                ccl = null;
              //  temp.Dispose();
            }

          


        }

        private int MaximumSize(int A, int B)
        {
            if (A > B)
                return A;
            else
                return B;
        }

        private Bitmap BitmapWithBorder(Bitmap originalImage, int Taille)
        {

            Bitmap newImage = new Bitmap(Taille, Taille);
            using (Graphics graphics = Graphics.FromImage(newImage))
            {
                graphics.Clear(Color.White);
                int x = (newImage.Width - originalImage.Width) / 2;
                int y = (newImage.Height - originalImage.Height) / 2;
                graphics.DrawImage(originalImage, x, y);
            }

            return newImage;
        }
    }
}
