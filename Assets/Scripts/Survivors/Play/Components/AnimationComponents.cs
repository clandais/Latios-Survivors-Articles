using System;
using Latios.Kinemation;
using Unity.Entities;
using UnityEngine;

namespace Survivors.Play.Components
{
    /// <summary>
    ///     Only relevant for the authoring time.
    /// </summary>
    [Serializable]
    public struct AnimationClipProperty
    {
        public AnimationClip clip;
        public float         speedMultiplier;
    }


    public struct Clips : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> ClipSet;
    }

    public struct DeathClips : IComponentData
    {
        public BlobAssetReference<SkeletonClipSetBlob> ClipSet;
    }

    public struct DeathClipsStates : IComponentData
    {
        public ClipState StateA;
        public ClipState StateB;
        public ClipState StateC;
        public int       ChosenState;
    }

    public struct FourDirectionClipStates : IComponentData
    {
        public ClipState Center;
        public ClipState Down;
        public ClipState Up;
        public ClipState Left;
        public ClipState Right;
    }


    public enum EDirections
    {
        Center,
        Down,
        Up,
        Left,
        Right
    }

    public enum EAction
    {
        Throw,
        OtherStuff
    }

    public struct InertialBlendState : IComponentData
    {
        public readonly float Duration;
        public readonly float VelocityChangeThreshold;
        public          float TimeInCurrentState;
        public          float PreviousDeltaTime;

        public InertialBlendState(float duration,
            float velocityChangeThreshold)
        {
            Duration                = duration;
            VelocityChangeThreshold = velocityChangeThreshold;
            TimeInCurrentState      = 0f;
            PreviousDeltaTime       = 0f;
        }
    }

    /// <summary>
    ///     The state of a clip.
    /// </summary>
    public struct ClipState
    {
        public float PreviousTime;
        public float Time;
        public float SpeedMultiplier;
        public int   EventHash;

        public void Update(float deltaTime)
        {
            PreviousTime =  Time;
            Time         += deltaTime;
        }
    }

    public struct AvatarMasks : IComponentData
    {
        public BlobAssetReference<SkeletonBoneMaskSetBlob> Blob;
    }
}