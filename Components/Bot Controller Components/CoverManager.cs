using BepInEx.Logging;
using Comfort.Common;
using UnityEngine;

public class CoverManager : MonoBehaviour
{
    private void Awake()
    {
        Logger = BepInEx.Logging.Logger.CreateLogSource(GetType().Name);
    }

    private ManualLogSource Logger;

    private void Update()
    {
        var game = Singleton<IBotGame>.Instance;
        if (game == null)
        {
            return;
        }
    }
}