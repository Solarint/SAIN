using SAIN.Components.CoverFinder;
using SAIN.Components.PlayerComponentSpace;
using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

namespace SAIN.Components.BotControllerSpace.Classes.Raycasts;

public struct DirCalcData
{
    public DirCalcData(Vector3 inPoint)
    {
        Point = inPoint;
    }

    public Vector3 Point;
    public Vector3 Dir;
    public Vector3 DirNormal;
    public float Magnitude;
}

public struct CheckCoverJob : IJobFor
{
    [ReadOnly] public NativeArray<ColliderCoverData> Input;
    [WriteOnly] public NativeArray<ColliderCoverData> Output;

    public void Execute(int index)
    {
        ColliderCoverData Data = Input[index];

        Data.BotToCoverDirectionData = CalcDirection(Data.BotToCoverDirectionData, Data.BotPosition, Data.ColliderPosition);
        Data.TargetToCoverDirectionData = CalcDirection(Data.TargetToCoverDirectionData, Data.TargetPosition, Data.ColliderPosition);

        Output[index] = Data;
    }

    private static DirCalcData CalcDirection(DirCalcData Data, Vector3 Point, Vector3 ColliderPosition)
    {
        Data.Dir = ColliderPosition - Point;
        Data.DirNormal = Data.Dir.normalized;
        Data.Magnitude = Data.Dir.magnitude;
        return Data;
    }

    private static void CreateCoverPosition(ref ColliderCoverData Data)
    {
        Vector3 BotToTargetNorm = Data.BotToTargetDirectionData.DirNormal;
        Data.DotFromBotToTargetToCollider = Vector3.Dot(BotToTargetNorm, Data.BotToCoverDirectionData.DirNormal);
        Data.DotFromTargetToBotToCollider = Vector3.Dot(-BotToTargetNorm, Data.TargetToCoverDirectionData.DirNormal);
    }

    public void Dispose()
    {
        Input.Dispose();
        Output.Dispose();
    }
}

public struct PlayerDirectionDataJob : IJobFor
{
    [ReadOnly] public NativeArray<PlayerDirectionData> Input;
    [WriteOnly] public NativeArray<PlayerDirectionData> Output;

    public void Execute(int index)
    {
        PlayerDirectionData Data = Input[index];

        Data.MainData.Update(Data.OwnerPosition);
        Data.MainData.UpdateDotProductAndCalcNormal(Data.OwnerViewPosition, Data.OwnerLookDirection);

        //for (int i = 0; i < Data.BodyParts.Length; i++)
        //{
        //    Data.BodyParts[i].DirectionData.Update(Data.OwnerViewPosition);
        //    Data.BodyParts[i].DirectionData.UpdateDotProduct(Data.OwnerLookDirection);
        //}
        Output[index] = Data;
    }

    public void Dispose()
    {
        if (Input.IsCreated) Input.Dispose();
        if (Output.IsCreated) Output.Dispose();
    }
}

public struct PlayerTickData
{
    public PlayerTickData(PlayerComponent inOwner)
    {
        Owner = inOwner;
        OwnerProfileId = inOwner.ProfileId;
        OwnerViewPosition = inOwner.Transform.EyePosition;
        OwnerPosition = inOwner.Position;
        OwnerLookDirection = inOwner.LookDirection;
    }

    public readonly PlayerComponent Owner;
    public readonly string OwnerProfileId;
    public Vector3 OwnerViewPosition;
    public Vector3 OwnerPosition;
    public Vector3 OwnerLookDirection;

