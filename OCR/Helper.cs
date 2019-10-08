using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCR
{
    public class Helpers
    {
        static Random rnd = new Random(0);

        public static void ShuffleRows(double[][] matrix)
        {
            for (int i = 0; i < matrix.Length; ++i)
            {
                int r = rnd.Next(i, matrix.Length);
                double[] tmp = matrix[r];
                matrix[r] = matrix[i];
                matrix[i] = tmp;
            }
        }

        public static double[][] MakeMatrix(int rows, int cols)
        {
            double[][] result = new double[rows][];
            for (int i = 0; i < rows; ++i)
                result[i] = new double[cols];
            return result;
        }


    }
}
