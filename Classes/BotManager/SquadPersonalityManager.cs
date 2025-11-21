using SAIN.Components;
using SAIN.Models.Enums;
using System.Collections.Generic;
using System.Text;

namespace SAIN.BotController.Classes;

public class SquadPersonalityManager
{
    public static ESquadPersonality GetSquadPersonality(Dictionary<string, BotComponent> Members, out SquadPersonalitySettings settings)
    {
        GetMemberPersonalities(Members);
        EPersonality mostFrequentPersonality = GetMostFrequentPersonality(PersonalityCounts, out int count);
        ESquadPersonality result = PickSquadPersonality(mostFrequentPersonality);
        settings = GetSquadSettings(result);
        return result;
    }

    private static void GetMemberPersonalities(Dictionary<string, BotComponent> Members)
    {
        PersonalityCounts.Clear();
        MemberPersonalities.Clear();

        foreach (var member in Members.Values)
        {
            if (member?.Player != null && member.Player.HealthController.IsAlive)
            {
                var personality = member.Info.Personality;
                MemberPersonalities.Add(personality);
                if (!PersonalityCounts.ContainsKey(personality))
                {
                    PersonalityCounts.Add(personality, 1);
                }
                else
                {
                    PersonalityCounts[personality]++;
                }
            }
        }

        StringBuilder stringbuilder = new();
        foreach (var personality in PersonalityCounts)
        {
            stringbuilder.AppendLine($"[{personality.Key}] : [{personality.Value}]");
        }

        //Logger.LogAndNotifyInfo(stringbuilder.ToString());

    }

    private static EPersonality GetMostFrequentPersonality(Dictionary<EPersonality, int> PersonalityCounts, out int count)
    {
        count = 0;
        EPersonality mostFrequent = EPersonality.Normal;
        foreach (var personalityCount in PersonalityCounts)
        {
            if (personalityCount.Value > count)
            {
                count = personalityCount.Value;
                mostFrequent = personalityCount.Key;
            }
        }

        //Logger.LogAndNotifyInfo($"Most Frequent Personality [{mostFrequent}] : Count {count}");
        return mostFrequent;
    }

    private static ESquadPersonality PickSquadPersonality(EPersonality mostFrequentPersonality)
    {
        ESquadPersonality result = ESquadPersonality.None;
        switch (mostFrequentPersonality)
        {
            case EPersonality.GigaChad:
            case EPersonality.Chad:
            case EPersonality.Wreckless:
                result = EFTMath.RandomBool(66) ? ESquadPersonality.GigaChads : ESquadPersonality.Elite;
                break;

            case EPersonality.Timmy:
            case EPersonality.Coward:
                result = ESquadPersonality.TimmyTeam6;
                break;

            case EPersonality.Rat:
            case EPersonality.SnappingTurtle:
                result = ESquadPersonality.Rats;
                break;

            default:
                result = Helpers.EnumValues.GetEnum<ESquadPersonality>().PickRandom();
                break;
        }

        //Logger.LogAndNotifyInfo($"Assigned Squad Personality of [{result}] because most frequent personality is [{mostFrequentPersonality}]");
        return result;
    }

    private static SquadPersonalitySettings GetSquadSettings(ESquadPersonality squadPersonality)
    {
        switch (squadPersonality)
        {
            case ESquadPersonality.Elite:
                return CreateSettings(squadPersonality, 2, 5, 4);

            case ESquadPersonality.GigaChads:
                return CreateSettings(squadPersonality, 5, 4, 5);

            case ESquadPersonality.Rats:
                return CreateSettings(squadPersonality, 1, 2, 1);

            case ESquadPersonality.TimmyTeam6:
                return CreateSettings(squadPersonality, 3, 1, 2);

            default:
                return CreateSettings(squadPersonality, 3, 3, 3);
        }
    }

    private static SquadPersonalitySettings CreateSettings(ESquadPersonality squadPersonality, float vocalization, float coordination, float aggression)
    {
        if (!SquadSettings.ContainsKey(squadPersonality))
        {
            var settings = new SquadPersonalitySettings
            {
                VocalizationLevel = vocalization,
                CoordinationLevel = coordination,
                AggressionLevel = aggression
            };
            SquadSettings.Add(squadPersonality, settings);
        }
        return SquadSettings[squadPersonality];
    }

    private static readonly List<EPersonality> MemberPersonalities = new();
    private static readonly Dictionary<EPersonality, int> PersonalityCounts = new();
    private static readonly Dictionary<ESquadPersonality, SquadPersonalitySettings> SquadSettings = new();
}
