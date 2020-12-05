using System;

namespace SKD.Model {
    public class PlantOverviewDTO {
        public Guid Id { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public DateTime CreatedAt {get; set; }
    }
}