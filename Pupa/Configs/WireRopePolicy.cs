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

        /// <summary>GroupCode values (IC_InventoryUserGroup) that resolve to the TANKER fleet.</summary>
        private static readonly HashSet<string> _tankerGroupCodes = new(StringComparer.OrdinalIgnoreCase)
            { "TK", "TK1", "TK2", "TK3", "TK4", "TK5", "TK6", "TK7", "ST" };

        /// <summary>GroupCode values that resolve to the TOWING fleet (harbour tug / towing barge).</summary>
        private static readonly HashSet<string> _towingGroupCodes = new(StringComparer.OrdinalIgnoreCase)
            { "TB", "TG" };

        /// <summary>
        /// Per-vessel Placement overrides for named Bulk Carrier vessels.
        /// Bulk Carrier vessels not listed here fall back to the generic
        /// Placement list (no data available for them).
        /// </summary>
        private static readonly Dictionary<string, string[]> _bulkCarrierVesselPlacements =
            new(StringComparer.OrdinalIgnoreCase)
        {
            ["MV. ASIAN WISDOM"] = new[]
            {
                "HOISTING", "LUFFING",
                "GRAB GUVEN (Z LAY)", "GRAB GUVEN (S LAY)",
                "GRAB JANUS (Z LAY)", "GRAB JANUS (S LAY)",
                "LIFEBOAT LASHING TRIGGER (PORT SIDE / STARBOARD SIDE)",
                "LIFEBOAT DAVIT - MOTOR SIDE", "LIFEBOAT DAVIT - BRAKE SIDE",
                "LIFEBOAT RELEASE CONTROL",
            },
            ["MV. PAN GLORY"] = new[]
            {
                "HOISTING", "LUFFING",
                "GRAB SMAG (Z LAY)",
                "GRAB JANUS (Z LAY)", "GRAB JANUS (S LAY)",
                "COMBINATION LADDER",
            },
            ["MV. SKY FREE"] = new[]
            {
                "HOISTING", "LUFFING",
                "GRAB",
                "GRAB JANUS YK20 (Z LAY)", "GRAB JANUS YK20 (S LAY)",
                "GRAB (Z LAY)", "GRAB (S LAY)",
                "ACCOMODATION LADDER - HOISTING (PORT SIDE / STARBOARD SIDE)",
                "PILOT LADDER",
            },
        };

        /// <summary>
        /// Placement list for the TANKER fleet. Source: mapping.xlsx rows for
        /// FLEET="TANKER" and FLEET="TANKER " (trailing space) — merged into a
        /// single list per product decision (both are legitimate tanker
        /// equipment, e.g. gas/LPG tanker cargo-compressor mast rigging, not a
        /// mislabeled towing list).
        /// </summary>
        private static readonly HashSet<string> _tankerPlacements = new(StringComparer.OrdinalIgnoreCase)
        {
            "ACCOMODATION LADDER - HOISTING (PORT SIDE / STARBOARD SIDE)",
            "ACCOMODATION LADDER - STOWING",
            "ANCHOR LASHING",
            "CARGO CRANE",
            "CARGO HOSE HANDLING CRANE",
            "FIRE WIRE",
            "FREEFALL LIFEBOAT",
            "GRABLINE LIFEBOAT & RESCUE BOAT",
            "HOSE HANDLING CRANE",
            "LABRANG",
            "LIFEBOAT",
            "LIFEBOAT DAVIT",
            "MOORING",
            "OVERHEAD CRANE",
            "OVERHEAD CRANE - ENGINE ROOM",
            "PROVISION CRANE",
            "PROVISION CRANE - HOISTING",
            "PROVISION CRANE - LUFFING",
            "RESCUE BOAT",
            "SAFETY WIRE",
            "AFT SIDE BOAT FALL",
            "AUTO TRIGGER LINE",
            "DECK REMOTE CONTROL",
            "FIRE WIRE FOR SAFETY FWD AND AFT",
            "INBOARD REMOTE CONTROL",
            "LABRANG FOR MAST AFT",
            "LABRANG FOR MAST CARGO COMPRESSOR",
            "LABRANG FOR MAST FORWARD",
            "LABRANG FOR MAST MIDLE/MANIFOLD",
            "LIFEBOAT DAVIT REMOTE",
            "OVER LASHING LINE",
            "SIDE BOAT FALL",
        };

        /// <summary>Placement list for the TOWING fleet.</summary>
        private static readonly HashSet<string> _towingPlacements = new(StringComparer.OrdinalIgnoreCase)
        {
            "BRIDLE", "JANGKAR TONGKANG",
        };

        /// <summary>
        /// Generic Placement list used as a fallback for any vessel that isn't
        /// a named Bulk Carrier override, Tanker, or Towing vessel. Source:
        /// mapping.xlsx sheet "beside bc, tanker, towing".
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

        /// <summary>
        /// Resolves the vessel's Wire Rope fleet ("BULK_CARRIER", "TANKER",
        /// "TOWING" or "OTHER") and the allowed Placement set for that vessel.
        /// Named Bulk Carrier vessels take priority over the generic Bulk
        /// Carrier fallback.
        /// </summary>
        public static (string Fleet, IReadOnlySet<string> Placements) ResolveFleetAndPlacements(
            string vesselName, string? groupCode)
        {
            var name = (vesselName ?? "").Trim();

            if (_bulkCarrierVesselPlacements.TryGetValue(name, out var vesselPlacements))
                return ("BULK_CARRIER", new HashSet<string>(vesselPlacements, StringComparer.OrdinalIgnoreCase));

            if (string.Equals(groupCode, "BC", StringComparison.OrdinalIgnoreCase))
                return ("BULK_CARRIER", _placementOptions);

            if (_tankerGroupCodes.Contains(groupCode ?? ""))
                return ("TANKER", _tankerPlacements);

            if (_towingGroupCodes.Contains(groupCode ?? ""))
                return ("TOWING", _towingPlacements);

            return ("OTHER", _placementOptions);
        }

        public static bool IsValidPlacement(string placement) => _placementOptions.Contains(placement);

        public static bool IsValidEndType(string endType) => _validEndTypes.Contains(endType);

        public static bool IsSingleEyeEndType(string endType) => _singleEyeEndTypes.Contains(endType);

        public static bool IsBothEyeEndType(string endType) => _bothEyeEndTypes.Contains(endType);
    }
}
