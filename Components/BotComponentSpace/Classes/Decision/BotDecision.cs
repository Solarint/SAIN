using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.Decision
{
    public class AIDecisionStruct<T> where T : Enum
    {
        public T Decision;
        public int Priority;
    }

    public abstract class AIDecisionClass
    {
        public AIDecisionClass(BotComponent bot)
        {
            Bot = bot;
        }

        protected BotComponent Bot;
    }

    public class AIDecisionManager<T> : BotBase, IBotClass where T : Enum
    {
        public AIDecisionManager(BotComponent bot) : base(bot)
        {
        }

        public void Init()
        {
        }

        public void Update()
        {
        }

        public void Dispose()
        {
        }

        private void sort()
        {
            _decisions.Sort((x, y) => x.Priority.CompareTo(y.Priority));
        }

        private readonly List<AIDecisionStruct<T>> _decisions = new List<AIDecisionStruct<T>>();
    }
}