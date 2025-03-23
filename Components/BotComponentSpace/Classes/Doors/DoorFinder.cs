using EFT;
using EFT.Interactive;
using SAIN.Components;
using SAIN.Helpers;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Mover
{
    public class DoorFinder : BotSubClass<DoorOpener>, IBotClass
    {
        static DoorFinder()
        {
            //debugFindDoors();
        }

        private const float DOORS_UPDATE_FREQ = 0.5f;
        private const float DOORS_FIND_CLOSE_FREQ = 2f;
        private const float DOORS_CLOSE_DISTANCE = 400f;
        private const float DOORS_INTERACTION_DISTANCE = 64f;
        private const float DOORS_FIND_INTERACTION_FREQ = 1f;
        private const float DOORS_UPDATE_VOXEL_FREQ = 0.5f;

        public event Action<NavGraphVoxelSimple, NavGraphVoxelSimple> OnNewVoxel;
        public event Action<List<DoorData>> OnNewCloseDoorsFound;
        public event Action<List<DoorData>> OnNewInteractionDoorsFound;

        public List<DoorData> InteractionDoors { get; } = new List<DoorData>();
        public List<DoorData> CloseDoors { get; } = new List<DoorData>();
        public List<DoorData> AllDoors { get; } = new List<DoorData>();
        public NavGraphVoxelSimple CurrentVoxel { get; private set; }

        public DoorFinder(DoorOpener opener) : base(opener) { }

        public void Init()
        {
            //Bot.Mover.OnNewGoToPoint += newMove;
            //Bot.Mover.SprintController.OnNewSprint += newMove;
            //Bot.Mover.SprintController.OnNewCornerMoveTo += newMove;
        }

        public void Update()
        {
            updateVoxel();
            updateCurrentDoors();
        }

        private void newMove(Vector3 currentCorner, Vector3 destination)
        {
            findDotProducts(Bot.Position, currentCorner);
        }

        private static void debugFindDoors()
        {
            var doors = Physics.OverlapSphere(Vector3.zero, 1000f, LayerMaskClass.DoorLayer);
            Logger.LogDebug($"Found {doors.Length} total doors");
            foreach (var door in doors)
            {
                DebugGizmos.Sphere(door.transform.position, -1f);
            }
        }

        public void Dispose()
        {
            //Bot.Mover.OnNewGoToPoint -= newMove;
            //Bot.Mover.SprintController.OnNewSprint -= newMove;
            //Bot.Mover.SprintController.OnNewCornerMoveTo -= newMove;
        }

        private void updateCurrentDoors()
        {
            if (_nextUpdateDoorTime < Time.time)
            {
                _nextUpdateDoorTime = Time.time + DOORS_UPDATE_FREQ;
                updateAllDoors(false);
            }
        }

        private void updateVoxel()
        {
            float time = Time.time;
            if (_nextUpdateVoxelTime < time && _moving)
            {
                _nextUpdateVoxelTime = time + DOORS_UPDATE_VOXEL_FREQ;
                BotOwner.AIData.SetPosToVoxel(Bot.Position);

                var lastVoxel = CurrentVoxel;
                CurrentVoxel = BotOwner.VoxelesPersonalData.CurVoxel;

                if (lastVoxel != CurrentVoxel)
                {
                    findAllDoors(CurrentVoxel);
                    OnNewVoxel?.Invoke(CurrentVoxel, lastVoxel);
                }
            }
        }

        private void findAllDoors(NavGraphVoxelSimple voxel)
        {
            AllDoors.Clear();
            if (voxel != null)
            {
                _nextUpdateDoorTime = Time.time + DOORS_UPDATE_FREQ;
                _nextCheckDistanceTime = Time.time + DOORS_FIND_CLOSE_FREQ;

                foreach (var link in voxel.DoorLinks)
                    if (isDoorOpenable(link.Door)) {
                        AllDoors.Add(new DoorData(link));
                        GameWorldComponent.Instance.Doors.CheckAddDoorTrigger(link);
                    }

                updateAllDoors(true);
            }
        }

        private bool isDoorOpenable(Door door)
        {
            if (!door.enabled || 
                !door.gameObject.activeInHierarchy ||
                !door.Operatable)
            {
                return false;
            }
            if (GlobalSettings.General.Doors.DisableAllDoors && 
                GameWorldComponent.Instance.Doors.DisableDoor(door))
            {
                return false;
            }
            return true;
        }

        private void updateAllDoors(bool force)
        {
            Vector3 botPosition = Bot.Position;

            foreach (var door in AllDoors)
                door.CalcDirection(botPosition);

            findCloseDoors(force);
            findDoorsToInteract(botPosition, force);
        }


        private void findDoorsToInteract(Vector3 botPosition, bool force)
        {
            if (force || _nextUpdateInteractTime < Time.time)
            {
                _nextUpdateInteractTime = Time.time + DOORS_FIND_INTERACTION_FREQ;

                findDoorsInRange(DOORS_INTERACTION_DISTANCE, CloseDoors, InteractionDoors);
                //
                //Vector3 targetMovePos;
                //if (BotOwner.Mover.HasPathAndNoComplete)
                //    targetMovePos = BotOwner.Mover.RealDestPoint;
                //else if (Bot.Mover.SprintController.Running)
                //    targetMovePos = Bot.Mover.SprintController.CurrentCornerDestination();
                //else return;
                //
                //findDotProducts(botPosition, targetMovePos);
            }
        }

        private void findDotProducts(Vector3 botPosition, Vector3 currentCornerDestination)
        {
            Vector3 moveDirection = (currentCornerDestination - botPosition).normalized;
            foreach (var door in InteractionDoors)
            {
                door.CalcDirection(botPosition);
                door.DotProduct = Vector3.Dot(door.DirectionNormal, moveDirection);
            }
        }

        private void findCloseDoors(bool force)
        {
            if (force || _nextCheckDistanceTime < Time.time)
            {
                findDoorsInRange(DOORS_CLOSE_DISTANCE, AllDoors, CloseDoors);
                OnNewCloseDoorsFound?.Invoke(CloseDoors);
            }
        }

        private static void findDoorsInRange(float range, List<DoorData> doorsToCheck, List<DoorData> result)
        {
            result.Clear();
            foreach (var door in doorsToCheck)
                if (door.CurrentSqrMagnitude <= range)
                    result.Add(door);
        }

        private bool _moving => BotOwner.Mover.IsMoving || Bot.Mover.SprintController.Running;
        private float _nextUpdateDoorTime;
        private float _nextCheckDistanceTime;
        private float _nextUpdateVoxelTime;
        private float _nextUpdateInteractTime;
    }
}