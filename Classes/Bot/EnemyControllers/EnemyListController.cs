using EFT;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using SAIN.Helpers;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyListController : BotSubClass<SAINEnemyController>, IBotClass
{
    public Dictionary<string, Enemy> Enemies { get; } = [];
    public HashSet<Enemy> EnemiesArray { get; } = [];

    public EnemyListController(SAINEnemyController controller) : base(controller)
    {
        //TickInterval = 1.0f;
    }

    public override void Init()
    {
        GameWorldComponent.Instance.PlayerTracker.OnPlayerRemoved += RemoveEnemy;
        BotOwner.Memory.OnAddEnemy += enemyAdded;
        if (BotOwner.BotsGroup is BotsGroup botsGroup)
        {
            botsGroup.OnEnemyAdd += enemyAdded;
            botsGroup.OnEnemyRemove += RemoveEnemy;
        }
        compareEnemyLists();
        base.Init();
    }

    private void RemoveEnemy(IPlayer player)
    {
        if (player != null)
            RemoveEnemy(player.ProfileId);
    }

    public override void ManualUpdate()
    {
        compareEnemyLists();
#if DEBUG
        if (SAINPlugin.DebugMode)
        {
            foreach (Enemy enemy in Bot.EnemyController.VisibleEnemies)
            {
                if (enemy.Path.PathCorners != null && enemy.Path.PathCorners.Length > 0)
                {
                    GameObject pathObject = DebugGizmos.DrawLine(Vector3.zero, Vector3.forward, Color.red, 0.1f, 0.02f);
                    DebugGizmos.SetLinePositions(pathObject, enemy.Path.PathCorners);
                }
                //DebugGizmos.DrawLine(Bot.Transform.EyePosition, enemy.EnemyPosition, Color.red, 0.025f, 0.02f);
                //if (enemy.LastKnownPosition != null)
                //    DebugGizmos.DrawLine(enemy.LastKnownPosition.Value, enemy.EnemyPosition, Color.red, 0.025f, 0.02f);
            }
            foreach (Enemy enemy in Bot.EnemyController.EnemiesInLineOfSight)
            {
                if (enemy.Path.PathCorners != null && enemy.Path.PathCorners.Length > 0)
                {
                    GameObject pathObject = DebugGizmos.DrawLine(Vector3.zero, Vector3.forward, Color.yellow, 0.1f, 0.02f);
                    DebugGizmos.SetLinePositions(pathObject, enemy.Path.PathCorners);
                }
                //DebugGizmos.DrawLine(Bot.Transform.EyePosition, enemy.EnemyPosition, Color.yellow, 0.025f, 0.02f);
                //if (enemy.LastKnownPosition != null)
                //    DebugGizmos.DrawLine(enemy.LastKnownPosition.Value, enemy.EnemyPosition, Color.yellow, 0.025f, 0.02f);
            }
            foreach (Enemy enemy in Bot.EnemyController.KnownEnemies)
            {
                if (enemy.Path.PathCorners != null && enemy.Path.PathCorners.Length > 0)
                {
                    GameObject pathObject = DebugGizmos.DrawLine(Vector3.zero, Vector3.forward, Color.blue, 0.1f, 0.02f);
                    DebugGizmos.SetLinePositions(pathObject, enemy.Path.PathCorners);
                }
                //DebugGizmos.DrawLine(Bot.Transform.EyePosition, enemy.EnemyPosition, Color.blue, 0.025f, 0.02f);
                //if (enemy.LastKnownPosition != null)
                //    DebugGizmos.DrawLine(enemy.LastKnownPosition.Value, enemy.EnemyPosition, Color.blue, 0.025f, 0.02f);
            }
        }
#endif

        base.ManualUpdate();
    }

    public override void Dispose()
    {
        GameWorldComponent.Instance.PlayerTracker.OnPlayerRemoved -= RemoveEnemy;
        BotMemoryClass memory = BotOwner?.Memory;
        if (memory != null)
        {
            memory.OnAddEnemy -= enemyAdded;
        }
        if (BotOwner?.BotsGroup is BotsGroup botsGroup)
        {
            botsGroup.OnEnemyAdd -= enemyAdded;
            botsGroup.OnEnemyRemove -= RemoveEnemy;
        }

        foreach (var enemy in EnemiesArray)
        {
            destroyEnemy(enemy);
        }
        EnemiesArray.Clear();
        Enemies.Clear();
        base.Dispose();
    }

    public Enemy GetEnemy(string profileID, bool mustBeActive)
    {
        if (!Enemies.TryGetValue(profileID, out Enemy enemy))
        {
            return null;
        }
        if (enemy == null || !enemy.CheckValid())
        {
            destroyEnemy(enemy);
            Enemies.Remove(profileID);
            EnemiesArray.Remove(enemy);
            //if (enemy.EnemyPlayer.IsYourPlayer)
            //{
            //    Logger.LogDebug($"Removed Player Enemy for [{Bot.name}]");
            //}
            return null;
        }
        if (mustBeActive && !Enemy.IsEnemyActive(enemy))
        {
            return null;
        }
        return enemy;
    }

    public void RemoveEnemy(string id, PlayerComponent playerComp) => RemoveEnemy(id);

    public void RemoveEnemy(string profileId)
    {
        if (Enemies.TryGetValue(profileId, out Enemy enemy))
        {
            destroyEnemy(enemy);
            Enemies.Remove(profileId);
            EnemiesArray.Remove(enemy);

            //if (enemy.EnemyPlayer.IsYourPlayer)
            //{
            //    Logger.LogDebug($"Removed Player Enemy for [{Bot.name}]");
            //}
        }
    }

    private void destroyEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        BaseClass.Events.EnemyRemoved(enemy.EnemyProfileId, enemy);
        enemy.Dispose();
        removeEnemyInfo(enemy);
    }

    public Enemy CheckAddEnemy(IPlayer IPlayer)
    {
        return tryAddEnemy(IPlayer);
    }

    private void enemyAdded(IPlayer player)
    {
        tryAddEnemy(player);
    }
    private void enemyAdded(IPlayer player, EBotEnemyCause cause)
    {
        tryAddEnemy(player);
    }

    public bool IsBotInBotsGroup(BotOwner botOwner)
    {
        int count = BotOwner.BotsGroup.MembersCount;
        for (int i = 0; i < count; i++)
        {
            var member = BotOwner.BotsGroup.Member(i);
            if (member == null) continue;
            if (member.ProfileId == botOwner.ProfileId) return true;
        }
        return false;
    }

    private Enemy tryAddEnemy(IPlayer enemyPlayer)
    {
        if (enemyPlayer == null)
        {
            //Logger.LogDebug("Cannot add null player as an enemy.");
            return null;
        }
        if (!enemyPlayer.HealthController.IsAlive)
        {
            //Logger.LogDebug("Cannot add dead player as an enemy.");
            return null;
        }
        if (enemyPlayer.ProfileId == Bot.ProfileId)
        {
            //string debugString = $"Cannot add enemy that matches this bot: ";
            //debugString = findSourceDebug(debugString);
            //Logger.LogDebug(debugString);
            return null;
        }

        if (enemyPlayer.IsAI)
        {
            BotOwner botOwner = enemyPlayer.AIData?.BotOwner;
            if (botOwner == null)
            {
                //Logger.LogDebug("Cannot add ai as enemy with null Botowner");
                return null;
            }
            if (IsBotInBotsGroup(botOwner))
            {
                //Logger.LogDebug("Cannot add ai that is in my group");
                return null;
            }
        }

        PlayerComponent enemyPlayerComponent = getEnemyPlayerComponent(enemyPlayer);
        if (enemyPlayerComponent == null)
        {
            //Logger.LogWarning("Cannot add enemy with null Player Component.");
            return null;
        }

        if (Enemies.TryGetValue(enemyPlayer.ProfileId, out Enemy sainEnemy))
        {
            return sainEnemy;
        }

        EnemyInfo enemyInfo = getEnemyInfo(enemyPlayer);
        if (enemyInfo == null)
        {
            //Logger.LogWarning("Cannot add enemy that doesn't have an enemyInfo");
            return null;
        }

        return createEnemy(enemyPlayerComponent, enemyInfo);
    }

    private PlayerComponent getEnemyPlayerComponent(IPlayer enemyPlayer)
    {
        var playerTracker = GameWorldComponent.Instance.PlayerTracker;
        PlayerComponent enemyPlayerComponent = playerTracker.GetPlayerComponent(enemyPlayer.ProfileId);
        if (enemyPlayerComponent == null)
        {
            //Logger.LogDebug("Cannot add enemy with null Player Component");
            if (Enemies.TryGetValue(enemyPlayer.ProfileId, out Enemy oldEnemy))
            {
                destroyEnemy(oldEnemy);
                Enemies.Remove(enemyPlayer.ProfileId);
                //Logger.LogDebug($"Removed Old Enemy.");
            }
            enemyPlayerComponent = playerTracker.AddPlayerManual(enemyPlayer);
            if (enemyPlayerComponent == null)
            {
                //Logger.LogError("Failed to recreate component!");
            }
        }
        return enemyPlayerComponent;
    }

    private EnemyInfo getEnemyInfo(IPlayer enemyPlayer)
    {
        if (!BotOwner.EnemiesController.EnemyInfos.TryGetValue(enemyPlayer, out EnemyInfo enemyInfo) &&
            BotOwner.BotsGroup.Enemies.TryGetValue(enemyPlayer, out BotSettingsClass value))
        {
            //Logger.LogDebug($"Got EnemyInfo from Bot's Group Enemies.");
            enemyInfo = BotOwner.EnemiesController.AddNew(BotOwner.BotsGroup, enemyPlayer, value);
            if (enemyInfo != null)
            {
                //Logger.LogDebug($"Successfully Added new EnemyInfo.");
            }
        }
        return enemyInfo;
    }

    private Enemy createEnemy(PlayerComponent enemyPlayerComponent, EnemyInfo enemyInfo)
    {
        Enemy enemy = new(Bot, enemyPlayerComponent, enemyInfo);
        enemy.Init();
        Enemies.Add(enemy.EnemyProfileId, enemy);
        EnemiesArray.Add(enemy);
        BaseClass.Events.EnemyAdded(enemy);
        //if (enemyPlayerComponent.Player.IsYourPlayer)
        //{
        //    Logger.LogDebug($"Created Player Enemy for [{Bot.name}]");
        //}
        return enemy;
    }

    public bool IsPlayerAnEnemy(string profileID)
    {
        return !profileID.IsNullOrEmpty() && Enemies.ContainsKey(profileID);
    }

    public bool IsPlayerFriendly(IPlayer iPlayer)
    {
        if (iPlayer == null)
        {
            return false;
        }
        if (iPlayer.ProfileId == Bot.ProfileId)
        {
            return true;
        }

        if (Enemies.ContainsKey(iPlayer.ProfileId))
        {
            return false;
        }

        // Check that the source isn't from a member of the bot's group.
        if (iPlayer.AIData.IsAI &&
            BotOwner.BotsGroup.Contains(iPlayer.AIData.BotOwner))
        {
            return true;
        }

        // Checks if the player is not an active enemy and that they are a neutral party
        if (!BotOwner.BotsGroup.IsPlayerEnemy(iPlayer)
            && BotOwner.BotsGroup.Neutrals.ContainsKey(iPlayer))
        {
            return true;
        }

        // Check that the source isn't an ally
        if (BotOwner.BotsGroup.Allies.Contains(iPlayer))
        {
            return true;
        }

        if (iPlayer.IsAI &&
            iPlayer.AIData?.BotOwner?.Memory.GoalEnemy?.ProfileId == Bot.ProfileId)
        {
            return false;
        }

        if (!BotOwner.BotsGroup.Enemies.ContainsKey(iPlayer))
        {
            return true;
        }
        return false;
    }

    private void removeEnemyInfo(Enemy enemy)
    {
        if (enemy == null)
        {
            return;
        }

        if (enemy.EnemyPlayer != null &&
            BotOwner.EnemiesController.EnemyInfos.ContainsKey(enemy.EnemyPlayer))
        {
            BotOwner.EnemiesController.Remove(enemy.EnemyPlayer);
            return;
        }

        EnemyInfo badInfo = null;
        foreach (var enemyInfo in BotOwner.EnemiesController.EnemyInfos.Values)
        {
            if (enemyInfo?.Person != null &&
                enemyInfo.ProfileId == enemy.EnemyProfileId)
            {
                badInfo = enemyInfo;
                break;
            }
        }

        if (badInfo?.Person != null)
        {
            BotOwner.EnemiesController.Remove(badInfo.Person);
        }
    }

    private void compareEnemyLists()
    {
        if (_nextCompareListsTime < Time.time)
        {
            _nextCompareListsTime = Time.time + COMPARE_ENEMY_LIST_FREQ;

            int enemyCount = Enemies.Count;
            int failedGroupAdds = 0;
            var groupEnemies = BotOwner.BotsGroup.Enemies;
            foreach (var person in groupEnemies.Keys)
            {
                Enemy enemy = tryAddEnemy(person);
                if (enemy == null)
                {
                    failedGroupAdds++;
                }
            }

            int failedMyAdds = 0;
            var myEnemies = BotOwner.EnemiesController.EnemyInfos;
            foreach (var person in myEnemies.Keys)
            {
                Enemy enemy = tryAddEnemy(person);
                if (enemy == null)
                {
                    failedMyAdds++;
                }
            }

            if (failedMyAdds > 0 || failedGroupAdds > 0)
            {
                //Logger.LogDebug($"Failed to add [{failedGroupAdds}] enemies from botsgroup and [{failedMyAdds}] enemies from enemyInfos");
            }
        }
    }

    private float _nextCompareListsTime;
    private const float COMPARE_ENEMY_LIST_FREQ = 1;
}