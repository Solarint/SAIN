using EFT;
using System;

namespace SAIN.SAINComponent.BaseClasses
{
    public abstract class PersonBaseClass
    {
        public PersonBaseClass(IAIDetails iAIDetails)
        {
            IAIDetails = iAIDetails;
        }

        public IAIDetails IAIDetails { get; private set; }
        public bool PlayerNull => IAIDetails == null;
        public Player Player => IAIDetails as Player;
    }
}