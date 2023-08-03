
namespace SAIN.BotPresets
{
    class PresetDefaults
    {
        static void InitGeneral()
        {
            string name = "Faster CQB Reactions";
            string desc = "Sets whether this bot reacts faster at close ranges";
            string section = "General";

            name = "Faster CQB Reactions Max Distance";
            desc = "Max distance a bot can react faster for Faster CQB Reactions. Scales with distance.";
            section = "General";
            float def = 30f;
            float min = 1f;
            float max = 100f;
            float round = 1f;

            name = "Faster CQB Reactions Minimum Speed";
            desc = "Absolute minimum speed (in seconds) that bot can react and shoot";
            section = "General";
            def = 0.1f;
            min = 0.0f;
            max = 1f;
            round = 100f;

            name = "Can Use Grenades";
            desc = "Can This Bot Use Grenades at all?";
            section = "General";
        }

        static void InitVision()
        {
            string name = "Max Visible Distance";
            string desc = "The Maximum Vision Distance for this bot";
            string section = "Vision";
            float defaultVal = 150f;
            float min = 50f;
            float max = 500f;
            float rounding = 1f;

            name = "Visible Angle";
            desc = "The Maximum Vision Cone for a bot";
            section = "Vision";
            defaultVal = 160f;
            min = 45;
            max = 180f;
            rounding = 1f;

            name = "Base Speed Multiplier";
            desc = "The Base vision speed multiplier, affects all ranges to enemy. " +
                "Bots will see this much faster, or slower, at any range. " +
                "Higher is slower speed, so 1.5 would result in bots taking 1.5 times longer to spot an enemy";
            section = "Vision";
            defaultVal = 1f;
            min = 0.25f;
            max = 3f;
            rounding = 100f;

            name = "Close Multiplier";
            desc = "Vision speed multiplier at close range. " +
                "Bots will see this much faster, or slower, at close range. " +
                "Higher is slower speed, so 1.5 would result in bots taking 1.5 times longer to spot an enemy";
            section = "Vision";
            defaultVal = 1f;
            min = 0.25f;
            max = 3f;
            rounding = 100f;

            name = "Far Multiplier";
            desc = "Vision speed multiplier at Far range, the range is defined by (Close/Far Threshold Property). " +
                "Bots will see this much faster, or slower, at Far range. " +
                "Higher is slower speed, so 1.5 would result in bots taking 1.5 times longer to spot an enemy";
            section = "Vision";
            defaultVal = 1f;
            min = 0.25f;
            max = 3f;
            rounding = 100f;

            name = "Close/Far Threshold";
            desc = "The Distance that defines what is close or far for the Close Speed and Far Speed properties.";
            section = "Vision";
            defaultVal = 50f;
            min = 5f;
            max = 100f;
            rounding = 1f;
        }

        static void InitHearing()
        {
            string name = "Audible Range Multiplier";
            string desc = "Modifies the distance that this bot can hear sounds.";
            string section = "Hearing";
            float def = 1f;
            float min = 0.25f;
            float max = 3f;
            float round = 100f;

            name = "Max Footstep Audio Distance";
            desc = "The Maximum Range that a bot can hear footsteps, in meters.";
            section = "Hearing";
            def = 50f;
            min = 5f;
            max = 100f;
            round = 1f;
        }

        static void InitShoot()
        {
            string name = "Accuracy Spread Multiplier";
            string desc = "Modifies a bot's base accuracy and spread. Higher = less accurate. 1.5 = 1.5x higher accuracy spread";
            string section = "Shoot/Aim";
            float def = 1f;
            float min = 0.1f;
            float max = 5f;
            float round = 100f;

            name = "Accuracy Speed Multiplier";
            desc = "Modifies a bot's Accuracy Speed, or how fast their accuracy improves over time when shooting. " +
                "Higher = longer to gain accuracy. 1.5 = 1.5x longer to aim";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Max Aim Time Multiplier";
            desc = "Modifies the maximum time a bot can aim, or how long in seconds a bot takes to finish aiming. " +
                "Higher = longer to full aim. 1.5 = 1.5x longer to aim";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Recoil Scatter Multiplier";
            desc = "Modifies a bot's recoil impulse from a shot. Higher = less accurate. 1.5 = 1.5x more recoil and scatter per shot";
            section = "Shoot/Aim";
            def = 1f;
            min = 0.25f;
            max = 3f;
            round = 100f;

            name = "Burst Length Multiplier";
            desc = "Modifies how long bots shoot a burst during full auto fire. Higher = longer full auto time. 1.5 = 1.5x longer bursts";
            section = "Shoot/Aim";
            def = 1.25f;
            min = 0.25f;
            max = 3f;
            round = 100f;

            name = "Semiauto Firerate Multiplier";
            desc = "Modifies the time a bot waits between semiauto fire. Higher = faster firerate. 1.5 = 1.5x more shots per second";
            section = "Shoot/Aim";
            def = 1.35f;
            min = 0.25f;
            max = 3f;
            round = 100f;
        }

        static void InitTalk()
        {
            string name = "Talk Frequency";
            string desc = "Multiplies how often this bot can say voicelines.";
            string section = "Talk";
            float def = 1f;
            float min = 0.25f;
            float max = 3f;
            float round = 100f;

            name = "Can Talk";
            desc = "Sets whether this bot can talk or not";
            section = "Talk";

            name = "Taunts";
            desc = "Enables bots yelling nasty words at you.";
            section = "Talk";

            name = "Squad Talk";
            desc = "Enables bots talking to each other in a squad";
            section = "Talk";

            name = "Squad Talk Multiplier";
            desc = "Multiplies the time between squad voice communication";
            section = "Talk";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;

            name = "Squad Leader Talk Multiplier";
            desc = "Multiplies the time between squad Leader commands and callouts";
            section = "Talk";
            def = 1f;
            min = 0.1f;
            max = 5f;
            round = 100f;
        }

        static void initExtract()
        {
            string name = "Extracts";
            string desc = "Can This Bot Use Extracts?";
            string section = "Extract";

            name = "Extract Max Percentage";
            desc = "The shortest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent";
            section = "Extract";
            float def = 35f;
            float min = 1f;
            float max = 99f;
            float round = 1f;

            name = "Extract Min Percentage";
            desc = "The longest possible time before this bot can decide to move to extract. " +
                "Based on total raid timer and time remaining. 60 min total raid time with 6 minutes remaining would be 10 percent";
            section = "Extract";
            def = 5f;
            min = 1f;
            max = 99f;
            round = 1f;
        }
    }
}
