using BepInEx.Logging;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using SAIN.SAINComponent;
using UnityEngine;

namespace SAIN.Layers.Combat.Solo
{
    public class ThrowGrenadeAction : SAINAction
    {
        public ThrowGrenadeAction(BotOwner bot) : base(bot, nameof(ThrowGrenadeAction))
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
        }

        public override void Update()
        {
            if (!Stopped && Time.time - StartTime > 1f || SAIN.Cover.CheckLimbsForCover())
            {
                Stopped = true;
                BotOwner.StopMove();
            }

            ExecuteThrow();

            if (SAIN.Cover.BotIsAtCoverPoint())
            {
                return;
            }
            else
            {
                bool prone = SAIN.Mover.Prone.ShallProne(true);
                SAIN.Mover.Prone.SetProne(prone);
            }
        }

        public void ExecuteThrow()
        {
            BotOwner.WeaponManager.Grenades.DoThrow();
            SAIN.Talk.Say(EPhraseTrigger.OnGrenade);
        }

        private readonly SAINComponentClass SAIN;

        public ManualLogSource Logger;
        private float StartTime = 0f;
        private bool Stopped = false;

        public override void Start()
        {
            StartTime = Time.time;
        }

        public override void Stop()
        {
        }
    }
}