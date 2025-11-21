using EFT.Interactive;
using UnityEngine;

namespace SAIN.Components;

public class BotDoorTrigger : MonoBehaviour
{
    public void Awake()
    {
        SphereCollider = this.gameObject.AddComponent<SphereCollider>();
        SphereCollider.isTrigger = true;
        SphereCollider.radius = 10f;
        SphereCollider.enabled = true;
        _doorHandler = GameWorldComponent.Instance.Doors;
    }

    public void Update()
    {
    }

    public void initDoor(Door door)
    {
        this._door = door;
        this.transform.position = door.transform.position;
        SphereCollider.transform.position = this.transform.position;
    }

    private Door _door;

    public void OnDestroy()
    {
    }

    public SphereCollider SphereCollider { get; private set; }

    public void OnTriggerEnter(Collider other)
    {
        Logger.LogDebug($"Enter: {other.gameObject?.name}");
        if (_door.DoorState == EDoorState.Shut)
        {
            bool shallInvert = shallInvertDoorAngle(_door, other.transform.position);
            _doorHandler.ChangeDoorState(_door, EDoorState.Open, shallInvert);
            Logger.LogInfo("open");
        }
    }

    public void OnTriggerStay(Collider other)
    {
        if (_door.DoorState == EDoorState.Open)
        {
            var colliders = _door.gameObject.GetComponents<Collider>();
            foreach (var collider in colliders)
            {
                Physics.IgnoreCollision(other, collider, true);
                //Logger.LogInfo("ignoreCollisions");
            }
        }
    }

    public void OnTriggerExit(Collider other)
    {
        Logger.LogDebug($"Exit: {other.gameObject?.name}");
        if (_door.DoorState == EDoorState.Open)
        {
            _doorHandler.ChangeDoorState(_door, EDoorState.Shut, false);
            Logger.LogInfo("close");
        }
        var colliders = _door.gameObject.GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            Physics.IgnoreCollision(other, collider, false);
            //Logger.LogInfo("resetCollisions");
        }
    }

    private bool shallInvertDoorAngle(Door door, Vector3 colliderPosition)
    {
        var interactionParameters = door.GetInteractionParameters(colliderPosition);
        if (interactionParameters.AnimationId == (door.DoorState is EDoorState.Locked ? (int)door.DoorKeyOpenInteraction : door.CalculateInteractionIndex(colliderPosition)))
        {
            return false;
        }
        return true;
    }

    private DoorHandler _doorHandler;
}