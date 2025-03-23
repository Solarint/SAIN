using HarmonyLib;
using SAIN.Components;
using SAIN.Components.PlayerComponentSpace;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using static WindowsManager;

namespace SAIN.SAINComponent.Classes.EnemyClasses
{
    public class EnemyVisionClass : EnemyBase, IBotEnemyClass
    {
        static EnemyVisionClass()
        {
            _bodyPartField = AccessTools.Field(typeof(EnemyInfo), "_bodyPart");
            _headPartField = AccessTools.Field(typeof(EnemyInfo), "_headPart");
        }

        private static FieldInfo _bodyPartField;
        private static FieldInfo _headPartField;

        private const float _repeatContactMinSeenTime = 12f;
        private const float _lostContactMinSeenTime = 12f;

        public float EnemyVelocity => EnemyTransform.VelocityMagnitudeNormal;
        public bool FirstContactOccured { get; private set; }
        public bool ShallReportRepeatContact { get; set; }
        public bool ShallReportLostVisual { get; set; }
        public bool InLineOfSight => VisionChecker.LineOfSight;
        public bool IsVisible { get; private set; }
        public bool CanShoot { get; private set; }
        public float VisibleStartTime { get; private set; }
        public float TimeSinceSeen => Seen ? Time.time - TimeLastSeen : -1f;
        public bool Seen { get; private set; }
        public float TimeFirstSeen { get; private set; }
        public float TimeLastSeen { get; private set; }
        public float LastChangeVisionTime { get; private set; }
        public float LastGainSightResult { get; set; }
        public float GainSightCoef => _gainSight.GainSightModifier;
        public float VisionDistance => _visionDistance.Value;

        public EnemyAnglesClass Angles { get; }
        public EnemyVisionChecker VisionChecker { get; }

        public KeyValuePair<EnemyPart, EnemyPartData> _bodyPart;
        public KeyValuePair<EnemyPart, EnemyPartData> _headPart;

        public EnemyVisionClass(Enemy enemy) : base(enemy)
        {
            Angles = new EnemyAnglesClass(enemy);
            _gainSight = new EnemyGainSightClass(enemy);
            _visionDistance = new EnemyVisionDistanceClass(enemy);
            VisionChecker = new EnemyVisionChecker(enemy);
        }

        public void Init()
        {
            _bodyPart = (KeyValuePair<EnemyPart, EnemyPartData>)_bodyPartField.GetValue(Enemy.EnemyInfo);
            _headPart = (KeyValuePair<EnemyPart, EnemyPartData>)_headPartField.GetValue(Enemy.EnemyInfo);
            Enemy.Events.OnEnemyKnownChanged.OnToggle += OnEnemyKnownChanged;
            Angles.Init();
            VisionChecker.Init();
        }

        public void Update()
        {
            VisionChecker.Update();
            Angles.Update();
            updateVision();
        }

        public void Dispose()
        {
            Enemy.Events.OnEnemyKnownChanged.OnToggle -= OnEnemyKnownChanged;
            Angles.Dispose();
            VisionChecker.Dispose();
        }

        public void OnEnemyKnownChanged(bool known, Enemy enemy)
        {
            if (known) {
                return;
            }
            UpdateVisibleState(true);
            UpdateCanShootState(true);
        }

        private void updateVision()
        {
            UpdateVisibleState(false);
            UpdateCanShootState(false);
        }

        public void UpdateVisibleState(bool forceOff)
        {
            bool wasVisible = IsVisible;
            if (forceOff)
                IsVisible = false;
            else
                IsVisible = EnemyInfo.IsVisible && Angles.CanBeSeen;

            if (IsVisible) {
                if (!wasVisible) {
                    VisibleStartTime = Time.time;
                    if (Seen && TimeSinceSeen >= _repeatContactMinSeenTime) {
                        ShallReportRepeatContact = true;
                    }
                }
                if (!Seen) {
                    FirstContactOccured = true;
                    TimeFirstSeen = Time.time;
                    Seen = true;
                    Enemy.Events.EnemyFirstSeen();
                }

                TimeLastSeen = Time.time;
                Enemy.UpdateCurrentEnemyPos(EnemyTransform.Position);
            }

            if (!IsVisible) {
                if (wasVisible)
                    Enemy.UpdateLastSeenPosition(EnemyTransform.Position);

                if (Seen &&
                    TimeSinceSeen > _lostContactMinSeenTime &&
                    _nextReportLostVisualTime < Time.time) {
                    _nextReportLostVisualTime = Time.time + 20f;
                    ShallReportLostVisual = true;
                }
                VisibleStartTime = -1f;
            }

            Enemy.Events.OnVisionChange.CheckToggle(IsVisible);
            if (IsVisible != wasVisible) {
                LastChangeVisionTime = Time.time;
            }
        }

        public void UpdateCanShootState(bool forceOff)
        {
            if (forceOff) {
                CanShoot = false;
                return;
            }
            CanShoot = EnemyInfo?.CanShoot == true;
        }

        private readonly EnemyGainSightClass _gainSight;
        private readonly EnemyVisionDistanceClass _visionDistance;
        private float _nextReportLostVisualTime;
    }
}