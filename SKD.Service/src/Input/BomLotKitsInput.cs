using System;
using System.Collections.Generic;

namespace SKD.Common {

    public class BomLotKitInput {
        public string PlantCode { get; set; }
        public int Sequence { get; set; }
        public List<Lot> Lots { get; set; } 
        public class Lot {
            public string LotNo { get; init; }
            public List<LotKit> Kits { get; init; } 

            public class LotKit {
                public string KitNo { get; init; }
                public string ModelCode { get; init; }
            }
        }
    }
}