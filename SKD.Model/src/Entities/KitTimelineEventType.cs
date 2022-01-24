#nullable enable

namespace SKD.Model {

    public enum TimeLineEventCode {
        CUSTOM_RECEIVED = 0,       // FPCR
        PLAN_BUILD,                // FPBP
        VIN_CHECK,                 // FPBS  (Notify Ford to check if VIN needs to change)
        BUILD_COMPLETED,           // FPBC
        GATE_RELEASED,             // FPGR
        WHOLE_SALE                 // FPWS
    }
    
    public partial class KitTimelineEventType : EntityBase {
        public TimeLineEventCode Code { get; set; }
        public string Description { get; set; } = "";
        public int Sequence { get; set; }

        public ICollection<KitSnapshot> Snapshots { get; set; } = new List<KitSnapshot>();
    }
}