using System.Collections.Generic;
using System.Linq;

namespace SAIN.Preset.GlobalSettings.Categories;

public enum EBrain
{
    ArenaFighter,
    BossBully,
    BossGluhar,
    BossBoar,
    BossPartisan,
    Knight,
    BossKojaniy,
    BossSanitar,
    BossKolontay,
    Tagilla,
    TagillaAgro,
    BossTest,
    //BossZryachiy,
    Obdolbs,
    ExUsec,
    BigPipe,
    BirdEye,
    FollowerBully,
    FollowerGluharAssault,
    FollowerGluharProtect,
    FollowerGluharScout,
    FollowerKojaniy,
    FollowerSanitar,
    FollowerBoar,
    FollowerBoarClose1,
    FollowerBoarClose2,
    BossBoarSniper,
    FollowerKolontayAssault,
    FollowerKolontaySecurity,
    TagillaFollower,
    TagillaHelperAgro,
    //Fl_Zraychiy,
    Gifter,
    Killa,
    KillaAgro,
    Marksman,
    PMC,
    SectantPriest,
    SectantWarrior,
    CursAssault,
    Assault,
    PmcBear,
    PmcUsec,
    FlBoarCl,
    FlBoarSt,
    FlKlnAslt,
    KolonSec,
}

public static class AIBrains
{
    private static IReadOnlyCollection<string> _allowedPlayerScavBrains;

    public static IReadOnlyCollection<string> AllowedPlayerScavBrains
    {
        get
        {
            if (_allowedPlayerScavBrains == null)
            {
                List<string> combinedBrains = [];

                foreach (var brain in AllowedPMCBrains)
                {
                    if (!combinedBrains.Contains(brain))
                    {
                        combinedBrains.Add(brain);
                    }
                }

                foreach (var brain in AllowedScavBrains)
                {
                    if (!combinedBrains.Contains(brain))
                    {
                        combinedBrains.Add(brain);
                    }
                }

                _allowedPlayerScavBrains = combinedBrains.AsReadOnly();
            }

            return _allowedPlayerScavBrains;
        }
    }

    public static IReadOnlyCollection<string> AllowedPMCBrains
    {
        get
        {
            if (_allowedPMCBrains == null)
            {
                List<EBrain> brains = [.. PMCs];
                if (BigBrainHandler.INCLUDE_RAIDER_BRAIN_FOR_PMCS)
                {
                    brains.Add(EBrain.PMC);
                }
                _allowedPMCBrains = brains.ConvertAll(brain => brain.ToString())
                    .AsReadOnly();
            }
            return _allowedPMCBrains;
        }
    }

    private static IReadOnlyCollection<string> _allowedPMCBrains;

    public static IReadOnlyCollection<string> AllowedScavBrains
    {
        get
        {
            if (_allowedScavBrains == null)
            {
                // PMC brain is needed for assaultGroup scavs
                List<EBrain> brains = [EBrain.PMC, .. Scavs];
                _allowedScavBrains = brains.ConvertAll(brain => brain.ToString())
                    .AsReadOnly();
            }
            return _allowedScavBrains;
        }
    }

    private static IReadOnlyCollection<string> _allowedScavBrains;

    public static readonly List<EBrain> PMCs =
    [
        EBrain.PmcBear,
        EBrain.PmcUsec,
    ];

    public static readonly List<EBrain> Scavs =
    [
        EBrain.CursAssault,
        EBrain.Assault,
    ];

    public static readonly List<EBrain> Goons =
    [
        EBrain.Knight,
        EBrain.BirdEye,
        EBrain.BigPipe,
    ];

    public static readonly List<EBrain> Others =
    [
        EBrain.Obdolbs,
    ];

    public static readonly List<EBrain> Bosses =
    [
        EBrain.BossBully,
        EBrain.BossGluhar,
        EBrain.BossKojaniy,
        EBrain.BossSanitar,
        EBrain.Tagilla,
        EBrain.TagillaAgro,
        EBrain.BossTest,
        //Brain.BossZryachiy,
        EBrain.Gifter,
        EBrain.Killa,
        EBrain.KillaAgro,
        EBrain.SectantPriest,
        EBrain.BossBoar,
        EBrain.BossKolontay,
        EBrain.BossPartisan
    ];

    public static readonly List<EBrain> Followers =
    [
        EBrain.FollowerBully,
        EBrain.FollowerGluharAssault,
        EBrain.FollowerGluharProtect,
        EBrain.FollowerGluharScout,
        EBrain.FollowerKojaniy,
        EBrain.FollowerSanitar,
        EBrain.TagillaFollower,
        EBrain.TagillaHelperAgro,
        //Brain.Fl_Zraychiy,
        EBrain.FollowerBoar,
        EBrain.FollowerBoarClose1,
        EBrain.FollowerBoarClose2,
        EBrain.BossBoarSniper,
        EBrain.FollowerKolontayAssault,
        EBrain.FollowerKolontaySecurity,
        EBrain.FlBoarCl,
        EBrain.FlBoarSt,
    ];
}