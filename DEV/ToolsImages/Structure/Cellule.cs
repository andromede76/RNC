using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsImages.Structures
{
    public class Cellule
    {
        public int X;
        public int Y;
        public int Value;

        public Cellule()
        {
            this.X = 0;
            this.Y = 0;
            this.Value = 0;
        }

        public Cellule(int x, int y)
            : this()
        {
            this.X = x;
            this.Y = y;
        }

        public Cellule(int x, int y, int value)
        {
            this.X = x;
            this.Y = y;
            this.Value = value;
        }

    }
}
