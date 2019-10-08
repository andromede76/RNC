using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using ToolsImages.Structures;

namespace ToolsImages.Segmentation
{
    public class UnionFind
    {
        #region Properties

        public SetList Sets { get; set; }
        public Vector Roots { get; set; }

        #endregion

        #region Constructors

        public UnionFind(SetList sets)
        {
            this.Sets = sets;
            this.Roots = new Vector();

            this.Initialize();
            this.Start();
        }

        #endregion

        #region Methods

        public void Initialize()
        {
            //initialize Roots (add all items to Roots)
            Roots.Clear();
            foreach (int index in Sets.Keys)
            {
                foreach (int item in Sets[index])
                {
                    if (!Roots.ContainsKey(item))
                    {
                        Roots.Add(item, -1);
                    }
                }
            }

            //assign root of each item as first-element of item's set
            foreach (int index in Sets.Keys)
            {
                foreach (int item in Sets[index])
                {
                    Roots[item] = Sets[index][0];
                }
            }
        }

        public bool Find(int item1, int item2)
        {
            return Roots[item1] == Roots[item2];
        }

        //to merge two sets containing item1 & item2,
        //set root of all items of item1's set to item2's root.
        public void Unite(int item1, int item2)
        {
            int item1Root = Roots[item1];
            for (int index = 0; index < Roots.Count; index++)
            {
                int item = Roots.Keys.ElementAt(index);
                if (Roots[item] == item1Root)
                {
                    Roots[item] = Roots[item2];
                }
            }
        }

        public void Start()
        {
            foreach (int index in Sets.Keys)
            {
                var set = Sets[index];
                for (int i = 0; i < set.Count; i++)
                {
                    for (int j = i + 1; j < set.Count; j++)
                    {
                        Unite(set[i], set[j]);
                    }
                }
            }
        }

        public override string ToString()
        {
            string result = string.Empty;
            foreach (int item in Roots.Keys)
            {
                result += item + ":" + Roots[item] + Environment.NewLine;
            }
            return result;
        }

        #endregion
    }
}
