namespace BluePrinceArchipelago
{
    /// <summary>
    ///  The Enum for the goals.
    /// </summary>
    public enum GoalType
    {
        option_antechamber = 0,
        option_room46 = 1,
        option_sanctum = 2,
        option_ascend = 3,
        option_blueprints = 4
    }

    /// <summary>
    ///     The enum for the death link types.
    /// </summary>
    public enum DeathLinkType {
        option_none = 0,
        option_eod = 1,
        option_bedroom = 2,
        option_steps = 3
    }

    /// <summary>
    ///     The enum for item trunk logic.
    /// </summary>
    public enum ItemLogicMode {
        option_default = 0,
        option_rare = 1,
        option_complex = 2,
        option_rare_complex = 3,
        option_extreme = 4
    }
}
