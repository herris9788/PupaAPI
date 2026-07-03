namespace Pupa.Configs
{
    /// <summary>
    /// Static wire-rope-requisition rules, mirroring the
    /// "Wire Rope Requisition Process Flow" diagram (T31.001 - WIRE ROPE's flow.jpeg):
    /// Placement, Length (Roll + Meter) and End Type are mandatory Define Item
    /// inputs for every T31.001-prefixed line item.
    /// </summary>
    public static class WireRopePolicy
    {
        public const string WireRopeItemCodePrefix = "T31.001";

        /// <summary>
        /// Master list of Placement buttons. Source: mapping.xlsx sheet
        /// "beside bc, tanker, towing" — the generic Application list, used for
        /// all vessels regardless of fleet for now. Per-fleet/per-vessel
        /// Placement overrides (Bulk Carrier, Tanker, Towing specific lists)
        /// are intentionally out of scope for this pass.
        /// </summary>
        private static readonly HashSet<string> _placementOptions = new(StringComparer.OrdinalIgnoreCase)
        {
            "ACCOMODATION LADDER - HOISTING (PORT SIDE / STARBOARD SIDE)",
            "ACCOMODATION LADDER - STOWING",
            "AFT SIDE BOAT FALL",
            "ANCHOR LASHING",
            "AUTO TRIGGER LINE",
            "BRIDLE",
            "CARGO CRANE",
            "CARGO HOSE HANDLING CRANE",
            "COMBINATION LADDER",
            "DECK REMOTE CONTROL",
            "FIRE WIRE",
            "FIRE WIRE FOR SAFETY FWD AND AFT",
            "FREEFALL LIFEBOAT",
            "GRAB",
            "GRABLINE LIFEBOAT & RESCUE BOAT",
            "HOISTING",
            "HOSE HANDLING CRANE",
            "INBOARD REMOTE CONTROL",
            "JANGKAR TONGKANG",
            "LABRANG",
            "LABRANG FOR MAST AFT",
            "LABRANG FOR MAST CARGO COMPRESSOR",
            "LABRANG FOR MAST FORWARD",
            "LABRANG FOR MAST MIDLE/MANIFOLD",
            "LIFEBOAT",
            "LIFEBOAT DAVIT",
            "LIFEBOAT DAVIT - BRAKE SIDE",
            "LIFEBOAT DAVIT - MOTOR SIDE",
            "LIFEBOAT DAVIT REMOTE",
            "LIFEBOAT LASHING TRIGGER (PORT SIDE / STARBOARD SIDE)",
            "LIFEBOAT RELEASE CONTROL",
            "LUFFING",
            "MOORING",
            "OVER LASHING LINE",
            "OVERHEAD CRANE",
            "OVERHEAD CRANE - ENGINE ROOM",
            "PILOT LADDER",
            "PROVISION CRANE",
            "PROVISION CRANE - HOISTING",
            "PROVISION CRANE - LUFFING",
            "RESCUE BOAT",
            "SAFETY WIRE",
            "SIDE BOAT FALL",
        };

        /// <summary>All 16 end-termination configurations from the flow diagram.</summary>
        private static readonly HashSet<string> _validEndTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "PLAIN",
            "TAPERED_WELDED_END",
            "SEIZED_END",
            "MECHANICAL_SINGLE_EYE_SPLICE",
            "HAND_SINGLE_END_EYE",
            "MECHANICAL_BOTH_EYE_SPLICE",
            "HAND_BOTH_EYE_SPLICE",
            "ONE_END_OPEN_EYE_THIMBLE_OTHER_END_PLAIN",
            "ONE_END_OPEN_EYE_THIMBLE_OTHER_END_MECHANICAL_EYE_SPLICE",
            "BOTH_END_OPEN_EYE_THIMBLE",
            "ONE_END_SOLID_EYE_THIMBLE_OTHER_END_PLAIN",
            "ONE_END_DEAD_EYE_THIMBLE_OTHER_END_PLAIN",
            "ONE_END_OPEN_SPELTER_SOCKET_OTHER_END_PLAIN",
            "ONE_END_CLOSE_SPELTER_SOCKET_OTHER_END_PLAIN",
            "ONE_END_OPEN_SWAGED_SOCKET_OTHER_END_PLAIN",
            "ONE_END_CLOSE_SWAGED_SOCKET_OTHER_END_PLAIN",
        };

        /// <summary>End types that accept one optional eye length (WireRopeEyeLengthM).</summary>
        private static readonly HashSet<string> _singleEyeEndTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "MECHANICAL_SINGLE_EYE_SPLICE",
            "HAND_SINGLE_END_EYE",
            "ONE_END_OPEN_EYE_THIMBLE_OTHER_END_MECHANICAL_EYE_SPLICE",
        };

        /// <summary>End types that accept two optional eye lengths (Left/Right).</summary>
        private static readonly HashSet<string> _bothEyeEndTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "MECHANICAL_BOTH_EYE_SPLICE",
            "HAND_BOTH_EYE_SPLICE",
        };

        public static bool IsWireRopeItemCode(string? itemCode) =>
            itemCode?.StartsWith(WireRopeItemCodePrefix, StringComparison.OrdinalIgnoreCase) == true;

        public static bool IsValidPlacement(string placement) => _placementOptions.Contains(placement);

        public static bool IsValidEndType(string endType) => _validEndTypes.Contains(endType);

        public static bool IsSingleEyeEndType(string endType) => _singleEyeEndTypes.Contains(endType);

        public static bool IsBothEyeEndType(string endType) => _bothEyeEndTypes.Contains(endType);
    }
}
