using Aki.Reflection.Patching;
using EFT;
using HarmonyLib;
using SAIN.Classes;
using SAIN.Components;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.Patches
{
    public class VisiblePatch : ModulePatch
    {
        private static PropertyInfo _GoalEnemy;
        private static PropertyInfo _SeenCoef;
        private static PropertyInfo _Direction;
        private static PropertyInfo _Distance;
        private static PropertyInfo _gclass551_0;
        private static PropertyInfo _VisibleOnlyBySense;
        private static PropertyInfo _PersonalLastShootTime;
        private static PropertyInfo _PersonalLastSeenTime;
        private static PropertyInfo _PersonalLastPos;

        protected override MethodBase GetTargetMethod()
        {
            _GoalEnemy = AccessTools.Property(typeof(BotMemoryClass), "GoalEnemy");
            _SeenCoef = AccessTools.Property(_GoalEnemy.PropertyType, "SeenCoef");
            _Direction = AccessTools.Property(_GoalEnemy.PropertyType, "Direction");
            _Distance = AccessTools.Property(_GoalEnemy.PropertyType, "Distance");
            _gclass551_0 = AccessTools.Property(_GoalEnemy.PropertyType, "gclass551_0");
            _VisibleOnlyBySense = AccessTools.Property(_GoalEnemy.PropertyType, "VisibleOnlyBySense");
            _PersonalLastShootTime = AccessTools.Property(_GoalEnemy.PropertyType, "PersonalLastShootTime");
            _PersonalLastSeenTime = AccessTools.Property(_GoalEnemy.PropertyType, "PersonalLastSeenTime");
            _PersonalLastPos = AccessTools.Property(_GoalEnemy.PropertyType, "PersonalLastPos");

            return AccessTools.Method(_GoalEnemy.PropertyType, "CheckLookEnemy");
        }

        [PatchPrefix]
        public static void Prefix(ref BotOwner ___botOwner_0, IAIDetails person)
        {
        }

        public bool CheckLookEnemy(ref BotOwner ___botOwner_0, IAIDetails person, GClass552 lookAll)
        {
            BotOwner BotOwner = ___botOwner_0;
            if (!SAINPlugin.BotController.GetBot(BotOwner.ProfileId, out var sain))
            {
                return true;
            }
            float minValue = float.MinValue;
            if (person == null || person.Transform == null || person.Transform.Original == null)
            {
                return false;
            }
            var goalEnemy = BotOwner.Memory.GoalEnemy;
            lookAll.distCheck = true;
            BotOwner owner = BotOwner;
            SAINEnemy sainEnemy = sain.Enemies[person.ProfileId];

            float seenCoef = sain.Vision.GetSeenCoef(person.Transform, person.AIData, goalEnemy.PersonalLastSeenTime, goalEnemy.PersonalLastPos);
            _SeenCoef.SetValue(goalEnemy, seenCoef);
            IAIDetails person2 = goalEnemy?.Person;
            Vector3 direction = goalEnemy.CurrPosition - owner.Transform.position;
            _Direction.SetValue(goalEnemy, direction);
            float distance = direction.magnitude;
            _Distance.SetValue(goalEnemy, distance);
            if (distance < lookAll.minDistance)
            {
                lookAll.minDistance = distance;
            }
            goalEnemy.UpdatePartsByPriority();
            Dictionary<BodyPartClass, GClass478> allActiveParts = goalEnemy.AllActiveParts;
            float addVisibility = 1f;
            if (person.AIData.UsingLight && BotOwner.LookSensor.VisibleDist < owner.Settings.FileSettings.Look.ENEMY_LIGHT_START_DIST)
            {
                addVisibility = owner.Settings.FileSettings.Look.ENEMY_LIGHT_ADD;
            }
            bool IsVisible = sainEnemy.InLineOfSight;
            if (owner.FlashGrenade.IsFlashed)
            {
                IsVisible = false;
                addVisibility = -1f;
            }
            var gClass = (GClass551)_gclass551_0.GetValue(goalEnemy);
            gClass.IsVisible = false;
            gClass.VisibleType = EEnemyPartVisibleType.notVisible;
            gClass.CanShoot = false;
            //GClass551 visionCheck = CheckVisibilityPart(keyValuePair_0, ref minValue, onSense, onSenceGreen, addVisibility);
            //method_6(visionCheck, gClass);
            //if (_checkHeadWithBody)
            //{
            //GClass551 visionCheck2 = CheckVisibilityPart(keyValuePair_1, ref minValue, onSense, onSenceGreen, addVisibility);
            //method_6(visionCheck2, gclass551_0);
            //}
            if (allActiveParts != null)
            {
                foreach (KeyValuePair<BodyPartClass, GClass478> enemyPart in allActiveParts)
                {
                    //visionCheck = CheckVisibilityPart(enemyPart, ref minValue, onSense, onSenceGreen, addVisibility);
                    //method_6(visionCheck, gclass551_0);
                }
            }
            goalEnemy.SetCanShoot(sainEnemy.CanShoot);
            if (sainEnemy.CanShoot)
            {
                _PersonalLastShootTime.SetValue(goalEnemy, Time.time);
            }
            bool isVisible = IsVisible;
            goalEnemy.SetVisible(IsVisible);
            EEnemyPartVisibleType VisibleType = IsVisible ? EEnemyPartVisibleType.visible : EEnemyPartVisibleType.notVisible;
            goalEnemy.SetVisibleParam(VisibleType);
            if (IsVisible)
            {
                goalEnemy.PrevIsVisible(true);
                _PersonalLastSeenTime.SetValue(goalEnemy, Time.time);
                _PersonalLastPos.SetValue(goalEnemy, goalEnemy.CurrPosition);

                goalEnemy.SetVisibleParam(VisibleType);
                GClass522 item = new GClass522(owner, person2, person2.Transform.position, VisibleType);
                lookAll.reportsData.Add(item);
                if (!isVisible)
                {
                    lookAll.shallRecalcGoal = true;
                    return false;
                }
            }
            else
            {
                goalEnemy.PrevIsVisible(false);
            }
            return false;
        }
    }
}