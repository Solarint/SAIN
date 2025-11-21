using SAIN.Components;
using System.Collections;
using UnityEngine;

namespace SAIN.SAINComponent.Classes;

public class SteeringController : BotBase
{
    public Vector3 LookDirection => Bot.Transform.LookDirection;
    public Vector3 TargetSteerDirection { get; private set; }

    private Coroutine _controller;

    public SteeringController(BotComponent sain) : base(sain)
    {
        CanEverTick = false;
    }

    public override void Init()
    {
        Bot.BotActivation.BotActiveToggle.OnToggle += onBotActive;
        onBotActive(true);
        base.Init();
    }

    public override void Dispose()
    {
        Bot.BotActivation.BotActiveToggle.OnToggle -= onBotActive;
        base.Dispose();
    }

    private void onBotActive(bool value)
    {
        switch (value)
        {
            case true:
                if (_controller == null)
                {
                    _controller = Bot.StartCoroutine(controlSteeringLoop());
                }
                break;

            case false:
                if (_controller != null)
                {
                    Bot.StopCoroutine(_controller);
                    _controller = null;
                }
                break;
        }
    }

    private IEnumerator controlSteeringLoop()
    {
        while (true)
        {
            yield return null;
        }
    }
}