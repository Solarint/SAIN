using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.Components.PlayerComponentSpace;

/// <summary>
/// A struct to pre-cache all relevant position data for a player for quicker memory access.
/// struct is updated from SAIN Gameworld Component in a batch.
/// </summary>
public struct PlayerPositionData
{
    public PlayerPositionData(Player Player)
    {
        if (Player == null)
        {
#if DEBUG
            Logger.LogError($"Player == null");
#endif
            return;
        }
        if (Player.Profile == null)
        {
#if DEBUG
            Logger.LogError($"Player.Profile == null");
#endif
        }
        PlayerNickname = Player.Profile.Nickname;
        if (Player.MainParts == null)
        {
#if DEBUG
            Logger.LogError($"Player.MainParts == null");
#endif
            return;
        }
        BodyParts = Player.MainParts;
        if (BodyParts.TryGetValue(BodyPartType.head, out var head))
        {
            Head = head;
        }
        if (BodyParts.TryGetValue(BodyPartType.body, out var body))
        {
            MainBody = body;
        }
    }

    private readonly Dictionary<BodyPartType, EnemyPart> BodyParts;
    private readonly string PlayerNickname;

    public readonly EnemyPart Head;
    public readonly EnemyPart MainBody;

    public Vector3 Forward;
    public Vector3 Right;

    /// <summary>
    /// Cache all properties in this struct.
    /// </summary>
    public void Update(Player Player)
    {
        // The excessive null checks here are just to verify no mistakes are made. If there are no errors during gameplay during testing we should be able to safely remove them and remove some overhead  - Solarint
        if (Player == null)
        {
#if DEBUG
            Logger.LogError($"Player Null");
#endif
            return;
        }
        /////
        Vector3 Zero = Vector3.zeroVector;

        BifacialTransform Transform = Player.Transform;
        if (Transform == null)
        {
#if DEBUG
            Logger.LogError($"Player Transform Null");
#endif
        }
        else
        {
            Position = Transform.position;
        }
        /////
        MovementContext movementContext = Player.MovementContext;
        if (movementContext == null)
        {
#if DEBUG
            Logger.LogError($"Player MovementContext Null");
#endif
        }
        else
        {
            LookDirection = movementContext.LookDirection;
            Forward = movementContext.PlayerRealForward;
            Right = movementContext.PlayerRealRight;
        }
        /////
        EnemyPart MyHeadPart = Head;
        if (MyHeadPart == null)
        {
#if DEBUG
            Logger.LogError($"{PlayerNickname}'s Head Part is null");
#endif
            HeadPosition = Zero;
        }
        else
        {
            HeadPosition = MyHeadPart.Position;
        }
        /////
        EnemyPart MyMainBodyPart = MainBody;
        if (MyMainBodyPart == null)
        {
#if DEBUG
            Logger.LogError($"{PlayerNickname}'s MainBody Part is null");
#endif
            BodyPosition = Zero;
        }
        else
        {
            BodyPosition = MyMainBodyPart.Position;
        }
        /////
        BifacialTransform WeaponRoot = Player.WeaponRoot;
        if (WeaponRoot == null)
        {
            HasWeaponEquipped = false;
            WeaponFireport = Zero;
            WeaponPointDirection = Zero;
        }
        else
        {
            HasWeaponEquipped = true;
            WeaponFireport = WeaponRoot.position;
            WeaponPointDirection = WeaponRoot.forward;
        }
    }

    public readonly bool GetBodyPartPosition(BodyPartType PartType, out Vector3 Result)
    {
        Result = PartType switch {
            BodyPartType.head => HeadPosition,
            BodyPartType.body => BodyPosition,
            _ => GetBodyPartPosition(PartType),
        };
        return Result != Vector3.zero;
    }

    private readonly Vector3 GetBodyPartPosition(BodyPartType PartType)
    {
        EnemyPart Part = GetBodyPart(PartType);
        return Part != null ? Part.Position : Vector3.zero;
    }

    private readonly EnemyPart GetBodyPart(BodyPartType PartType)
    {
        EnemyPart Result = null;
        if (BodyParts == null)
        {
#if DEBUG
            Logger.LogError($"[{PlayerNickname}] Body Parts Dictionary Null");
#endif
            return Result;
        }
        if (!BodyParts.TryGetValue(PartType, out Result))
        {
#if DEBUG
            Logger.LogError($"[{PlayerNickname}] Body Part [{PartType}] is not in Parts Dictionary");
#endif
            return null;
        }
        if (Result == null)
        {
#if DEBUG
            Logger.LogError($"[{PlayerNickname}] Body Part [{PartType}] is Null");
#endif
        }
        return Result;
    }

    public Vector3 Position;
    public Vector3 LookDirection;
    public Vector3 HeadPosition;
    public Vector3 BodyPosition;

    public bool HasWeaponEquipped;
    public Vector3 WeaponFireport;
    public Vector3 WeaponPointDirection;

    public bool IsOnNavMesh;
    public Vector3 NavMeshPosition;
    public Vector3 LastValidNavMeshPosition;
}