using EFT;
using EFT.InventoryLogic;
using SAIN.Components;
using SAIN.Models.Enums;
using System.Collections;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class BotBackpackDropClass : BotComponentClassBase
{
    public BotBackpackDropClass(BotComponent sain) : base(sain)
    {
        
        TickRequirement = ESAINTickState.OnlyNoSleep;
    }

    public override void ManualUpdate()
    {
        if (BackpackDropPosition != null &&
            DroppedBackpack == null)
        {
            BackpackDropPosition = null;
        }
        //if (Bot.Enemy != null)
        //{
        //    DropBackpack();
        //}
        //if (_wantToDrop && _nextTryDropTime < Time.time)
        //{
        //   _nextTryDropTime = Time.time + 0.5f;
        //   executeDrop();
        //}
    }

    //private float _nextTryDropTime;

    public bool DropBackpack()
    {
        //Item backpack = _currentBackpack;
        //if (DroppedBackpack == null && backpack == null)
        //{
        //    _wantToDrop = false;
        //    BackpackStatus = EBackpackStatus.NoBackpack;
        //    return false;
        //}
        //if (DroppedBackpack == null &&
        //    backpack != null)
        //{
        //    _wantToDrop = true;
        //    return true;
        //}
        return false;
    }

    //private bool _wantToDrop;

    private void ExecuteDrop()
    {
        if (_dropCoroutine != null)
        {
            return;
        }
        _dropCoroutine = Bot.StartCoroutine(ExecuteBackpackDrop());
    }

    private Coroutine _dropCoroutine;

    private IEnumerator ExecuteBackpackDrop()
    {
        Item backpack = CurrentBackpack;
        if (backpack != null)
        {
            if (DroppedBackpack == null)
            {
                DroppedBackpack = backpack;
            }
            Player.DropBackpack();
        }

        yield return new WaitForSeconds(0.5f);

        if (CurrentBackpack == null)
        {
            //_wantToDrop = false;
            BackpackDropPosition = new Vector3?(Bot.Position);
            BackpackStatus = EBackpackStatus.Dropped;
            Logger.LogInfo($"{BotOwner.name} Dropped Backpack at {Bot.Position} at {Time.time}");
        }
    }

    public EBackpackStatus BackpackStatus { get; private set; }

    private Item CurrentBackpack => Bot.PlayerComponent.Equipment.GearInfo.GetItem(EquipmentSlot.Backpack);

    public bool RetreiveBackpack()
    {
        return false;
    }

    public bool BackpackDropped => BackpackDropPosition != null;

    public Item DroppedBackpack { get; private set; }

    public Vector3? BackpackDropPosition { get; private set; }
}