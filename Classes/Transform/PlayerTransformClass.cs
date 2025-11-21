using EFT;
using UnityEngine;

namespace SAIN.Classes.Transform;

public class PlayerTransformClass
{
    public Vector3 Position { get; private set; }
    public Vector3 EyePosition { get; private set; }
    public Vector3 BodyPosition { get; private set; }
    public Vector3 LookDirection { get; private set; }

    public PlayerHeadData HeadData { get; private set; } = new();
    public PlayerNavData NavData { get; private set; } = new();
    public PlayerVelocityData VelocityData { get; private set; } = new();
    public PlayerWeaponData WeaponData { get; private set; } = new();

    public Vector3 WeaponRoot => WeaponData.WeaponRoot;

    public Vector3 Right() => AngledLookDirection(0f, 90f, 0f);

    public Vector3 Left() => AngledLookDirection(0f, -90f, 0f);

    public Vector3 AngledLookDirection(float x, float y, float z) => Quaternion.Euler(x, y, z) * LookDirection;

    public void ManualUpdate(Player player, bool isAI)
    {
        Vector3 playerPosition = player.Position;
        Vector3 playerLookDir = player.LookDirection;
        BifacialTransform headTransform = player.PlayerBones.Head;
        BifacialTransform bodyTransform = player.PlayerBones.Ribcage;

        // zombies don't have eye part
        EBodyPartColliderType eyePart = player.PlayerBones.BodyPartCollidersDictionary.ContainsKey(EBodyPartColliderType.Eyes)
            ? EBodyPartColliderType.Eyes
            : EBodyPartColliderType.HeadCommon;
        BodyPartCollider eyeCollider = player.PlayerBones.BodyPartCollidersDictionary[eyePart];

        Position = playerPosition;
        LookDirection = playerLookDir;
        EyePosition = eyeCollider.Center;
        BodyPosition = bodyTransform.position;

        WeaponData = PlayerWeaponData.Update(WeaponData, playerPosition, player);
        NavData = PlayerNavData.Update(NavData, playerPosition, isAI);
        VelocityData = PlayerVelocityData.Update(VelocityData, player.MovementContext.Velocity);
        HeadData = PlayerHeadData.Update(HeadData, headTransform.position, headTransform.rotation, headTransform.up);
    }
}