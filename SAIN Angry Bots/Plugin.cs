using BepInEx;
using DrakiaXYZ.VersionChecker;
using SAIN.Helpers;
using SAIN.Talk.Components;
using SAIN.Talk.UserSettings;
using System;
using System.Diagnostics;

namespace SAIN.Talk
{
    [BepInPlugin("me.sol.saintalk", "SAIN Layers AI Talk", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            if (!TarkovVersion.CheckEftVersion(Logger, Info, Config))
            {
                throw new Exception($"Invalid EFT Version");
            }

            try
            {
                TalkConfig.Init(Config);

                new Patches.PlayerTalkPatch().Enable();
                new Patches.BotGlobalSettingsPatch().Enable();

                new Patches.TalkDisablePatch1().Enable();
                new Patches.TalkDisablePatch2().Enable();
                new Patches.TalkDisablePatch3().Enable();
                new Patches.TalkDisablePatch4().Enable();

                new Patches.AddComponentPatch().Enable();
                new Patches.DisposeComponentPatch().Enable();
            }
            catch (Exception ex)
            {
                Logger.LogError($"{GetType().Name}: {ex}");
                throw;
            }
        }

        private void Update()
        {
            AddComponent.AddSingleComponent<PlayerTalkComponent>();
        }

        private readonly MainPlayerComponentSingle AddComponent = new MainPlayerComponentSingle();
    }
}