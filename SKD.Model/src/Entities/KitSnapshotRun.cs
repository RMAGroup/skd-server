using System;
using System.Collections.Generic;

namespace SKD.Model {
    public class kitSnapshotRun : EntityBase {
        public Guid PlantId { get; set; }
        public Plant Plant { get; set; }
        public DateTime RunDate { get; set; }
        public int Sequence { get; set; }
        public ICollection<KitSnapshot> KitSnapshots { get; set; } = new List<KitSnapshot>();

    }
}