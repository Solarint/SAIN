using SAIN.SAINComponent.SubComponents.CoverFinder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SAIN.SAINComponent.GOAP
{
    public enum CharacterStates
    {
        NeedCover,
        FoundCover,
        InCover,
        OutOfAmmo,
        GoodPosition,
        HasEnemy,
    }

    public class GOAPTestClassSeekCover
    {
        public readonly CharacterStates[] EnterBy =
        {
            CharacterStates.NeedCover
        };

        public bool FoundCover = false;
        public CoverPoint PointFound = null;

        public void Activate()
        {
            FoundCover = true;
        }
    }
    public class GOAPTestClassMoveToCover
    {
        public readonly CharacterStates[] EnterBy =
        {
            CharacterStates.FoundCover
        };

        public bool InCover = false;
        public CoverPoint PointImMovingTo = null;

        public void Activate()
        {
            InCover = true;
        }
    }
}
