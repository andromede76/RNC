using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ToolsImages.Structures
{
    public class Set : List<int>
    {
        public override string ToString()
        {
            return "{" + string.Join(",", this) + "}";
        }
    }

    public class SetList : Dictionary<int, Set>
    {
        public override string ToString()
        {
            return string.Join(Environment.NewLine, this);
        }
    }

    public class Vector : Dictionary<int, int>
    {
        public override string ToString()
        {
            return string.Join(Environment.NewLine, this);
        }
    }
}
