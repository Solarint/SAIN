using System;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.Classes.EnemyClasses;

public class EnemyList(string name) : List<Enemy>
{
    public enum EBotListSortType
    {
        None,
        ByRealDistance,
        ByTimeSinceSensed,
        ByLastKnownDistance,
        ByPathLength,
        VisiblePathPointDistanceToBot,
        VisiblePathPointDistanceToEnemy,
    }

    public string Name { get; } = name;

    public event Action<bool, float> OnListEmptyOrGetFirst;

    public event Action<bool, float> OnListEmptyOrGetFirstHuman;

    public void Subscribe(ref Action<bool, Enemy> action)
    {
        action += AddOrRemoveEnemy;
    }

    public void Unsubscribe(ref Action<bool, Enemy> action, Enemy enemy)
    {
        action -= AddOrRemoveEnemy;
        this.RemoveEnemy(enemy);
    }

    public void SortBy(EBotListSortType sortingType)
    {
        if (this.Count > 1)
        {
            switch (sortingType)
            {
                case EBotListSortType.ByRealDistance:
                    Sort((x, y) => x.RealDistance.CompareTo(y.RealDistance));
                    break;

                case EBotListSortType.ByTimeSinceSensed:
                    Sort((x, y) => x.TimeSinceLastKnownUpdated.CompareTo(y.TimeSinceLastKnownUpdated));
                    break;

                case EBotListSortType.ByLastKnownDistance:
                    Sort((x, y) => x.KnownPlaces.BotDistanceFromLastKnown.CompareTo(y.KnownPlaces.BotDistanceFromLastKnown));
                    break;

                case EBotListSortType.ByPathLength:
                    Sort((x, y) => x.Path.PathLength.CompareTo(y.Path.PathLength));
                    break;

                case EBotListSortType.VisiblePathPointDistanceToBot:
                    Sort((x, y) => x.VisiblePathPointDistanceToBot.CompareTo(y.VisiblePathPointDistanceToBot));
                    break;

                case EBotListSortType.VisiblePathPointDistanceToEnemy:
                    Sort((x, y) => x.VisiblePathPointDistanceToEnemyLastKnown.CompareTo(y.VisiblePathPointDistanceToEnemyLastKnown));
                    break;

                default:
                    break;
            }
        }
    }

    public void AddOrRemoveEnemy(bool value, Enemy enemy)
    {
        if (value)
        {
            //Logger.LogDebug($"EnemyList {Name} added {enemy.EnemyName} IsAI? {enemy.IsAI}");
            this.AddEnemy(enemy);
        }
        else
        {
            //Logger.LogDebug($"EnemyList {Name} removed {enemy.EnemyName} IsAI? {enemy.IsAI}");
            this.RemoveEnemy(enemy);
        }
    }

    private void sortByLastUpdated()
    {
        this.Sort((x, y) => x.KnownPlaces.TimeSinceLastKnownUpdated.CompareTo(y.KnownPlaces.TimeSinceLastKnownUpdated));
    }

    public Enemy First()
    {
        switch (this.Count)
        {
            case 0: return null;
            case 1: break;
            default:
                sortByLastUpdated();
                break;
        }
        return this[0];
    }

    public void AddEnemy(Enemy enemy)
    {
        this.Add(enemy);

        if (this.Count == 1)
        {
            OnListEmptyOrGetFirst?.Invoke(true, Time.time);
        }

        if (!enemy.IsAI)
        {
            Humans++;
            if (Humans == 1)
            {
                OnListEmptyOrGetFirstHuman?.Invoke(true, Time.time);
            }
        }
        else
        {
            Bots++;
        }
    }

    public void RemoveEnemy(Enemy enemy)
    {
        if (enemy == null)
            return;

        this.Remove(enemy);

        if (!enemy.IsAI)
        {
            Humans--;
            if (Humans == 0)
            {
                OnListEmptyOrGetFirstHuman?.Invoke(false, Time.time);
            }
        }
        else
        {
            Bots--;
        }

        if (Bots < 0)
        {
            Bots = 0;
        }
        if (Humans < 0)
        {
            Humans = 0;
        }

        if (this.Count == 0)
        {
            OnListEmptyOrGetFirst?.Invoke(false, Time.time);
        }
    }

    public int Humans { get; private set; }
    public int Bots { get; private set; }
}