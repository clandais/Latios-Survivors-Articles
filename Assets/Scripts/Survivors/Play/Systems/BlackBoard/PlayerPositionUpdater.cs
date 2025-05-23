﻿using Latios;
using Latios.Transforms;
using Survivors.Play.Authoring;
using Unity.Burst;
using Unity.Entities;

namespace Survivors.Play.Systems.BlackBoard
{
    [BurstCompile]
    public partial struct PlayerPositionUpdater : ISystem
    {
        LatiosWorldUnmanaged m_world;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_world = state.GetLatiosWorldUnmanaged();
            state.RequireForUpdate<PlayerTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var playerPosition = m_world.sceneBlackboardEntity.GetComponentData<PlayerPosition>();


            foreach (var transformAspect in SystemAPI.Query<TransformAspect>()
                         .WithAll<PlayerTag>())
            {
                playerPosition.LastPosition     = playerPosition.Position;
                playerPosition.Position         = transformAspect.worldPosition;
                playerPosition.ForwardDirection = transformAspect.forwardDirection;
            }


            m_world.sceneBlackboardEntity.SetComponentData(playerPosition);
        }
    }
}