using Latios;
using Survivors.GameScope.MonoBehaviours;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using VContainer;

namespace Survivors.Play.Systems.Camera
{
    public partial class CinemachineTargetUpdater : SubSystem
    {
        CinemachineBehaviour m_cinemachine;

        [Inject]
        public void Construct(CinemachineBehaviour cinemachine)
        {
            m_cinemachine = cinemachine;
        }

        protected override void OnCreate()
        {
            RequireForUpdate<PlayerTag>();
        }

        protected override void OnUpdate()
        {
            var playerPosition = sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            var inputState = sceneBlackboardEntity.GetComponentData<PlayerInputState>();
            m_cinemachine.SetTargetsPositions(playerPosition.Position, inputState.MousePosition);
        }
    }
}