namespace SAIN.Components;

public interface IGameWorldClass
{
    void Init();

    void ManualUpdate(float currentTime, float deltaTime);

    void Dispose();
}