using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC.Donnees
{
    public class MnistImage
    {
        public int largeur; // 28
        public int hauteur; // 28
        public byte[][] pixels1; // 0(white) - 255(black)
        public byte label; // '0' - '9'
        public byte[] pixels;
       

        //public MnistImage1(int largeur, int hauteur, byte[][] pixels, byte label)
        //{
        //    this.largeur = largeur;
        //    this.hauteur = hauteur;

        //    this.pixels1 = new Byte[hauteur][];

        //    for (int i = 0; i < this.pixels.Length; ++i)
        //        this.pixels[i] = new byte[largeur];

        //    for (int i = 0; i < hauteur; ++i)
        //        for (int j = 0; j < largeur; ++j)
        //            this.pixels1[i][j] = Convert.ToByte(255 - Convert.ToInt32(pixels[i][j]));

        //    this.label = label;


        //}

        public MnistImage(int largeur, int hauteur, byte[][] pixels, byte label)
        {
            this.largeur = largeur;
            this.hauteur = hauteur;

            this.pixels = new byte[largeur * hauteur];

            for (int i = 0; i < hauteur; i++)
                for (int j = 0; j < largeur; j++)
                    this.pixels[(i * hauteur) + j] = Convert.ToByte(255 - pixels[i][j]);
            this.label = label;

            // Convert.ToByte(255 - Convert.ToInt32(pArray[ii]));
        }


    }
}
