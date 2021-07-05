#nullable enable
using System;
using System.Collections.Generic;

namespace SKD.Service {
    public class VehicleModelInput {
        public Guid? Id { get; init; }
        public string Code { get; init; } = "";
        public string Name { get; init; } = "";
        public string ModelYear { get; set; } = "";
        public string Model { get; set; } = "";
        public string Series { get; set; } = "";
        public string Body { get; set; } = "";
        public ICollection<ComponentStationInput> ComponentStationInputs { get; set; } = new List<ComponentStationInput>();
    }

    public class ComponentStationInput {
        public string ComponentCode { get; init; } = "";
        public string ProductionStationCode { get; init; } = "";
    }

    public class VehicleModelFromExistingInput {
        public string Code { get; set; } = "";
        public string ModelYear { get; set; } = "";
        public string ExistingModelCode { get; set; } = "";
    }
}