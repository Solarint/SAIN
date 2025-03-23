using EFT;
using SAIN.Helpers;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class SAINEnemyController : BotBase, IBotClass
    {
        public Dictionary<string, Enemy> Enemies => _listController.Enemies;
        public EnemyControllerEvents Events { get; }
        public EnemyListsClass EnemyLists { get; }

        public Enemy ActiveEnemy => _enemyChooser.ActiveEnemy;
        public Enemy LastEnemy => _enemyChooser.LastEnemy;
        public bool AtPeace => Events.OnPeaceChanged.Value && Events.OnPeaceChanged.TimeSinceTrue > 1f;
        public bool ActiveHumanEnemy => Events.ActiveHumanEnemyEvent.Value;
        public bool HumanEnemyInLineofSight => Events.HumanInLineOfSightEvent.Value;

        public SAINEnemyController(BotComponent sain) : base(sain)
        {
            Events = new EnemyControllerEvents(this);
            EnemyLists = new EnemyListsClass(this);
            _listController = new EnemyListController(this);
            _enemyChooser = new EnemyChooserClass(this);
            _enemyUpdater = new EnemyUpdaterClass(sain);
        }

        public void Init()
        {
            _listController.Init();
            Events.Init();
            EnemyLists.Init();
            _enemyChooser.Init();
            _enemyUpdater.Init();
        }

        public void Update()
        {
            _enemyUpdater.Update();
            Events.Update();
            _listController.Update();
            _enemyChooser.Update();
            EnemyLists.Update();
            updateDebug();
        }

        public void LateUpdate()
        {
            _enemyUpdater.LateUpdate();
        }

        public void Dispose()
        {
            // must be first, so all enemies are removed properly and their events are triggered!
            _listController.Dispose();
            Events.Dispose();
            EnemyLists.Dispose();
            _enemyChooser.Dispose();
            _enemyUpdater.Dispose();
        }

        private void updateDebug()
        {
            var enemy = ActiveEnemy;
            if (enemy != null) {
                if (SAINPlugin.DebugMode && SAINPlugin.DrawDebugGizmos) {
                    if (enemy.KnownPlaces.LastHeardPosition != null) {
                        if (debugLastHeardPosition == null) {
                            debugLastHeardPosition = DebugGizmos.Line(enemy.KnownPlaces.LastHeardPosition.Value, Bot.Position, Color.yellow, 0.01f, false, Time.deltaTime, true);
                        }
                        DebugGizmos.UpdatePositionLine(enemy.KnownPlaces.LastHeardPosition.Value, Bot.Position, debugLastHeardPosition);
                    }
                    if (enemy.KnownPlaces.LastSeenPosition != null) {
                        if (debugLastSeenPosition == null) {
                            debugLastSeenPosition = DebugGizmos.Line(enemy.KnownPlaces.LastSeenPosition.Value, Bot.Position, Color.red, 0.01f, false, Time.deltaTime, true);
                        }
                        DebugGizmos.UpdatePositionLine(enemy.KnownPlaces.LastSeenPosition.Value, Bot.Position, debugLastSeenPosition);
                    }
                }
                else if (debugLastHeardPosition != null || debugLastSeenPosition != null) {
                    GameObject.Destroy(debugLastHeardPosition);
                    GameObject.Destroy(debugLastSeenPosition);
                }
            }
            else if (debugLastHeardPosition != null || debugLastSeenPosition != null) {
                GameObject.Destroy(debugLastHeardPosition);
                GameObject.Destroy(debugLastSeenPosition);
            }
        }

        public void ClearEnemy() => _enemyChooser.ClearEnemy();

        public Enemy GetEnemy(string profileID, bool mustBeActive) => _listController.GetEnemy(profileID, mustBeActive);

        public Enemy CheckAddEnemy(IPlayer IPlayer) => _listController.CheckAddEnemy(IPlayer);

        public void RemoveEnemy(string profileID) => _listController.RemoveEnemy(profileID);

        public bool IsPlayerAnEnemy(string profileID) => _listController.IsPlayerAnEnemy(profileID);

        public bool IsPlayerFriendly(IPlayer iPlayer) => _listController.IsPlayerFriendly(iPlayer);

        public bool IsBotInBotsGroup(BotOwner botOwner) => _listController.IsBotInBotsGroup(botOwner);

        private readonly EnemyListController _listController;
        private readonly EnemyChooserClass _enemyChooser;
        private readonly EnemyUpdaterClass _enemyUpdater;

        private GameObject debugLastSeenPosition;
        private GameObject debugLastHeardPosition;
    }
}