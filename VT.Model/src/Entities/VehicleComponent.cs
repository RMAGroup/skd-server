using System;

namespace VT.Model {
    public class VehicleComponent : EntityBase {

        public Guid VehicleId { get; set; }
        public Vehicle Vehicle { get; set; }


        public Guid ComponentId { get; set; }
        public Component Component { get; set; }


        public int Sequence { get; set; }
        public string SerialNumber { get; set; }

        public DateTime? CompletedAt { get; set; }

        public VehicleComponent() {

        }
    }
}