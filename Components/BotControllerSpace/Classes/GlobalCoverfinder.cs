using SAIN.Components;
using SAIN.SAINComponent;
using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace SAIN.BotController.Classes
{
    public class GlobalCoverfinder : SAINControllerBase
    {
        public GlobalCoverfinder(SAINBotController botController) : base(botController)
        {
        }

        public void FindCoverForBots()
        {
            if (_findCoverCoroutine == null) {
                _findCoverCoroutine = BotController.StartCoroutine(Main());
            }
        }

        private Coroutine _findCoverCoroutine;

        private IEnumerator Main()
        {
            while (true) {
                yield return null;
            }
        }

        public void Dispose()
        {
            if (_findCoverCoroutine != null) {
                BotController.StopCoroutine(_findCoverCoroutine);
                _findCoverCoroutine = null;
            }
        }

        private readonly List<BotComponent> _localBotList = new List<BotComponent>();
        public readonly Dictionary<int, CoverObject> ActiveCoverObjects = new Dictionary<int, CoverObject>();
    }

    public class CoverObject
    {
        public int id;
        public Collider collider;
        public Dictionary<string, CoverPoint> BotCoverPointDictionary = new Dictionary<string, CoverPoint>();
    }
}