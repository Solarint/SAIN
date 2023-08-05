using BepInEx.Logging;
using Comfort.Common;
using DrakiaXYZ.BigBrain.Brains;
using EFT;
using EFT.InventoryLogic;
using SAIN.SAINComponent;
using SAIN.SAINComponent.Classes.Decision;
using SAIN.SAINComponent.Classes.Talk;
using SAIN.SAINComponent.Classes.WeaponFunction;
using SAIN.SAINComponent.Classes.Mover;
using SAIN.SAINComponent.Classes;
using SAIN.SAINComponent.SubComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Layers
{
    public class ThrowGrenade : CustomLogic
    {
        public ThrowGrenade(BotOwner bot) : base(bot)
        {
            Logger = BepInEx.Logging.Logger.CreateLogSource(this.GetType().Name);
            SAIN = bot.GetComponent<SAINComponentClass>();
        }

        public override void Update()
        {
            if ((!Stopped && Time.time - StartTime > 1f) || SAIN.Cover.CheckLimbsForCover())
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