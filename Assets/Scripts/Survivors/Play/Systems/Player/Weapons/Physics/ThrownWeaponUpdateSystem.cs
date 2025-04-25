using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Latios.Transforms;
using Survivors.Play.Authoring.Player.Weapons;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Burst.Intrinsics;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Player.Weapons.Physics
{
    public partial struct ThrownWeaponUpdateSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_latiosWorldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_latiosWorldUnmanaged = state.GetLatiosWorldUnmanaged();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var dcb = m_latiosWorldUnmanaged.syncPoint.CreateDestroyCommandBuffer();
            var collisionLayer = m_latiosWorldUnmanaged.sceneBlackboardEntity
                .GetCollectionComponent<EnvironmentCollisionLayer>().layer;

            state.Dependency = new ThrownWeaponUpdateJob
            {
                DeltaTime            = SystemAPI.Time.DeltaTime,
                EnvironmentLayer     = collisionLayer,
                DestroyCommandBuffer = dcb.AsParallelWriter(),
                Rng                  = state.GetJobRng(),
                SfxQueue =
                    m_latiosWorldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<SfxSpawnQueue>().SfxQueue,
                SpawnerLookup    = SystemAPI.GetComponentLookup<OneShotSfxSpawner>(true),
                SfxPrefabsLookup = SystemAPI.GetBufferLookup<OneShotSfxElement>(true),
                SpawnerRefLookup = SystemAPI.GetComponentLookup<OneShotSfxSpawnerRef>()
            }.ScheduleParallel(state.Dependency);
        }


        public void OnNewScene(ref SystemState state)
        {
            state.InitSystemRng("ThrownWeaponUpdateSystem");
        }
    }


    [BurstCompile]
    internal partial struct ThrownWeaponUpdateJob : IJobEntity, IJobEntityChunkBeginEnd
    {
        public            DestroyCommandBuffer.ParallelWriter DestroyCommandBuffer;
        [ReadOnly] public CollisionLayer                      EnvironmentLayer;
        [ReadOnly] public float                               DeltaTime;


        [NativeDisableParallelForRestriction] public NativeQueue<SfxSpawnQueue.SfxSpawnData> SfxQueue;
        public                                       SystemRng                               Rng;
        [ReadOnly] public                            ComponentLookup<OneShotSfxSpawnerRef>   SpawnerRefLookup;
        [ReadOnly] public                            ComponentLookup<OneShotSfxSpawner>      SpawnerLookup;
        [ReadOnly] public                            BufferLookup<OneShotSfxElement>         SfxPrefabsLookup;

        void Execute(
            Entity entity,
            [EntityIndexInQuery] int entityIndexInQuery,
            ref WorldTransform transform,
            in ThrownWeaponComponent thrownWeapon,
            in Collider collider,
            in AxeSfxRefs axeSfxRefs
        )
        {
            var transformQvs = transform.worldTransform;
            var stepCount = 4;

            var steppedSpeed = thrownWeapon.Speed / stepCount;
            var steppedRotation = thrownWeapon.RotationSpeed / stepCount;

            for (float i = 0; i < stepCount; i++)
            {
                var collision = Latios.Psyshock.Physics.ColliderCast(
                    in collider,
                    in transformQvs,
                    transformQvs.position + thrownWeapon.Direction * thrownWeapon.Speed * DeltaTime,
                    in EnvironmentLayer,
                    out _,
                    out _);

                if (collision)
                {
                    var spawnerRef = SpawnerRefLookup[axeSfxRefs.HitSpawnerEntity];
                    var sfxPrefab = SfxUtilities.GetSfx(in spawnerRef, in SpawnerLookup, in SfxPrefabsLookup, ref Rng);

                    if (sfxPrefab != Entity.Null)
                        SfxQueue.Enqueue(new SfxSpawnQueue.SfxSpawnData
                        {
                            EventType = ESfxEventType.AxeSwoosh,
                            Position  = transformQvs.position,
                            SfxPrefab = sfxPrefab
                        });

                    //  if (SfxTriggeredTag.HasComponent(axeSfxRefs.HitSpawnerEntity))
                    //  SfxTriggeredTag.SetComponentEnabled(axeSfxRefs.HitSpawnerEntity, true);

                    DestroyCommandBuffer.Add(entity, entityIndexInQuery);

                    return;
                }


                transformQvs.position += thrownWeapon.Direction * steppedSpeed * DeltaTime;
                transformQvs.rotation = math.mul(transformQvs.rotation, Quat.RotateAroundAxis(
                    thrownWeapon.RotationAxis, steppedRotation * DeltaTime));
            }


            transform.worldTransform = transformQvs;
        }

        public bool OnChunkBegin(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask)
        {
            Rng.BeginChunk(unfilteredChunkIndex);
            return true;
        }

        public void OnChunkEnd(in ArchetypeChunk chunk, int unfilteredChunkIndex, bool useEnabledMask,
            in v128 chunkEnabledMask,
            bool chunkWasExecuted) { }
    }
}