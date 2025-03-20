using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EFT.Player;
using HandEvent = GEventArgs1;

namespace SAIN.SAINComponent.Classes
{
    public class BotBusyHandsDetector : BotBase, IBotClass
    {
        private const float CHECK_FREQ = 0.1f;
        private const float TIME_TO_RESET_GENERIC = 5f;
        private const float TIME_TO_RESET_HEAL_FIRSTAID = 10f;
        private const float TIME_TO_RESET_HEAL_STIMS = 3f;
        private const float TIME_TO_RESET_HEAL_SURGERY = 40f;
        private const float TIME_TO_RESET_WEAPONS_RELOAD = 10f;
        private const float TIME_TO_RESET_WEAPONS_SWAP = 3f;
        private const float TIME_TO_RESET_WEAPONS_GRENADE = 3f;

        public BotBusyHandsDetector(BotComponent sain) : base(sain)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            checkShallFix();
        }

        public void Dispose()
        {
        }

        private void checkShallFix()
        {
            if (_nextCheckTime < Time.time)
            {
                _nextCheckTime = Time.time + CHECK_FREQ;
                checkBusyHands();
                checkBusyTooLong();
            }
        }

        private void checkBusyHands()
        {
            var handsController = Player.HandsController;
            if (handsController != null)
            {
                _isInInteraction = handsController.IsInInteraction();
                _isInInteractionStrictCheck = _isInInteraction || handsController.IsInInteractionStrictCheck();

                bool inInteraction = _isInInteraction || _isInInteractionStrictCheck;
                if (inInteraction)
                {
                    logTimeSince();
                    collectQueEvents();
                    if (_timeStartInteraction <= 0f)
                        _timeStartInteraction = Time.time;
                    return;
                }
                if (!inInteraction)
                {
                    _OngoingEvents.Clear();
                    if (_timeStartInteraction > 0f)
                        _timeStartInteraction = -1f;
                    return;
                }
            }
        }

        private void checkBusyTooLong()
        {
            float startTime = _timeStartInteraction;
            if (startTime <= 0f)
            {
                return;
            }
            if (botHasBusyHands(startTime, out string reason))
            {
                resetHands(reason);
            }
        }

        private bool botHasBusyHands(float startTime, out string reason)
        {
            float timeSinceStart = Time.time - startTime;
            var meds = BotOwner.Medecine;
            if (meds != null)
            {
                if (meds.Stimulators?.Using == true)
                {
                    reason = "stims";
                    return timeSinceStart > TIME_TO_RESET_HEAL_STIMS;
                }
                if (meds.FirstAid?.Using == true)
                {
                    reason = "firstAid";
                    return timeSinceStart > TIME_TO_RESET_HEAL_FIRSTAID;
                }
                if (meds.SurgicalKit?.Using == true)
                {
                    reason = "surgery";
                    return timeSinceStart > TIME_TO_RESET_HEAL_SURGERY;
                }
            }
            var weaponManager = BotOwner.WeaponManager;
            if (weaponManager != null)
            {
                if (weaponManager.Reload.Reloading)
                {
                    reason = "reloading";
                    return timeSinceStart > TIME_TO_RESET_WEAPONS_RELOAD;
                }
                if (weaponManager.Selector.IsChanging)
                {
                    reason = "changingWeapon";
                    return timeSinceStart > TIME_TO_RESET_WEAPONS_SWAP;
                }
                if (weaponManager.Grenades.ThrowindNow)
                {
                    reason = "throwingGrenade";
                    return timeSinceStart > TIME_TO_RESET_WEAPONS_GRENADE;
                }
            }
            reason = "generic";
            return timeSinceStart > TIME_TO_RESET_GENERIC;
        }

        private void resetHands(string reason)
        {
            Logger.LogWarning($"[{BotOwner.name}] is resetting hands because [{reason}] too long!");
            resetHandsController(Player);
        }

