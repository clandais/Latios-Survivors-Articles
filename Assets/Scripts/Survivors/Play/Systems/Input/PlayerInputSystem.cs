using Latios;
using Survivors.Input;
using Survivors.Play.Authoring;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Systems.Input
{
    [RequireMatchingQueriesForUpdate]
    public partial class PlayerInputSystem : SubSystem
    {
        InputSystem_Actions m_inputActions;
        EntityQuery m_Query;

        protected override void OnCreate()
        {
            m_inputActions = new InputSystem_Actions();
            m_inputActions.Enable();

            m_Query = Fluent
                .With<PlayerInputState>()
                .Build();
        }

        protected override void OnDestroy()
        {
            m_inputActions.Disable();
        }


        protected override void OnUpdate()
        {
            var move = m_inputActions.Player.Move.ReadValue<Vector2>();

            if (math.length(move) > 0.01f) move = math.normalize(move);

            sceneBlackboardEntity.SetComponentData(new PlayerInputState
            {
                Direction = move
            });
        }
    }
}