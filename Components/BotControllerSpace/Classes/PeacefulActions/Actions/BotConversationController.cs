using SAIN.SAINComponent;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace SAIN.Components.BotController.PeacefulActions
{
    public class BotConversationController : SAINControllerBase, IPeacefulActionController
    {
        private const float FIND_COVERSATION_FREQ = 5f;
        private const float MAX_START_CONVO_RANGE = 40f;
        private const float MAX_START_CONVO_RANGE_SQR = MAX_START_CONVO_RANGE * MAX_START_CONVO_RANGE;

        public EPeacefulAction Action { get; }
        public List<IPeacefulActionExecutor> ActiveActions { get; } = new List<IPeacefulActionExecutor>();
        public bool Active => Count > 0;
        public int Count => ActiveActions.Count;

        public void CheckExecute(BotZoneData data)
        {
            if (_nextCheckTime < Time.time) {
                _nextCheckTime = Time.time + FIND_COVERSATION_FREQ;
                findConvoTargetsAndExecute(data);
            }
        }

        public BotConversationController(SAINBotController controller, EPeacefulAction action) : base(controller)
        {
            Action = action;
        }

        private void findConvoTargetsAndExecute(BotZoneData data)
        {
            _selectedBots.Clear();
            if (PeacefulActionHelpers.findBotsForPeacefulAction(data, _localList, _selectedBots, MAX_START_CONVO_RANGE_SQR)) {
                logConvoStart(data);
            }
        }

        private IEnumerator executeConversation(params BotComponent[] bots)
        {
            bool finishedConvo = false;
            while (!finishedConvo) {
                if (!recheckBots(bots)) {
                    finishedConvo = true;
                    break;
                }
                yield return null;
            }
        }

        private bool recheckBots(params BotComponent[] bots)
        {
            foreach (BotComponent bot in bots)
                if (bot == null || bot.HasEnemy)
                    return false;
            return true;
        }

        private void logConvoStart(BotZoneData data)
        {
            if (SAINPlugin.DebugMode || true) {
                StringBuilder stringBuilder = new StringBuilder();
                stringBuilder.AppendLine($"Conversation [{_conversations++}]");
                stringBuilder.AppendLine($"Selected [{_selectedBots.Count}] Bots");
                stringBuilder.AppendLine($"Name: [{data.Name}]");
                stringBuilder.AppendLine($"Time: [{Time.time}]");
                for (int j = 0; j < _selectedBots.Count; j++) {
                    var selected = _selectedBots[j];
                    stringBuilder.AppendLine($"[{j + 1}] : Selected: [{selected.name}]");
                }
                Logger.LogDebug(stringBuilder.ToString());
            }
        }

        private int _conversations;
        private readonly List<BotComponent> _selectedBots = new List<BotComponent>();
        private readonly List<BotComponent> _localList = new List<BotComponent>();
        private float _nextCheckTime;
    }
}