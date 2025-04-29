using Latios;
using Latios.Transforms;
using Survivors.GameScope.MonoBehaviours;
using Survivors.Play.Authoring.Player.SFX;
using Unity.Entities;
using VContainer;

namespace Survivors.Play.Systems.SFX
{
    public partial class MainAudioListenerUpdateSystem : SubSystem
    {
        CinemachineBehaviour m_cinemachineBehaviour;

        [Inject]
        public void Construct(CinemachineBehaviour cinemachineBehaviour)
        {
            m_cinemachineBehaviour = cinemachineBehaviour;
        }

        protected override void OnUpdate()
        {
            foreach (var worldTransform in SystemAPI.Query<RefRW<WorldTransform>>()
                         .WithAll<MainAudioListener>())
                worldTransform.ValueRW.worldTransform.position = m_cinemachineBehaviour.transform.position;
        }
    }
}