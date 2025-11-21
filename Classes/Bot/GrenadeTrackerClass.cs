using EFT;
using SAIN.Components;
using UnityEngine;

namespace SAIN.SAINComponent.SubComponents;

public class GrenadeTrackerClass
{
    public GrenadeTrackerClass(BotComponent bot, Grenade grenade, Vector3 dangerPoint, float reactionTime)
    {
        _bot = bot;
        _reactionTime = reactionTime;
        DangerPoint = dangerPoint;
        Grenade = grenade;
        if ((grenade.transform.position - bot.Position).magnitude < 10f)
        {
            SetSpotted();
        }
    }

    public void CheckHeardGrenadeCollision(float maxRange)
    {
        if (Spotted)
        {
            return;
        }
        maxRange *= 0.75f;
        if (GrenadeDistance < maxRange)
        {
            SetSpotted();
        }
    }

    private readonly BotComponent _bot;
    private BotOwner BotOwner => _bot.BotOwner;

    public float GrenadeDistance { get; private set; }

    public void Update()
    {
        if (BotOwner == null || BotOwner.IsDead || Grenade == null || _sentToBot)
        {
            return;
        }

        if (!_sentToBot && CanReact)
        {
            _sentToBot = true;
            var collisionSound = Grenade.GrenadeSettings.CollisionSound;
            bool isFrag = collisionSound == GrenadeSettings.CollisionSounds.frag;
            var trigger = isFrag ? EPhraseTrigger.OnEnemyGrenade : EPhraseTrigger.Look;
            _bot.Talk.GroupSay(trigger, ETagStatus.Combat, false, 100);

            Vector3 pos = DangerPoint;
            BotOwner.BewareGrenade.AddGrenadeDanger(pos, Grenade);
            return;
        }

        if (Spotted)
        {
            return;
        }

        GrenadeDistance = (Grenade.transform.position - BotOwner.Position).magnitude;
        if (GrenadeDistance < 3f)
        {
            SetSpotted();
            return;
        }

        if (_nextCheckRaycastTime < Time.time)
        {
            _nextCheckRaycastTime = Time.time + 0.05f;
            if (CheckVisibility())
            {
                SetSpotted();
            }
        }
    }

    private bool _sentToBot;

    private void SetSpotted()
    {
        if (!Spotted)
        {
            TimeSpotted = Time.time;
            Spotted = true;
        }
    }

    private bool CheckVisibility()
    {
        Vector3 lookPoint = _bot.Transform.WeaponRoot;
        Vector3 lookDir = _bot.LookDirection;

        Vector3 grenadePos = Grenade.transform.position + (Vector3.up * 0.05f);
        Vector3 grenadeDir = grenadePos - lookPoint;
        if (Vector3.Dot(lookDir, grenadeDir.normalized) < 0.25f)
        {
            return false; // Not looking in the right direction
        }
        return !Physics.Raycast(lookPoint, grenadeDir, 1f, LayerMaskClass.HighPolyWithTerrainMaskAI);
    }

    public void UpdateGrenadeDanger(Vector3 Danger)
    {
        DangerPoint = Danger;
        if (_sentToBot && !_updated)
        {
            _updated = true;
            BotOwner.BewareGrenade.AddGrenadeDanger(Danger, Grenade);
        }
    }

    private bool _updated;

    private float TimeSpotted { get; set; }
    public float TimeSinceSpotted => Spotted ? Time.time - TimeSpotted : 0f;
    public Grenade Grenade { get; private set; }
    public Vector3 DangerPoint { get; set; }
    private bool Spotted { get; set; }
    public bool CanReact => Spotted && TimeSinceSpotted > _reactionTime;

    private readonly float _reactionTime;
    private float _nextCheckRaycastTime;
}