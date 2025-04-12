using Latios;
using Survivors.Input;
using Survivors.Play.Authoring;
using Survivors.Play.Scope;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using VContainer;
using VitalRouter;

namespace Survivors.Play.Systems.Input
{
    [RequireMatchingQueriesForUpdate]
    public partial class PlayerInputSystem : SubSystem
    {
        bool _attackTriggered;

        ICommandPublisher   m_commandPublisher;
        InputSystem_Actions m_inputActions;
        UnityEngine.Camera  m_mainCamera;
        EntityQuery         m_Query;

        [Inject]
        public void Construct(ICommandPublisher commandPublisher)
        {
            m_commandPublisher = commandPublisher;
        }

        protected override void OnCreate()
        {
            m_inputActions = new InputSystem_Actions();
            m_inputActions.Enable();

            m_Query = Fluent
                .With<PlayerInputState>()
                .Build();

            m_mainCamera = UnityEngine.Camera.main;
        }

        protected override void OnDestroy()
        {
            m_inputActions.Disable();
        }


        void AttackPerformed(InputAction.CallbackContext _)
        {
            _attackTriggered = true;
        }


        protected override void OnStartRunning()
        {
            m_inputActions.Player.Attack.performed += AttackPerformed;
        }

        protected override void OnStopRunning()
        {
            m_inputActions.Player.Attack.performed -= AttackPerformed;
        }

        protected override void OnUpdate()
        {
            var inputState = new PlayerInputState();

            var move = m_inputActions.Player.Move.ReadValue<Vector2>();
            if (math.length(move) > 0.01f) move = math.normalize(move);

            inputState.Direction = move;

            float2 mousePosition = m_inputActions.Player.MousePosition.ReadValue<Vector2>();
            
            float mouseScroll = m_inputActions.Player.Scroll.ReadValue<float>();

           
            
            m_commandPublisher.PublishAsync(new MousePositionCommand
            {
                MousePosition = mousePosition
            });

            m_commandPublisher.PublishAsync(new MouseScrollCommand
            {
                ScrollDelta = mouseScroll
            });

            var ray = m_mainCamera.ScreenPointToRay(new float3(mousePosition.x, mousePosition.y, 0f));
            var plane = new Plane(Vector3.up, Vector3.zero);

            if (plane.Raycast(ray, out var enter))
            {
                float3 hitPoint = ray.GetPoint(enter);
                inputState.MousePosition = new float3(hitPoint.x, 0f, hitPoint.z);
            }


            inputState.AttackTriggered = _attackTriggered;

            sceneBlackboardEntity.SetComponentData(inputState);

            _attackTriggered = false;
        }
    }
}