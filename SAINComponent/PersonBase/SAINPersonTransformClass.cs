using EFT;
using System.Collections.Generic;
using UnityEngine;

namespace SAIN.SAINComponent.BaseClasses
{
    public class SAINPersonTransformClass
    {
        public SAINPersonTransformClass(SAINPersonClass person)
        {
            Person = person;
        }

        public void Update()
        {
            Head = PartPosition(BodyPartType.head);
            Chest = PartPosition(BodyPartType.body);
            Stomach = PartPosition(PlayerBoneType.Pelvis);
        }

        private readonly SAINPersonClass Person;
        public bool TransformNull => Person.PlayerNull || DefaultTransform == null || Person?.Player?.gameObject == null;
        public BifacialTransform DefaultTransform => Person.IAIDetails.Transform;
        public Vector3 Position => !TransformNull ? DefaultTransform.position : Vector3.zero;
        public Vector3 LookDirection => !TransformNull ? Person.IAIDetails.LookDirection : Vector3.zero;

        public Vector3 Direction(Vector3 start) => !TransformNull ? Position - start : Vector3.zero;

        public Vector3 WeaponRootPosition => WeaponRoot != null ? WeaponRoot.position : Vector3.zero;
        public BifacialTransform WeaponRoot => Person.Player?.WeaponRoot;

        public Vector3 Forward => !TransformNull ? DefaultTransform.forward : Vector3.zero;
        public Vector3 Back => -Forward;
        public Vector3 Right => !TransformNull ? DefaultTransform.right : Vector3.zero;
        public Vector3 Left => -Right;
        public Vector3 Up => !TransformNull ? DefaultTransform.up : Vector3.zero;
        public Vector3 Down => -Up;
        public Vector3 Head { get; private set; }
        public Vector3 Chest { get; private set; }
        public Vector3 Stomach { get; private set; }

        public Dictionary<BodyPartType, BodyPartClass> MainParts => Person.IAIDetails?.MainParts;

        public Dictionary<PlayerBoneType, BifacialTransform> AllParts => Person.IAIDetails?.PlayerBones?.BifacialTransforms;

        public Vector3 PartPosition(BodyPartType part)
        {
            if (MainParts != null && MainParts.ContainsKey(part))
            {
                return MainParts[part].Position;
            }
            else
            {
                return Position;
            }
        }

        public Vector3 PartPosition(PlayerBoneType part)
        {
            if (AllParts != null && AllParts.ContainsKey(part))
            {
                return AllParts[part].position;
            }
            else
            {
                return Position;
            }
        }
    }
}