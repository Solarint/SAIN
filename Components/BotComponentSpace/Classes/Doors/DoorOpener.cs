using EFT;
using EFT.Interactive;
using SAIN.Components;
using SAIN.Helpers;
using SAIN.Preset.GlobalSettings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class DoorOpener : BotBase, IBotClass
    {
        public bool Interacting
        {
            get
            {
                return BotOwner.DoorOpener.Interacting;
            }
            private set
            {
                BotOwner.DoorOpener.Interacting = value;
            }
        }

        public bool NearDoor
        {
            get
            {
                return BotOwner.DoorOpener.NearDoor;
            }
            private set
            {
                BotOwner.DoorOpener.NearDoor = value;
            }
        }

        public bool BreachingDoor { get; private set; }

        public DoorFinder DoorFinder { get; }

        public DoorOpener(BotComponent sain) : base(sain)
        {
            DoorFinder = new DoorFinder(this);
        }

        public void Init()
        {
            DoorFinder.Init();
        }

        public void Update()
        {
            DoorFinder.Update();
            if (BotOwner.Mover.IsMoving || Bot.Mover.SprintController.Running)
            {
                CheckUseSAINOpener();
            }
            if (!_debugMode && _debugObjects.Count > 0)
            {
                foreach (var obj in _debugObjects.Values)
                {
                    GameObject.Destroy(obj.link);
                    GameObject.Destroy(obj.midClose);
                    GameObject.Destroy(obj.midOpen);
                }
                _debugObjects.Clear();
            }
        }

        public void Dispose()
        {
            DoorFinder.Dispose();
        }

        public bool CheckUseSAINOpener()
        {
            if (!SAINPlugin.LoadedPreset.GlobalSettings.General.Doors.NewDoorOpening)
            {
                return BotOwner.DoorOpener.Update();
            }
            if (!Bot.BotActivation.SAINLayersActive)
            {
                return BotOwner.DoorOpener.Update();
            }

            return FindDoorsToOpen();
        }

        private void checkEndDoorOpening()
        {
            //this.BotOwner.Steering.SetYAngle(0f);
            if (this._traversingEnd < Time.time || 
                (_lastInteractedInfo != null && _lastInteractedInfo.Door.DoorState != EDoorState.Interacting))
            {
                endDoorInteraction();
            }
        }

        public bool FindDoorsToOpen()
        {
            if (Interacting)
            {
                checkEndDoorOpening();
                return true;
            }
            if (_nextPosibleDoorInteractTime < Time.time)
            {
                checkIfLastDoorExpire();
                _interactingWithDoor = findADoorToOpen();
                NearDoor = _interactingWithDoor;
            }
            return this._interactingWithDoor;
        }

        private void drawLink(NavMeshDoorLink link)
        {
            if (_debugMode &&
                SAINPlugin.DebugSettings.Gizmos.DrawDebugGizmos &&
                !_debugObjects.ContainsKey(link))
            {
                Vector3 linkPosition = link.transform.position + Vector3.down;
                var objects = new linkObjects
                {
                    link = DebugGizmos.Line(linkPosition, linkPosition + Vector3.up * 2f, Color.white, 0.2f, false, -1f),
                    midOpen = DebugGizmos.Line(linkPosition, link.MidOpen + Vector3.down, Color.blue, 0.2f, false, -1f),
                    midClose = DebugGizmos.Line(linkPosition, link.MidClose + Vector3.down, Color.green, 0.2f, false, -1f),
                };
                _debugObjects.Add(link, objects);
            }
        }

        private struct linkObjects
        {
            public GameObject link;
            public GameObject midOpen;
            public GameObject midClose;
        }

        private bool canInteract(NavMeshDoorLink link)
        {
            if (!link.ShallInteract())
            {
                //Logger.LogDebug($"Link {link.Id} shall not interact!");
                return false;
            }
            if (!link.Door.enabled || !link.Door.gameObject.activeInHierarchy)
            {
                return false;
            }
            if (checkIfDoorLast(link))
            {
                //Logger.LogDebug($"Link {link.Id} is last!");
                return false;
            }
            if (!link.Door.Operatable || !link.Door.enabled)
            {
                //Logger.LogDebug($"Link {link.Id} door not operable!");
                return false;
            }
            return true;
        }

        private bool findADoorToOpen()
        {
            List<DoorData> list = DoorFinder.InteractionDoors;
            if (list.Count == 0)
            {
                return false;
            }

            _nextPosibleDoorInteractTime = Time.time + DOOR_CHECK_FREQ;

            findPossibleInteractDoors(list);
            var interactDoors = _possibleInteractDoors;
            if (interactDoors.Count == 0)
            {
                return false;
            }

            DoorData selectedDoor = checkWantToOpenAnyDoors(interactDoors) ?? checkWantToCloseAnyDoors(interactDoors);

            if (selectedDoor == null)
            {
                return false;
            }
            return interactWithDoor(selectedDoor);
        }

        private bool interactWithDoor(DoorData data)
        {
            data.LastInteractTime = Time.time;
            NavMeshDoorLink link = data.Link;
            Door door = link.Door;

            //Logger.LogDebug($"Found Door to interact for {BotOwner.name}! ID: {link.Id}");

            switch (door.DoorState)
            {
                case EDoorState.Shut:
                    data.LastOpenTime = Time.time;
                    //Logger.LogDebug($"{BotOwner.name} opening door...");
                    _nextPosibleDoorInteractTime = Time.time + 1f;
                    Interact(data, EInteractionType.Open);
                    return true;

                case EDoorState.Open:
                    data.LastCloseTime = Time.time;
                    //Logger.LogDebug($"{BotOwner.name} closing door...");
                    _nextPosibleDoorInteractTime = Time.time + 1f;
                    Interact(data, EInteractionType.Close);
                    return true;

                default:
                    return false;
            }
        }

        private DoorData checkWantToOpenAnyDoors(List<DoorData> doors)
        {
            float highestDot = -1f;
            DoorData selectedDoor = null;
            foreach (var data in doors)
            {
                if (!data.DoorInFront)
                    continue;

                NavMeshDoorLink link = data.Link;
                Door door = link.Door;

                if (door.DoorState != EDoorState.Shut)
                    continue;

                if (data.DotProduct > highestDot)
                {
                    highestDot = data.DotProduct;
                    selectedDoor = data;
                }
            }
            if (selectedDoor != null && selectedDoor.DotProduct > 0f)
            {
                return selectedDoor;
            }
            return null;
        }

        private DoorData checkWantToCloseAnyDoors(List<DoorData> doors)
        {
            //float lowestDot = 0f;
            DoorData selectedDoor = null;
            foreach (var data in doors)
            {
                //if (data.DoorInFront)
                //    continue;

                NavMeshDoorLink link = data.Link;
                Door door = link.Door;

                if (door.DoorState != EDoorState.Open)
                    continue;

                selectedDoor = data;
                break;
            }
            return selectedDoor;
        }

        private void findPossibleInteractDoors(List<DoorData> list)
        {
            _possibleInteractDoors.Clear();

            Vector3 targetMovePos;
            // This is the same as the old call (and was previously the same backing field)
            if (BotOwner.Mover.HasPathAndNoComplete)
                targetMovePos = BotOwner.Mover.RealDestPoint;
            else if (Bot.Mover.SprintController.Running)
                targetMovePos = Bot.Mover.SprintController.CurrentCornerDestination();
            else return;

            Vector3 botPos = BotOwner.Transform.position;
            Vector3 moveDirection = (targetMovePos - botPos).normalized;

            foreach (var data in list)
            {
                //Logger.LogDebug($"Checking {link.Id}...");

                NavMeshDoorLink link = data.Link;
                if (!canInteract(link))
                    continue;

                if (!data.CanInteractByTime())
                    continue;

                //Logger.LogDebug($"Door {link.Id} can be interacted with. Checking if {BotOwner.name} wants to open it...");

                Door door = data.Door;
                drawLink(link);
                float maxDistance;

                switch (door.DoorState)
                {
                    case EDoorState.Open:
                        maxDistance = 4f;
                        break;

                    case EDoorState.Shut:
                        maxDistance = 4f;
                        break;

                    default:
                        continue;
                }

                //Logger.LogDebug($"Door {link.Id} is either open or closed.");

                data.CalcDirection(botPos);

                if (data.CurrentSqrMagnitude > maxDistance)
                    continue;

                data.DotProduct = Vector3.Dot(data.DirectionNormal, moveDirection);

                if (!CheckWantToInteract(data, botPos))
                    continue;

                _possibleInteractDoors.Add(data);
            }
        }

        private readonly List<DoorData> _possibleInteractDoors = new List<DoorData>();

        private void checkIfLastDoorExpire()
        {
            DoorData lastInfo = _lastInteractedInfo;
            if (_lastInteractedInfo == null)
            {
                return;
            }
            if (lastInfo.Door.DoorState == EDoorState.Interacting)
            {
                lastInfo.LastInteractTime = Time.time;
                return;
            }
            if (lastInfo.LastInteractTime + DOOR_SINGLE_INTERACTION_FREQ < Time.time)
            {
                _lastInteractedInfo = null;
            }
        }

        private const float DOOR_SINGLE_INTERACTION_FREQ = 1f;
        private const float DOOR_INTERACTION_FREQ = 0.66f;
        private const float DOOR_CHECK_FREQ = 0.25f;

        private bool checkIfDoorLast(NavMeshDoorLink link)
        {
            DoorData lastInfo = _lastInteractedInfo;
            if (lastInfo == null)
            {
                return false;
            }
            if (lastInfo.Link.Id != link.Id)
            {
                return false;
            }
            return lastInfo.CanInteractByTime();
        }

        private Vector3 getMovePoint()
        {
            Vector3 targetDest;
            if (Bot.Mover.SprintController.Running)
                targetDest = Bot.Mover.SprintController.CurrentCornerDestination();
            else
                targetDest = BotOwner.Mover.RealDestPoint;
            return targetDest;
        }

        private void doDefaultInteract(Door door, EInteractionType Etype)
        {
            //BotOwner.GetPlayer.CurrentManagedState.StartDoorInteraction(door, new InteractionResult(Etype), new Action(endDoorInteraction));
            method_0(door, Etype);
        }

        private void method_0(Door door, EInteractionType type)
        {
            Player.MovementContext.ResetCanUsePropState();
            var gstruct = Door.Interact(Player, type);
            if (gstruct.Succeeded)
            {
                //Logger.LogDebug("Success");
                switch (type)
                {
                    case EInteractionType.Breach:
                        Player.vmethod_0(door, gstruct.Value, new Action(endDoorInteraction));
                        break;

                    default:
                        Player.vmethod_1(door, gstruct.Value);
                        break;
                }
                return;
            }
            //Logger.LogDebug("Fail");
        }

        private void endDoorInteraction()
        {
            NearDoor = false;
            BreachingDoor = false;
            Interacting = false;
            if (!Bot.Mover.SprintController.Running)
            {
                BotOwner.Mover.MovementResume();
                BotOwner.Mover.SprintPause(-1f);
            }
        }

        public bool ShallPauseSprintForOpening()
        {
            if (!Interacting)
            {
                return false;
            }

            var general = GlobalSettingsClass.Instance.General.Doors;
            return
                general.NoDoorAnimations == false ||
                BreachingDoor == true ||
                general.NewDoorOpening == false;
        }

        private bool shallKickOpen(Door door, EInteractionType Etype)
        {
            if (Etype != EInteractionType.Open)
            {
                return false;
            }
            if (!wantToKick())
            {
                return false;
            }
            var breakInParameters = door.GetBreakInParameters(Bot.Position);
            return door.BreachSuccessRoll(breakInParameters.InteractionPosition);
        }

        private bool wantToKick()
        {
            var enemy = Bot.Enemy;
            if (enemy != null)
            {
                if (Bot.Info.PersonalitySettings.General.KickOpenAllDoors)
                {
                    return true;
                }
                if (BotOwner.Memory.IsUnderFire)
                {
                    return true;
                }
                float timeSinceSeen = enemy.TimeSinceSeen;
                if (timeSinceSeen < 3f)
                {
                    return true;
                }
                if (timeSinceSeen < 5f && enemy.InLineOfSight)
                {
                    return true;
                }
            }
            return false;
        }

        public void Interact(DoorData doorInfo, EInteractionType Etype)
        {
            doorInfo.LastInteractTime = Time.time;
            _lastInteractedInfo = doorInfo;

            Door door = doorInfo.Door;
            //bool noAnimation = door.interactWithoutAnimation;
            EDoorState snap = door.Snap;
            if (shallKickOpen(door, Etype) || Etype == EInteractionType.Breach)
            {
                //Logger.LogDebug($"{BotOwner.name} Breaching Door {doorInfo.Link.Id}!");
                BreachingDoor = true;
                Etype = EInteractionType.Breach;
            }
            else
            {
                BreachingDoor = false;
                //door.interactWithoutAnimation = true;
                door.Snap = EDoorState.None;
            }

            if (Etype == EInteractionType.Breach ||
                ModDetection.ProjectFikaLoaded ||
                !GlobalSettingsClass.Instance.General.Doors.NoDoorAnimations)
            {
                //Logger.LogDebug($"{BotOwner.name} Executing [{Etype}] door interaction on {doorInfo.Link.Id}");
                this.BotOwner.Mover.SprintPause(2f);
                this.BotOwner.Mover.MovementPause(2f);
                Interacting = true;
                _traversingEnd = Time.time + (BreachingDoor ? 2f : 1f);
                doDefaultInteract(door, Etype);
                Bot.Steering.LookToPoint(door.transform.position + (door.transform.position - Bot.Position));
                //door.interactWithoutAnimation = noAnimation;
                return;
            }

            //Logger.LogDebug($"{BotOwner.name} Auto Opening Door on {doorInfo.Link.Id}");

            EDoorState state = EDoorState.None;
            switch (Etype)
            {
                case EInteractionType.Open:
                    state = EDoorState.Open;
                    break;

                case EInteractionType.Close:
                    state = EDoorState.Shut;
                    break;

                default:
                    Logger.LogError($"Door open type set wrong! {Etype}");
                    return;
            }

            bool shallInvert = ShallInvertDoorAngle(door);
            GameWorldComponent.Instance.Doors.ChangeDoorState(door, state, shallInvert);
            SAINBotController.Instance.BotHearing.PlayAISound(PlayerComponent, SAINSoundType.Door, door.transform.position, 30f, 1f, true);
        }

        private bool ShallInvertDoorAngle(Door door)
        {
			if (!GlobalSettingsClass.Instance.General.Doors.InvertDoors)
			{
				return false;
			}
			var interactionParameters = door.GetInteractionParameters(BotOwner.Position);
			if (interactionParameters.AnimationId == (door.DoorState is EDoorState.Locked ? (int)door.DoorKeyOpenInteraction : door.CalculateInteractionIndex(BotOwner.Position)))
			{
				return false;
			}
			return true;
		}

        // Token: 0x060010AF RID: 4271 RVA: 0x0004CED4 File Offset: 0x0004B0D4
        public bool CheckWantToInteract(DoorData data, Vector3 botPosition)
        {
            NavMeshDoorLink link = data.Link;
            botPosition += Vector3.up;
            if (Mathf.Abs(botPosition.y - link.Open1.y) >= 0.5f)
            {
                return false;
            }

            switch (data.Door.DoorState)
            {
                case EDoorState.Open:
                    return data.DotProduct < 0;

                case EDoorState.Shut:
                    return data.DotProduct > 0f;

                default:
                    return false;
            }
        }

        private bool checkCrossPoint(Vector3 goTo, Vector3 botPosition, DoorData data)
        {
            NavMeshDoorLink link = data.Link;
			GClass355 gclass;
            switch (link.Door.DoorState)
            {
                case EDoorState.Open:
                    //if ((link.MidClose - vector).sqrMagnitude > 4)
                    //    return false;

                    gclass = link.SegmentClose;
                    break;

                case EDoorState.Shut:
                    //if ((link.MidOpen - vector).sqrMagnitude > 4)
                    //    return false;

                    gclass = link.SegmentOpen;
                    break;

                default:
                    return false;
            }

            Vector3 v = botPosition - goTo;
            v.y = 0f;
            Vector3 a = Vector.NormalizeFastSelf(v);
            Vector3 b = a * 0.25f;
            Vector3 b2 = a * 0.5f;
            Vector3 a2 = botPosition + b;
            Vector3 b3 = goTo - b2;
            Vector3? crossPoint = Vector.GetCrossPoint(new Vector.VectorPair(a2, b3), new Vector.VectorPair(gclass.a, gclass.b));
            return crossPoint != null;
        }

        private static bool _debugMode => SAINPlugin.DebugSettings.Gizmos.DrawDoorLinks;
        private readonly List<NavMeshDoorLink> _doorsOnPath = new List<NavMeshDoorLink>();
        private DoorData _lastInteractedInfo;
        private static readonly Dictionary<NavMeshDoorLink, linkObjects> _debugObjects = new Dictionary<NavMeshDoorLink, linkObjects>();
        public bool _interactingWithDoor;
        private float _nextPosibleDoorInteractTime;
        private float _traversingEnd;
        private float _refreshWayPeriod;
    }
}