using Comfort.Common;
using EFT.Interactive;
using SAIN.Preset.GlobalSettings;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components
{
    public class DoorHandler : GameWorldBase, IGameWorldClass
    {
        public event Action<Door, EDoorState, bool> OnDoorStateChanged;

        public event Action<bool> OnDoorsDisabled;

        public DoorHandler(GameWorldComponent component) : base(component)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
            checkDoors();
        }

        public void Dispose()
        {
            foreach (var door in _doorsWithTriggers) {
                var collider = door.Value?.gameObject?.GetComponent<SphereCollider>();
                if (collider != null) {
                    GameObject.Destroy(collider);
                }
                GameObject.Destroy(door.Value);
            }
            _doorsWithTriggers.Clear();
        }

        public void ChangeDoorState(Door door, EDoorState state, bool shallInvert)
        {
            if (shallInvert)
                door.OpenAngle = -door.OpenAngle;

            door.SetDoorState(state);
            OnDoorStateChanged?.Invoke(door, state, shallInvert);

            if (shallInvert)
                door.OpenAngle = -door.OpenAngle;
        }

        public void HostDisabledDoors(bool value)
        {
            _doorsDisabledByHost = value;
        }

        private void checkDoors()
        {
            if (Singleton<IBotGame>.Instance == null) { return; }

            bool shallDisable = _doorsDisabledByHost || GlobalSettingsClass.Instance.General.Doors.DisableAllDoors;

            if (!_doorsDisabled &&
                shallDisable) {
                OnDoorsDisabled?.Invoke(true);
                _doorsDisabled = true;
                disableDoors();
                return;
            }
            if (_doorsDisabled &&
                !shallDisable) {
                OnDoorsDisabled?.Invoke(false);
                _doorsDisabled = false;
                enableDoors();
                return;
            }
        }

        public bool DisableDoor(Door door)
        {
            // We don't support doors that don't start open/closed
            if (door.DoorState != EDoorState.Open && door.DoorState != EDoorState.Shut)
                return false;

            // We don't support non-operatable doors
            if (!door.Operatable || !door.enabled)
                return false;

            // We don't support doors that aren't on the "Interactive" layer
            if (door.gameObject.layer != LayerMaskClass.InteractiveLayer)
                return false;

            door.gameObject.SmartDisable();
            door.enabled = false;
            _disabledDoors.Add(door.Id, door);
            return true;
        }

        private void disableDoors()
        {
            int doorCount = 0;
            // Code taken from Drakia's Door Randomizer Mod
            UnityEngine.Object.FindObjectsOfType<Door>().ExecuteForEach(door => {
                if (DisableDoor(door))
                    doorCount++;
            });

            _doorsDisabled = true;
            Logger.LogDebug($"Disabled Doors: {doorCount}");
        }

        private void enableDoors()
        {
            int doorCount = 0;
            foreach (var door in _disabledDoors) {
                door.Value.gameObject.SmartEnable();
                door.Value.enabled = true;
                doorCount++;
            }
            _disabledDoors.Clear();
            _doorsDisabled = false;
            Logger.LogDebug($"Enabled Doors: {doorCount}");
        }

        private bool _doorsDisabled;
        private readonly Dictionary<string, Door> _disabledDoors = new Dictionary<string, Door>();
        private readonly Dictionary<int, GameObject> _doorsWithTriggers = new Dictionary<int, GameObject>();

        private bool _doorsDisabledByHost;
    }
}