using EFT;
using HarmonyLib;
using SAIN.Components;
using SAIN.SAINComponent.Classes.EnemyClasses;
using SAIN.SAINComponent.SubComponents;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.WeaponFunction;

public class GrenadeVelocityTracker : MonoBehaviour
{
    public Vector3 Velocity { get; private set; }
    public float VelocityMagnitude { get; private set; }

    private const float GRENADE_UPDATE_FREQUENCY = 0.5f;

    public void Awake()
    {
        _grenade = this.GetComponent<Grenade>();
        _grenade.DestroyEvent += GrenadeDestroyed;
        _rigidBody = (Rigidbody)_rigidBodyField.GetValue(_grenade);
    }

    public void Update()
    {
        if (_grenade == null) return;
        if (_rigidBody == null)
        {
            GrenadeDestroyed(_grenade);
            return;
        }
        if (_nextUpdateTime < Time.time)
        {
            _nextUpdateTime = Time.time + GRENADE_UPDATE_FREQUENCY;
            Velocity = _rigidBody.velocity;
            VelocityMagnitude = Velocity.magnitude;
            //Logger.LogInfo($"Grenade {_grenade.Id} Velocity [{Velocity}] Magnitude: [{VelocityMagnitude}]");
        }
    }

    private Rigidbody _rigidBody;
    private Grenade _grenade;

    static GrenadeVelocityTracker()
    {
        _rigidBodyField = AccessTools.Field(typeof(Throwable), "Rigidbody");
    }

    private void GrenadeDestroyed(Throwable grenade)
    {
        if (grenade != null)
        {
            grenade.DestroyEvent -= GrenadeDestroyed;
        }
        Destroy(this);
    }

    private static FieldInfo _rigidBodyField;
    private float _nextUpdateTime;
}

public class GrenadeReactionClass : BotSubClass<BotGrenadeManager>, IBotClass
{
    public GrenadeTrackerClass DangerGrenade { get; private set; }
    public Vector3? GrenadeDangerPoint => DangerGrenade?.DangerPoint;
    public Dictionary<Throwable, GrenadeTrackerClass> EnemyGrenadesList { get; private set; } = [];

    public GrenadeReactionClass(BotGrenadeManager ThrowWeapItemClass) : base(ThrowWeapItemClass)
    {
    }

    public override void Init()
    {
        var grenadeController = BotManagerComponent.Instance.GrenadeController;
        grenadeController.OnGrenadeCollision += GrenadeCollision;
        grenadeController.OnGrenadeThrown += EnemyGrenadeThrown;
        grenadeController.OnGrenadeDangerUpdated += GrenadeDangerUpdated;
        base.Init();
    }

    public override void ManualUpdate()
    {
        foreach (var tracker in EnemyGrenadesList.Values)
        {
            tracker?.Update();
        }
        base.ManualUpdate();
    }

    public override void Dispose()
    {
        var grenadeController = BotManagerComponent.Instance.GrenadeController;
        grenadeController.OnGrenadeCollision -= GrenadeCollision;
        grenadeController.OnGrenadeThrown -= EnemyGrenadeThrown;
        grenadeController.OnGrenadeDangerUpdated -= GrenadeDangerUpdated;

        foreach (var tracker in EnemyGrenadesList.Values)
            if (tracker?.Grenade != null)
                tracker.Grenade.DestroyEvent -= RemoveGrenade;
        EnemyGrenadesList.Clear();
        base.Dispose();
    }

    public void EnemyGrenadeThrown(Grenade grenade, Vector3 dangerPoint, string profileId)
    {
        if (Bot == null || profileId == Bot.ProfileId || !Bot.BotActive)
        {
            return;
        }
        Enemy enemy = Bot.EnemyController.GetEnemy(profileId, false);
        if (enemy != null &&
            enemy.RealDistance <= MAX_ENEMY_GRENADE_DIST_TOCARE)
        {
            EnemyGrenadesList.Add(grenade, new GrenadeTrackerClass(Bot, grenade, dangerPoint, GetReactionTime()));
            grenade.DestroyEvent += RemoveGrenade;
            return;
        }
        BotOwner.BewareGrenade.AddGrenadeDanger(dangerPoint, grenade);
    }

    private const float MAX_ENEMY_GRENADE_DIST_TOCARE = 125;

    private void GrenadeCollision(Grenade grenade, float maxRange)
    {
        if (EnemyGrenadesList.TryGetValue(grenade, out var Tracker))
        {
            Tracker?.CheckHeardGrenadeCollision(maxRange);
        }
    }

    private void GrenadeDangerUpdated(Grenade grenade, Vector3 Danger)
    {
        if (EnemyGrenadesList.TryGetValue(grenade, out var Tracker))
        {
            Tracker.UpdateGrenadeDanger(Danger);
        }
    }

    private void RemoveGrenade(Throwable grenade)
    {
        if (grenade != null)
        {
            grenade.DestroyEvent -= RemoveGrenade;
            EnemyGrenadesList.Remove(grenade);
        }
    }

    public float GetReactionTime()
    {
        float reactionTime = 0.25f;
        reactionTime /= Bot.Info.Profile.DifficultyModifier;
        reactionTime *= Random.Range(0.75f, 1.25f);
        return Mathf.Clamp(reactionTime, 0.2f, 1f);
    }
}