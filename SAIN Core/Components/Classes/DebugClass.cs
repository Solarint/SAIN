using EFT;
using SAIN.Components;
using SAIN.Helpers;
using SAIN_Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

namespace SAIN.Classes
{
    public class DebugClass
    {
        public static string Reason(bool canSee)
        {
            string reason;
            if (canSee)
            {
                reason = "Can See";
            }
            else
            {
                reason = "Can't See";
            }
            return reason;
        }

        public static string Reason(EnemyClass enemy)
        {
            string reason;
            if (enemy.Path.RangeFar)
            {
                reason = "Far";
            }
            else if (enemy.Path.RangeMid)
            {
                reason = "MidRange";
            }
            else if (enemy.Path.RangeClose)
            {
                reason = "Close";
            }
            else
            {
                reason = "Very Close";
            }
            return reason;
        }

        public static string Reason(StatusClass status)
        {
            string reason;
            if (status.Injured)
            {
                reason = "Injured";
            }
            else if (status.BadlyInjured)
            {
                reason = "Badly Injured";
            }
            else if (status.Dying)
            {
                reason = "Dying";
            }
            else
            {
                reason = "Healthy";
            }
            return reason;
        }

        public static string Reason(MedicalClass medical)
        {
            string reason = "Some Reason";

            if (medical.CanHeal)
            {
                reason = "Can Heal";
            }
            if (medical.HasStims)
            {
                reason += " and Have Stims";
            }
            if (medical.Bleeding)
            {
                reason += " and Bleeding";
            }
            if (medical.HasFirstAid)
            {
                reason += " and Have First Aid";
            }

            return reason;
        }
    }
}