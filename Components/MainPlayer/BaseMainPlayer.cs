using EFT;

namespace SAIN.Components.MainPlayer
{
    public abstract class BaseMainPlayer
    {
        public BaseMainPlayer(SAINMainPlayerComponent playerComp)
        {
            PlayerComponent = playerComp;
        }

        public readonly SAINMainPlayerComponent PlayerComponent;
        public Player MainPlayer => PlayerComponent.MainPlayer;
    }
    public interface iMainPlayer
    {
        void Start();
        void Update();
        void OnDestroy();
    }
}