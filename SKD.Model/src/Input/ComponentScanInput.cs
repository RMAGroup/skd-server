using System;

namespace SKD.Model {
    public class ComponentScanInput {
        public Guid VehicleComponentId { get; set; }
        public string Scan1 { get; set; }
        public string Scan2 { get; set; }
        public Boolean Replace { get; set; } 
    }
}