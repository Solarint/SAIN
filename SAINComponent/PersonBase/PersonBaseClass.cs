using EFT;
using System;

namespace SAIN.SAINComponent.BaseClasses
{
    public abstract class PersonBaseClass
    {
        public PersonBaseClass(IPlayer iPlayer)
        {
            IPlayer = iPlayer;
        }

        public IPlayer IPlayer { get; private set; }
        public bool PlayerNull => IPlayer == null;
        public Player Player => IPlayer as Player;
    }
}