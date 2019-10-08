using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RNC.Donnees
{
    public class MnistDatabase
    {
        public string ImagesApprentissage;
        public string LabelsApprentissage;
        public string ImagesTest;
        public string LabelTest;

        public int numRows;
        public int numCols;
        
        public MnistImage[] result;

        public int imageCount;

        public uint indexNextPattern;

        public int[] tabRandomizedPatternSequence;

       
        //public MnistDatabase(string ImagesApprentissage, string LabelsApprentissage, string ImagesTest, string LabelTest)
        //{
        //    this.ImagesApprentissage = ImagesApprentissage;
        //    this.LabelsApprentissage = LabelsApprentissage;
        //    this.ImagesTest = ImagesTest;
        //    this.LabelTest = LabelTest;

        //}

        public MnistImage[] LoadData(string pixelFile, string labelFile)
        {
            
            FileStream ifsPixels = new FileStream(pixelFile, FileMode.Open);
            FileStream ifsLabels = new FileStream(labelFile, FileMode.Open);

            BinaryReader brImages = new BinaryReader(ifsPixels);
            BinaryReader brLabels = new BinaryReader(ifsLabels);

            int magic1 = brImages.ReadInt32(); // stored as Big Endian
           // magic1 = ReverseBytes(magic1); // convert to Intel format


            imageCount = brImages.ReadInt32();
            imageCount = ReverseBytes(imageCount);

            tabRandomizedPatternSequence = new int[imageCount];

            numRows = brImages.ReadInt32();
            numRows = ReverseBytes(numRows);
            numCols = brImages.ReadInt32();
            numCols = ReverseBytes(numCols);

            int magic2 = brLabels.ReadInt32();
            //magic2 = ReverseBytes(magic2);

            int numLabels = brLabels.ReadInt32();
            numLabels = ReverseBytes(numLabels);

            // récupérer l'entete 
            result = new MnistImage[imageCount];

            byte[][] pixels = new byte[numRows][];
            for (int i = 0; i < pixels.Length; ++i)
                pixels[i] = new byte[numCols];

            // each image
            for (int di = 0; di < imageCount; ++di)
            {
                for (int i = 0; i < numRows; ++i) // get 28x28 pixel values
                {
                    for (int j = 0; j < numCols; ++j)
                    {
                        byte b = brImages.ReadByte();
                        pixels[i][j] = b;
                                               
                        
                    }
                }

                byte lbl = brLabels.ReadByte(); // get the label
                MnistImage dImage = new MnistImage(numRows, numCols, pixels, lbl);
                result[di] = dImage;
            } // each image

            ifsPixels.Close(); brImages.Close();
            ifsLabels.Close(); brLabels.Close();

            return result;


        }


        public uint GetNextPattern()
        {
            uint indexRet = indexNextPattern;
            indexNextPattern++;

            if (indexNextPattern >= imageCount)
                indexNextPattern++;

            return indexRet;

        }
        public static int ReverseBytes(int v)
        {
            byte[] intAsBytes = BitConverter.GetBytes(v);
            Array.Reverse(intAsBytes);
            return BitConverter.ToInt32(intAsBytes, 0);
        }

        public void RandomizePatternSequence()
        {
            int indexPattern;
            int temp;
            indexNextPattern = 0;

            // Initialisation 
            for (int i = 0; i < imageCount; i++)
               tabRandomizedPatternSequence[i] = i;

            Random rdm = new Random();

            for (int i = 0; i < imageCount; i++)
            {
                indexPattern = (int)(rdm.NextDouble() * imageCount);

                temp = tabRandomizedPatternSequence[i];
                tabRandomizedPatternSequence[i] = tabRandomizedPatternSequence[indexPattern];
                tabRandomizedPatternSequence[indexPattern] = temp;
            }
        }

    }
}
