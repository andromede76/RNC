using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OCR.Tools
{
    public static class Outils
    {
        public static int MaximumSize(int A, int B)
        {
            if (A > B)
                return A;
            else
                return B;
        }
    }
}
