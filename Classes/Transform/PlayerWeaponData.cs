using EFT;
using UnityEngine;

namespace SAIN.Classes.Transform;

public struct PlayerWeaponData
{
    /// <summary>
    /// The place the bullet shoots from
    /// </summary>
    public Vector3 FirePort;

    /// <summary>
    /// The direction a player's weapon is actually pointing
    /// </summary>
    public Vector3 PointDirection;

    /// <summary>
    /// Player's Weapon Root Position
    /// </summary>
    public Vector3 WeaponRoot;

    /// <summary>
    /// Direction from a player's position to their weaponroot.
    /// </summary>
    public Vector3 WeaponRootOffset;

    /// <summary>
    /// A Player's position, but the y value is set to their weaponroot's y.
    /// </summary>
    public Vector3 WeaponRootHeightOffset;

    public static PlayerWeaponData Update(PlayerWeaponData weaponData, Vector3 playerPosition, Player player)
    {
        Vector3 weapRoot = player.WeaponRoot.position;
        weaponData.WeaponRoot = new Vector3(weapRoot.x, weapRoot.y, weapRoot.z);
        weaponData.WeaponRootOffset = new Vector3(weapRoot.x - playerPosition.x, weapRoot.y - playerPosition.y, weapRoot.z - playerPosition.z);
        weaponData.WeaponRootHeightOffset = new Vector3(0, weapRoot.y - playerPosition.y, 0);

        // Is the player holding a weapon?
        if (player.HandsController is Player.FirearmController firearmController &&
            firearmController.CurrentFireport != null)
        {
            Vector3 firePort = firearmController.CurrentFireport.position;
            Vector3 pointDir = firearmController.CurrentFireport.Original.TransformDirection(player.LocalShotDirection);
            firearmController.AdjustShotVectors(ref firePort, ref pointDir);
            weaponData.FirePort = firePort;
            weaponData.PointDirection = pointDir.normalized;
        }
        else
        {
            // we failed to get fireport info, set the positions to a fallback
            weaponData.FirePort = weaponData.WeaponRoot;
            weaponData.PointDirection = player.LookDirection;
        }
        return weaponData;
    }
}