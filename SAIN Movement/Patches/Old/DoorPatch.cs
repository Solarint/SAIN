//using static Config.Door;
/*
namespace Movement.Patches
{
    public class DoorPatch : ModulePatch
    {
        protected override MethodBase GetTargetMethod()
        {
            return typeof(BotOwner)?.GetProperty("DoorOpener")?.PropertyType?.GetMethod("method_2", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
        }
        [PatchPrefix]
        public static bool PatchPrefix(ref BotOwner ___botOwner_0, ref float ___float_7, ref Door ___door_0, ref bool ___bool_0)
        {
            if (DoorPatchToggle.Value == false)
            {
                return true;
            }
            float WaitTime = 0.0f;
            bool CanOpenDoorNow = WaitTime > Time.time;
            float distance2Enemy = (___botOwner_0.AimingData.RealTargetPoint - ___botOwner_0.Transform.position).magnitude;
            float time = Time.time;
            float num = time - ___float_7;

            ___botOwner_0.Mover.SprintPause(DoorWalkPause.Value);
            ___botOwner_0.MovementPause(DoorWalkPause.Value);

            if (num > DoorTimer.Value)
            {
                ___float_7 = time;
                if (DebugDoor.Value == true)
                {
                    Logger.LogInfo($"Cant open that door yet! [{___botOwner_0.name},{num}");
                }
                return false;
            }
            if (!___door_0.Operatable || CanOpenDoorNow)
            {
                return false;
            }
            if (___door_0.DoorState == EDoorState.Interacting)
            {
                WaitTime = Time.time + DoorOpenDelay.Value;
                return false;
            }
            ___bool_0 = false;
            if (___door_0.DoorState != EDoorState.Shut)
            {
                if (___door_0.DoorState == EDoorState.Open)
                {
                    WaitTime = Time.time + DoorOpenDelay.Value;
                    ___botOwner_0.DoorOpener.Interact(___door_0, EInteractionType.Close);
                    if (DebugDoor.Value == true)
                    {
                        Logger.LogInfo($"Closing Door! [{___botOwner_0.name},{___door_0.DoorState},{WaitTime}");
                    }
                }
                return false;
            }
            bool flag;
            if (flag = distance2Enemy < DoorBreachDistance.Value)
            {
                WorldInteractiveObject.GStruct300 breakInParameters = ___door_0.GetBreakInParameters(___botOwner_0.Transform.position);
                flag = ___door_0.BreachSuccessRoll(breakInParameters.InteractionPosition);
            }
            if (flag)
            {
                WaitTime = Time.time + DoorOpenDelay.Value;
                ___botOwner_0.DoorOpener.Interact(___door_0, EInteractionType.Breach);
                if (DebugDoor.Value == true)
                {
                    Logger.LogInfo($"{___botOwner_0.name} BREACHING THE DOOR! {WaitTime}");
                }
                return false;
            }
            WaitTime = Time.time + DoorOpenDelay.Value;

            //MethodInfo targetMethod = AccessTools.Method(typeof(WorldInteractiveObject), "SmoothDoorOpenCoroutine ");
            //___door_0.KickOpen(true);
            //___door_0.SmoothDoorOpenCoroutine(EInteractionType.Open, true, 1f);

            //MethodInfo targetMethod = AccessTools.Method(typeof(Door), "Interact");
            //targetMethod.Invoke(___door_0, new object[] { EInteractionType.Open });
            //___door_0.LockForInteraction();

            //if (Singleton<GClass627>.Instantiated)
            //{
            //	Singleton<GClass627>.Instance.PlaySound(___botOwner_0, ___botOwner_0.Transform.position, GClass558.Core.SOUND_DOOR_OPEN_METERS, AISoundType.step);
            //}
            //Player Player = ___botOwner_0.GetPlayer;
            //GClass2596 interactionResult = new GClass2596(EInteractionType.Open);
            //Player.CurrentState.StartDoorInteraction(___door_0, interactionResult, null);
            //PlayerAnimatorSetInteraction(true, approached);

            //ORIGINAL
            ___botOwner_0.DoorOpener.Interact(___door_0, EInteractionType.Open);
            if (DebugDoor.Value == true)
            {
                Logger.LogInfo($"{___botOwner_0.name} OPENING THE DOOR! {WaitTime}");
            }
            return false;
        }
    }
    /*public class DoorPatch2 : ModulePatch
    {
        protected override MethodBase GetTargetMethod() // Target the "Shoot" method of the class assigned to "ShootData" in the "BotOwner" class
            {
                return typeof(WorldInteractiveObject)?.GetMethod("GetInteractionParameters", BindingFlags.Instance | BindingFlags.Public);
            }
    
        [PatchPostfix] // Gives us access to the member variable "botOwner_0" in GClass541 that would normally be private
        public static void PatchPostfix(ref WorldInteractiveObject.GStruct300 __result)
        {
    	    //Vector3 viewDirection = ___door_0.GetViewDirection(yourPosition);
    	    //Vector3 vector = WorldInteractiveObject.GetRotationAxis(___door_0.DoorForward, Component.transform) * 0.5f + Component.transform.position;
    	    //viewDirection.x = vector.x;
    	    //viewDirection.z = vector.z;
          Logger.LogInfo($"Door Parameters Updated");
    	    __result.Grip = null;
    	    __result.AnimationId = 0;
    	    //__result.ViewTarget = viewDirection;
            __result.Snap = false;
            __result.RotationMode = WorldInteractiveObject.ERotationInterpolationMode.ViewTargetWithZeroPitch;
        }
    }
}*/