        private void collectQueEvents()
        {
            InventoryController inventoryController = Player.InventoryController;
            if (inventoryController == null)
            {
                Logger.LogError("FixHandsController: could not find '_inventoryController'");
                return;
            }
            if (inventoryController.List_0.Count > 0)
            {
                _events.Clear();
                _events.AddRange(inventoryController.List_0);
                float time = Time.time;

                foreach (HandEvent queuedEvent in _events)
                    if (!_OngoingEvents.ContainsKey(queuedEvent))
                        _OngoingEvents.Add(queuedEvent, time);

                _eventsToRemove.Clear();

                foreach (var queuedEvent in _OngoingEvents)
                    if (!_events.Contains(queuedEvent.Key))
                        _eventsToRemove.Add(queuedEvent.Key);

                foreach (var queuedEvent in _eventsToRemove)
                    _OngoingEvents.Remove(queuedEvent);

                _events.Clear();
                _eventsToRemove.Clear();
            }
        }

        private void logTimeSince()
        {
            float time = Time.time;
            foreach (var queuedEvent in _OngoingEvents)
            {
                Logger.LogDebug($"[{queuedEvent.Key.EventId}] : [{time - queuedEvent.Value}]");
            }
        }

        private Dictionary<GEventArgs1, float> _OngoingEvents = new Dictionary<GEventArgs1, float>();
        private List<HandEvent> _eventsToRemove = new List<HandEvent>();
        private List<HandEvent> _events = new List<HandEvent>();

        // Credit to Lacyway's "Hands are Not Busy" mod https://github.com/Lacyway/HandsAreNotBusy/blob/main/HANB_Component.cs
        private static void resetHandsController(Player player)
        {
            InventoryController inventoryController = player.InventoryController;
            if (inventoryController == null)
            {
                Logger.LogError("FixHandsController: could not find '_inventoryController'");
                return;
            }
            int length = inventoryController.List_0.Count;
            if (length > 0)
            {
                HandEvent[] args = new HandEvent[length];
                inventoryController.List_0.CopyTo(args);
                foreach (HandEvent queuedEvent in args)
                {
                    inventoryController.RemoveActiveEvent(queuedEvent);
                }
                Logger.LogInfo($"Cleared {length} stuck inventory operations.");
            }

            AbstractHandsController handsController = player.HandsController;

            if (handsController is FirearmController currentFirearmController)
            {
				player.MovementContext.OnStateChanged -= currentFirearmController.method_17;
				player.Physical.OnSprintStateChangedEvent -= currentFirearmController.method_16;
				currentFirearmController.RemoveBallisticCalculator();
			}

            try
            {
                player.SpawnController(player.HandsController);
            }
            catch (Exception ex)
            {
                Logger.LogWarning("Stopped exception when spawning controller. InnerException: " + ex.InnerException);
            }

            if (player.LastEquippedWeaponOrKnifeItem != null)
            {
				InteractionsHandlerClass.Discard(player.LastEquippedWeaponOrKnifeItem, inventoryController, true);

				player.ProcessStatus = EProcessStatus.None;
                player.TrySetLastEquippedWeapon();
            }
            else
            {
                player.ProcessStatus = EProcessStatus.None;
                player.SetFirstAvailableItem(PlayerOwner.Class1667.class1667_0.method_0);
            }

            player.SetInventoryOpened(false);
            handsController?.Destroy();

            if (handsController != null)
            {
                GameObject.Destroy(handsController);
            }

            // This fixes a null ref error
            if (player.HandsController is FirearmController firearmController && firearmController.Weapon != null)
            {
                Traverse.Create(player.ProceduralWeaponAnimation).Field("_firearmAnimationData").SetValue(firearmController);
            }
        }

        private float _timeStartInteraction = -1f;
        private bool _isInInteraction;
        private bool _isInInteractionStrictCheck;
        private float _nextCheckTime;
    }
}