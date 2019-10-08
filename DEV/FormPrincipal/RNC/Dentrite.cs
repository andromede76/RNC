using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RNC
{
    public class Dentrite
    {
        public uint NeuroneIndex;
        public uint PoidstIndex;

        public uint DentriteIndex;

        public Dentrite()
        {
        }

        public Dentrite(uint neuroneIndex, uint poidstIndex, uint dentriteIndex)
        {
            this.NeuroneIndex = neuroneIndex;
            this.PoidstIndex = poidstIndex;
            this.DentriteIndex = dentriteIndex;

        }
    }
}