    public void Prepare(PlayerComponent Owner)
    {
        OwnerViewPosition = Owner.Transform.EyePosition;
        OwnerPosition = Owner.Position;
        OwnerLookDirection = Owner.LookDirection;

        OtherPlayerData.Clear();
        List<OtherPlayerData> OtherPlayers = Owner.OtherPlayersData.DataList;
        OtherPlayerDirectionData = new NativeArray<PlayerDirectionData>(OtherPlayers.Count, Allocator.TempJob);
        for (int j = 0; j < OtherPlayers.Count; j++)
        {
            OtherPlayerData otherPlayer = OtherPlayers[j];
            OtherPlayerDirectionData[j] = PlayerDirectionData.GetUpdatedDirectionData(otherPlayer.DistanceData.Data, Owner, otherPlayer.OtherPlayerComponent);
            OtherPlayerData.Add(otherPlayer);
        }
    }

    public void Execute()
    {
        for (int i = 0; i < OtherPlayerDirectionData.Length; i++)
        {
            var data = OtherPlayerDirectionData[i];
            data.MainData.Update(OwnerPosition);
            data.MainData.UpdateDotProductAndCalcNormal(OwnerViewPosition, OwnerLookDirection);
            OtherPlayerDirectionData[i] = data;
        }
    }

    public void ReadData()
    {
        for (int i = 0; i < OtherPlayerDirectionData.Length; i++)
        {
            OtherPlayerData[i].DistanceData.SetPlayerDirectionData(OtherPlayerDirectionData[i]);
        }
        OtherPlayerData.Clear();
        OtherPlayerDirectionData.Dispose();
    }

    public List<OtherPlayerData> OtherPlayerData = [];
    public NativeArray<PlayerDirectionData> OtherPlayerDirectionData = [];
}

public struct PlayerTickJob : IJobFor
{
    [ReadOnly] public NativeArray<PlayerTickData> Input;
    [WriteOnly] public NativeArray<PlayerTickData> Output;

    public void Execute(int index)
    {
        PlayerTickData Data = Input[index];
        Data.Execute();
        Output[index] = Data;
    }

    public void Dispose()
    {
        if (Input.IsCreated) Input.Dispose();
        if (Output.IsCreated) Output.Dispose();
    }
}

public class DirectionDataJob : BotManagerBase
{
    private JobHandle _PlayerTickJobHandle;
    private PlayerTickJob _PlayerTickJob;
    private readonly List<PlayerTickData> _playerTickData = [];

    public DirectionDataJob(BotManagerComponent botController) : base(botController)
    {
        botController.StartCoroutine(DirectionDataJobLoop());
    }

    private IEnumerator DirectionDataJobLoop()
    {
        yield return null;
        while (GameWorldComponent.Instance != null)
        {
            var players = GameWorldComponent.Instance.PlayerTracker?.AlivePlayerArray;
            if (players == null || players.Count <= 1)
            {
                yield return null;
                continue;
            }

            foreach (PlayerComponent playerComp in players)
            {
                if (playerComp != null && playerComp.OtherPlayersData != null)
                {
                    _playerTickData.Add(playerComp.GetPreparedTickData());
                }
            }

            int jobCount = _playerTickData.Count;
            if (jobCount > 0)
            {
                _PlayerTickJob = new() {
                    Input = new NativeArray<PlayerTickData>(jobCount, Allocator.TempJob),
                    Output = new NativeArray<PlayerTickData>(jobCount, Allocator.TempJob)
                };
                for (int i = 0; i < jobCount; i++)
                {
                    _PlayerTickJob.Input[i] = _playerTickData[i];
                }

                // schedule job and wait for next frame to read data
                _PlayerTickJobHandle = _PlayerTickJob.Schedule(jobCount, new JobHandle());

                yield return null;

                var handle = _PlayerTickJobHandle;
                if (!handle.IsCompleted) handle.Complete();
                _PlayerTickJobHandle = handle;

                for (int i = 0; i < jobCount; i++)
                {
                    PlayerTickData data = _PlayerTickJob.Output[i];
                    data.ReadData();
                    data.Owner.SetTickData(data);
                }
                _PlayerTickJob.Dispose();
                _playerTickData.Clear();
            }
            yield return null;
        }
    }

    public void Dispose()
    {
        _PlayerTickJobHandle.Complete();
        _PlayerTickJob.Dispose();
    }
}