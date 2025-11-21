using EFT;
using EFT.InventoryLogic;
using HarmonyLib;
using SAIN.Components;
using System;
using System.Collections.Generic;
using UnityEngine;
using static EFT.Player;
using HandEvent = GEventArgs1;

namespace SAIN.SAINComponent.Classes;

public class BotBusyHandsDetector : BotComponentClassBase
{
    private const float CHECK_FREQ = 0.5f;
    private const float TIME_TO_RESET_GENERIC = 5f;
    private const float TIME_TO_RESET_HEAL_FIRSTAID = 3f;
    private const float TIME_TO_RESET_HEAL_STIMS = 3f;
    private const float TIME_TO_RESET_HEAL_SURGERY = 30f;
    private const float TIME_TO_RESET_WEAPONS_RELOAD = 10f;
    private const float TIME_TO_RESET_WEAPONS_SWAP = 3f;
    private const float TIME_TO_RESET_WEAPONS_GRENADE = 3f;

    public BotBusyHandsDetector(BotComponent sain) : base(sain)
    {
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    public override void ManualUpdate()
    {
        CheckShallFix();
        base.ManualUpdate();
    }

    private void CheckShallFix()
    {
        if (_nextCheckTime < Time.time)
        {
            _nextCheckTime = Time.time + CHECK_FREQ;
            //checkBusyHands();
            //checkBusyTooLong();
        }
    }

    private void CheckBusyHands()
    {
        if (Player.HandsController is ItemHandsController itemHandsController)
        {
            _isInInteraction = itemHandsController.CurrentHandsOperation.State == EOperationState.Executing;
            _isInInteractionStrictCheck = itemHandsController.IsInInteractionStrictCheck();
            bool inInteraction = _isInInteraction || _isInInteractionStrictCheck;
            if (inInteraction)
            {
                if (_timeStartInteraction <= 0f)
                    _timeStartInteraction = Time.time;
                return;
            }
            if (!inInteraction)
            {
                if (_timeStartInteraction > 0f)
                    _timeStartInteraction = -1f;
                return;
            }
        }
    }

    private void CheckBusyTooLong()
    {
        float startTime = _timeStartInteraction;
        if (startTime <= 0f)
        {
            return;
        }
        if (BotHasBusyHands(startTime, out string reason))
        {
            ResetHands(reason);
        }
    }

    private bool BotHasBusyHands(float startTime, out string reason)
    {
        float timeSinceStart = Time.time - startTime;
        if (Player.HandsController is ItemHandsController itemController)
        {
            Item item = itemController.Item;
            if (item is StimulatorItemClass)
            {
                reason = "stims";
                return timeSinceStart > TIME_TO_RESET_HEAL_STIMS;
            }
            if (item is MedKitItemClass)
            {
                reason = "firstAid";
                return timeSinceStart > TIME_TO_RESET_HEAL_FIRSTAID;
            }
            if (item is MedicalItemClass medsItemClass && medsItemClass.HealthEffectsComponent.AffectsAny(EDamageEffectType.DestroyedPart))
            {
                reason = "surgery";
                return timeSinceStart > TIME_TO_RESET_HEAL_SURGERY;
            }
        }
        reason = "generic";
        return timeSinceStart > 20;
    }

    private void ResetHands(string reason)
    {
#if DEBUG
        Logger.LogWarning($"[{BotOwner.name}] is resetting hands because [{reason}] too long!");
#endif
        ResetHandsController(Player);
    }

    private readonly Dictionary<GEventArgs1, float> _ongoingEvents = [];
    private readonly List<HandEvent> _eventsToRemove = [];
    private readonly List<HandEvent> _events = [];

    // Credit to Lacyway's "Hands are Not Busy" mod https://github.com/Lacyway/HandsAreNotBusy/blob/main/HANB_Component.cs
    private static void ResetHandsController(Player player)
    {
        var hands = player.HandsController as Player.ItemHandsController;
#if DEBUG
        Logger.LogWarning($"[{hands?.Item?.Name} :: {hands.CurrentHandsOperationName}]");
#endif
        hands.FastForwardCurrentState();

        //InventoryController inventoryController = player.InventoryController;
        //if (inventoryController == null)
        //{
        //    Logger.LogError("FixHandsController: could not find '_inventoryController'");
        //    return;
        //}
        //int length = inventoryController.List_0.Count;
        //if (length > 0)
        //{
        //    HandEvent[] args = new HandEvent[length];
        //    inventoryController.List_0.CopyTo(args);
        //    foreach (HandEvent queuedEvent in args)
        //    {
        //        inventoryController.RemoveActiveEvent(queuedEvent);
        //    }
        //    Logger.LogInfo($"Cleared {length} stuck inventory operations.");
        //}
        //
        //AbstractHandsController handsController = player.HandsController;
        //
        //if (handsController is FirearmController currentFirearmController)
        //{
        //    player.MovementContext.OnStateChanged -= currentFirearmController.method_17;
        //    player.Physical.OnSprintStateChangedEvent -= currentFirearmController.method_16;
        //    currentFirearmController.RemoveBallisticCalculator();
        //}
        //
        //try
        //{
        //    player.SpawnController(player.method_156());
        //}
        //catch (Exception ex)
        //{
        //    Logger.LogWarning("Stopped exception when spawning controller. InnerException: " + ex.InnerException);
        //}
        //
        //if (player.LastEquippedWeaponOrKnifeItem != null)
        //{
        //    InteractionsHandlerClass.Discard(player.LastEquippedWeaponOrKnifeItem, inventoryController, true);
        //
        //    player.ProcessStatus = EProcessStatus.None;
        //    player.TrySetLastEquippedWeapon();
        //}
        //else
        //{
        //    player.ProcessStatus = EProcessStatus.None;
        //    player.SetFirstAvailableItem(PlayerOwner.Class1667.class1667_0.method_0);
        //}
        //
        //player.SetInventoryOpened(false);
        //handsController?.Destroy();
        //
        //if (handsController != null)
        //{
        //    GameObject.Destroy(handsController);
        //}
        //
        //// This fixes a null ref error
        //if (player.HandsController is FirearmController firearmController && firearmController.Weapon != null)
        //{
        //    Traverse.Create(player.ProceduralWeaponAnimation).Field("_firearmAnimationData").SetValue(firearmController);
        //}
    }

    private float _timeStartInteraction = -1f;
    private bool _isInInteraction;
    private bool _isInInteractionStrictCheck;
    private float _nextCheckTime;
}