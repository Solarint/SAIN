using System;
using System.Collections.Generic;
using System.Linq;

namespace SAIN.Preset.GlobalSettings.Categories;

public class BotBrains
{
    public static string Parse(EBrain brain)
    {
        return brain.ToString();
    }

    public static EBrain Parse(string brain)
    {
        if (Enum.TryParse(brain, out EBrain result))
        {
            return result;
        }
        return EBrain.Assault;
    }

    public static readonly EBrain[] AllBrains = Enum.GetValues(typeof(EBrain)).Cast<EBrain>().ToArray();

    public static readonly List<EBrain> AllBrainsList = AllBrains.ToList();

    public static readonly EBrain[] Bosses =
    [
        EBrain.BossBully,
        EBrain.BossGluhar,
        EBrain.Knight,
        EBrain.BossKojaniy,
        EBrain.BossSanitar,
        EBrain.Tagilla,
        EBrain.TagillaAgro,
        //Brain.BossZryachiy,
        EBrain.Killa,
        EBrain.KillaAgro,
        EBrain.SectantPriest,
        EBrain.BossBoar,
        EBrain.BossKolontay,
        EBrain.BossPartisan,
    ];

    public static readonly EBrain[] Followers =
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
        EBrain.SectantWarrior,
        EBrain.BigPipe,
        EBrain.BirdEye,
        EBrain.FollowerBoar,
        EBrain.FollowerBoarClose1,
        EBrain.FollowerBoarClose2,
        EBrain.BossBoarSniper,
        EBrain.FollowerKolontayAssault,
        EBrain.FollowerKolontaySecurity,
    ];

    public static readonly EBrain[] Goons =
    [
        EBrain.Knight,
        EBrain.BigPipe,
        EBrain.BirdEye,
    ];

    public static readonly EBrain[] Special =
    [
        EBrain.BossTest,
        EBrain.Obdolbs,
        EBrain.Gifter,
        EBrain.CursAssault,
    ];
}