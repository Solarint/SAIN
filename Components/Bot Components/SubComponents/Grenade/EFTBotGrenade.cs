using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Comfort.Common;
using EFT;
using EFT.InventoryLogic;
using UnityEngine;

namespace SAIN.Classes
{
    public class EFTBotGrenade : SAINBot
    {
        public GClass516 AIGreanageThrowData { get; private set; }

        public event Action<GrenadeClass> OnGrenadeThrowComplete
        {
            [CompilerGenerated]
            add
            {
                Action<GrenadeClass> action = action_0;
                Action<GrenadeClass> action2;
                do
                {
                    action2 = action;
                    Action<GrenadeClass> value2 = (Action<GrenadeClass>)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action<GrenadeClass>>(ref action_0, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action<GrenadeClass> action = action_0;
                Action<GrenadeClass> action2;
                do
                {
                    action2 = action;
                    Action<GrenadeClass> value2 = (Action<GrenadeClass>)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action<GrenadeClass>>(ref action_0, value2, action2);
                }
                while (action != action2);
            }
        }

        public event Action OnGrenadeThrowStart
        {
            [CompilerGenerated]
            add
            {
                Action action = action_1;
                Action action2;
                do
                {
                    action2 = action;
                    Action value2 = (Action)Delegate.Combine(action2, value);
                    action = Interlocked.CompareExchange<Action>(ref action_1, value2, action2);
                }
                while (action != action2);
            }
            [CompilerGenerated]
            remove
            {
                Action action = action_1;
                Action action2;
                do
                {
                    action2 = action;
                    Action value2 = (Action)Delegate.Remove(action2, value);
                    action = Interlocked.CompareExchange<Action>(ref action_1, value2, action2);
                }
                while (action != action2);
            }
        }

        public bool ThrowindNow { get; private set; }

        public float Mass { get; private set; }

        public bool HaveGrenade
        {
            get
            {
                return SavedGrenade != null;
            }
        }
        public bool ReadyToThrow
        {
            get
            {
                return AIGreanageThrowData != null && HaveGrenade && !AIGreanageThrowData.ThrowComplete;
            }
        }
        public Vector3 StartThrow
        {
            get
            {
                return BotOwner.GetPlayer.WeaponRoot.position;
            }
        }

        public float MaxPower
        {
            get
            {
                return BotOwner.Settings.FileSettings.Grenade.MAX_THROW_POWER / Mass;
            }
        }

        public bool NearLastThrow
        {
            get
            {
                return Time.time - float_2 < BotOwner.Settings.FileSettings.Grenade.NEAR_DELTA_THROW_TIME_SEC;
            }
        }

        public Vector3 ToThrowDirection
        {
            get
            {
                return AIGreanageThrowData.Direction + vector3_0;
            }
        }

        public EFTBotGrenade(BotOwner owner) : base(owner)
        {
            GClass564.Core.G = Mathf.Abs(Physics.gravity.y);
            method_0();
            method_2();
        }

        public bool HaveGrenadeOfType(ThrowWeapType grenadeType)
        {
            using (IEnumerator<Item> enumerator = SAIN.Equipment.Inventory.GetAllEquipmentItems().Where(new Func<Item, bool>(EFTBotGrenade.Class138.class138_0.method_0)).GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    if ((enumerator.Current as GrenadeClass).ThrowType == grenadeType)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        public bool CheckPeriodTime()
        {
            if (bool_0 && float_0 > 0f && float_0 < Time.time)
            {
                method_6(null);
                return true;
            }
            return false;
        }

        public void DoThrow()
        {
            if (BotOwner.WeaponManager != null && BotOwner.WeaponManager.Selector.IsChanging)
            {
                return;
            }
            if (BotOwner.WeaponManager.Reload.Reloading)
            {
                return;
            }
            if (!method_5())
            {
                return;
            }
            if (CheckPeriodTime())
            {
                return;
            }
            if (!BotOwner.GetPlayer.HealthController.IsAlive)
            {
                return;
            }
            switch (grenadeActionType_0)
            {
                case GrenadeActionType.ready:
                    bool_0 = true;
                    float_0 = Time.time + 3f;
                    grenadeActionType_0 = GrenadeActionType.change2grenade;
                    if (SavedGrenade == null)
                    {
                        method_6(null);
                        return;
                    }
                    if (AIGreanageThrowData.GrenadeType != null)
                    {
                        method_1(AIGreanageThrowData.GrenadeType.Value);
                    }
                    if (BotOwner.BotPersonalStats != null)
                    {
                        BotOwner.BotPersonalStats.GrendateThrow(null);
                    }
                    ThrowindNow = true;
                    action_1?.Invoke();
                    BotOwner.GetPlayer.SetInHandsForQuickUse(SavedGrenade, new Callback<GInterface122>(method_9));
                    return;
                case GrenadeActionType.change2grenade:
                    method_5();
                    return;
                case GrenadeActionType.grenadeReady:
                    method_5();
                    return;
                default:
                    return;
            }
        }

        public bool CanThrowGrenade(Vector3 trg)
        {
            return CanThrowGrenade(StartThrow, trg);
        }

        public bool CanThrowGrenade(Vector3 from, Vector3 trg)
        {
            if (!BotOwner.Settings.FileSettings.Grenade.CAN_LAY && BotOwner.BotLay.IsLay)
            {
                return false;
            }
            if (!BotOwner.BotsGroup.GroupGrenade.CanThrow())
            {
                return false;
            }
            if (BotOwner.AIData.PlaceInfo != null && BotOwner.AIData.PlaceInfo.BlockGrenade)
            {
                return false;
            }
            if (!BotOwner.Settings.FileSettings.Core.CanGrenade)
            {
                return false;
            }
            if (float_1 > Time.time)
            {
                return false;
            }
            method_7();
            if (!method_8(trg))
            {
                return false;
            }
            if (SAIN.Grenade.GetThrowType(out var direction, out Vector3 point) != GrenadeThrowType.None)
            {
                GClass516 gclass = GClass518.CanThrowGrenade2(from, point, MaxPower, AIGreandeAng.ang15, 1f, 0f);
                if (gclass.CanThrow)
                {
                    SetThrowData(gclass);
                    return true;
                }
            }
            return false;
        }

        public bool SetThrowData(GClass516 data)
        {
            AIGreanageThrowData = data;
            BotOwner.BotsGroup.GroupGrenade.ThrowGrenade(BotOwner);
            if (!data.CanThrow)
            {
                Debug.LogError("wrong throw data");
                return false;
            }
            return true;
        }

        private void method_0()
        {
            float grenadePrecision = BotOwner.Settings.FileSettings.Grenade.GrenadePrecision;
            if (grenadePrecision > 0f)
            {
                float x = GClass793.Random(-grenadePrecision, grenadePrecision);
                float y = GClass793.Random(-grenadePrecision, grenadePrecision);
                float z = GClass793.Random(-grenadePrecision, grenadePrecision);
                vector3_0 = new Vector3(x, y, z);
                return;
            }
            vector3_0 = Vector3.zero;
        }

        private void method_1(ThrowWeapType grenadeType)
        {
            foreach (Item item in SAIN.Equipment.Inventory.GetAllEquipmentItems().Where(new Func<Item, bool>(EFTBotGrenade.Class138.class138_0.method_1)))
            {
                GrenadeClass grenadeClass = item as GrenadeClass;
                if (grenadeClass.ThrowType == grenadeType)
                {
                    method_3(grenadeClass);
                    break;
                }
            }
        }

        private void method_2()
        {
            GrenadeClass grenadeClass = SAIN.Equipment.Inventory.GetAllEquipmentItems().OfType<GrenadeClass>().FirstOrDefault<GrenadeClass>();
            if (grenadeClass != null)
            {
                method_3(grenadeClass);
                return;
            }
            SavedGrenade = null;
        }

        private void method_3(GrenadeClass potentialGrenade)
        {
            SavedGrenade = potentialGrenade;
            Mass = SavedGrenade.Weight;
            Mass = 0.6f;
        }

        private bool method_4()
        {
            if (AIGreanageThrowData != null)
            {
                if (AIGreanageThrowData.Force < 0.01f)
                {
                    return true;
                }
                LastThrowDirection = AIGreanageThrowData.Direction + vector3_0;
                BotOwner.Steering.LookToDirection(LastThrowDirection, 500f);
                LastThrowDirection.Normalize();
                Vector3 vector = GClass792.NormalizeFastSelf(BotOwner.LookDirection);
                Vector3 vector2 = LastThrowDirection;
                float y = 0f;
                float y2 = 0f;
                vector2.y = y;
                vector.y = y2;
                vector = GClass792.NormalizeFastSelf(vector);
                vector2 = GClass792.NormalizeFastSelf(vector2);
                if (Mathf.Abs(vector.x - vector2.x) < 0.1f && Mathf.Abs(vector.z - vector2.z) < 0.1f)
                {
                    return true;
                }
            }
            return false;
        }

        private bool method_5()
        {
            return method_4();
        }

        private void method_6(GrenadeClass grenade = null)
        {
            ThrowindNow = false;
            method_0();
            float_0 = 0f;
            bool_0 = false;
            grenadeActionType_0 = GrenadeActionType.ready;
            AIGreanageThrowData.ThrowComplete = true;
            method_2();
            method_7();
            if (!BotOwner.WeaponManager.Selector.TakePrevWeapon())
            {
                BotOwner.AITaskManager.RegisterDelayedTask(2f, new Action(method_11));
            }
            BotOwner.SetYAngle(0f);
            if (action_0 != null && grenade != null)
            {
                action_0(grenade);
            }
        }

        private void method_7()
        {
            float_1 = BotOwner.Settings.FileSettings.Grenade.DELTA_NEXT_ATTEMPT + Time.time;
        }

        private bool method_8(Vector3 trg)
        {
            for (int i = 0; i < BotOwner.BotsGroup.MembersCount; i++)
            {
                if ((BotOwner.BotsGroup.Member(i).Transform.position - trg).sqrMagnitude < BotOwner.Settings.FileSettings.Grenade.MIN_DIST_NOT_TO_THROW_SQR)
                {
                    return false;
                }
            }
            return true;
        }

        public void Dispose()
        {
        }

        [CompilerGenerated]
        private void method_9(Result<GInterface122> result)
        {
            float_0 = Time.time + 3f;
            if (result.Value != null)
            {
                result.Value.SetOnUsedCallback(new Callback<GInterface121<GrenadeClass>>(method_10));
            }
            else
            {
                Debug.LogError("Quick use result.Value is null");
                method_6(null);
                SavedGrenade = null;
            }
            grenadeActionType_0 = GrenadeActionType.grenadeReady;
        }

        [CompilerGenerated]
        private void method_10(Result<GInterface121<GrenadeClass>> result1)
        {
            method_6(SavedGrenade);
        }

        [CompilerGenerated]
        private void method_11()
        {
            BotOwner.WeaponManager.Selector.TakePrevWeapon();
        }

        public Vector3 LastThrowDirection;
        public float MaxThrowForce = 20f;
        private GrenadeClass SavedGrenade;
        private GrenadeActionType grenadeActionType_0;
        private float float_0;
        private bool bool_0;
        private float float_1;
        private readonly float float_2;
        private Vector3 vector3_0;
        [CompilerGenerated]
        private Action<GrenadeClass> action_0;
        [CompilerGenerated]
        private Action action_1;
        [CompilerGenerated]
        [Serializable]
        private sealed class Class138
        {
            internal bool method_0(Item x)
            {
                return x is GrenadeClass;
            }

            internal bool method_1(Item x)
            {
                return x is GrenadeClass;
            }

            public static readonly EFTBotGrenade.Class138 class138_0 = new EFTBotGrenade.Class138();
        }
    }
}
