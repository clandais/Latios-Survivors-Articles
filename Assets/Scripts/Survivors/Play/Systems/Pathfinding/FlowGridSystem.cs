using Latios;
using Latios.Anna;
using Latios.Psyshock;
using Survivors.Play.Authoring;
using Survivors.Play.Authoring.SceneBlackBoard;
using Survivors.Play.Components;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Pathfinding
{
    [BurstCompile]
    public partial struct FlowGridSystem : ISystem, ISystemNewScene
    {
        LatiosWorldUnmanaged m_worldUnmanaged;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
            state.RequireForUpdate<LevelTag>();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var collisionLayerComponent = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<GridCollisionLayer>();

            if (m_worldUnmanaged.sceneBlackboardEntity.HasComponent<FloorGridConstructedTag>())
            {
                state.Enabled = false;
            
                return;
            }


            var settings = m_worldUnmanaged.GetPhysicsSettings();

            var flowFieldSettings = m_worldUnmanaged.sceneBlackboardEntity.GetComponentData<FlowFieldSettings>();
            var grid = new FloorGrid
            {
                CellSize = math.max(1, flowFieldSettings.CellSize)
            };

            var worldAabb = settings.collisionLayerSettings.worldAabb;

            grid.MinX = (int)math.floor(worldAabb.min.x);
            grid.MinY = (int)math.floor(worldAabb.min.z);
            grid.MaxX = (int)math.ceil(worldAabb.max.x);
            grid.MaxY = (int)math.ceil(worldAabb.max.z);

            grid.Width  = math.max(1, (grid.MaxX - grid.MinX) / grid.CellSize);
            grid.Height = math.max(1, (grid.MaxY - grid.MinY) / grid.CellSize);


            if (grid.CellCount <= 0)
            {
                UnityEngine.Debug.LogError($"Invalid grid dimensions: W={grid.Width}, H={grid.Height}. Aborting grid creation.");

                return;
            }


            grid.Walkable         = new NativeArray<bool>(grid.CellCount, Allocator.Persistent);
            grid.IntegrationField = new NativeArray<int>(grid.CellCount, Allocator.Persistent);
            grid.VectorField      = new NativeArray<float2>(grid.CellCount, Allocator.Persistent);

            state.Dependency = new CheckWalkabilityJob
            {
                Grid           = grid,
                CollisionLayer = collisionLayerComponent.Layer
            }.ScheduleParallel(grid.CellCount, 128, state.Dependency);

            m_worldUnmanaged.sceneBlackboardEntity.SetCollectionComponentAndDisposeOld(grid);
            m_worldUnmanaged.sceneBlackboardEntity.AddComponent<FloorGridConstructedTag>();

            UnityEngine.Debug.Log(
                $"Floor Grid Constructed: {grid.Width}x{grid.Height}, CellSize: {grid.CellSize}, Bounds: ({grid.MinX},{grid.MinY})->({grid.MaxX},{grid.MaxY})");
        }

        public void OnNewScene(ref SystemState state)
        {
            // Dispose previous grid if it exists
            if (m_worldUnmanaged.sceneBlackboardEntity.HasCollectionComponent<FloorGrid>())
            {
                var oldGrid = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<FloorGrid>();
                state.Dependency = oldGrid.TryDispose(state.Dependency);
            }


            m_worldUnmanaged.sceneBlackboardEntity.AddOrSetCollectionComponentAndDisposeOld<FloorGrid>(default);
            m_worldUnmanaged.sceneBlackboardEntity.RemoveComponent<FloorGridConstructedTag>();
            state.Enabled = true; // Re-enable the system for the new scene
        }

        [BurstCompile]
        struct CheckWalkabilityJob : IJobFor
        {
            [ReadOnly] public CollisionLayer CollisionLayer;

            public FloorGrid Grid;

            const float RaycastVerticalOffset = 10f;
            const float RaycastDistance       = 20f;

            public void Execute(int index)
            {
                Grid.Walkable[index]         = false;
                Grid.IntegrationField[index] = FloorGrid.UnreachableIntegrationCost;
                Grid.VectorField[index]      = float2.zero;


                var cellCoords = Grid.IndexToCell(index);
                var worldPos = Grid.CellToWorld(cellCoords);

                var cellLeft = new float3(worldPos.x, 0, worldPos.y);
                var cellRight = new float3(worldPos.x + Grid.CellSize, 0, worldPos.y + Grid.CellSize);


                var rayStartLeft = cellLeft + new float3(0, RaycastVerticalOffset, 0);
                var rayStartRight = cellRight + new float3(0, RaycastVerticalOffset, 0);

                var rayDir = math.down();

                //  Cast at two points to check for "walkability"
                if (Latios.Psyshock.Physics.Raycast(rayStartLeft, rayStartLeft + rayDir * RaycastDistance, in CollisionLayer, out _, out _)
                    && Latios.Psyshock.Physics.Raycast(rayStartRight, rayStartRight + rayDir * RaycastDistance, in CollisionLayer, out _, out _))
                    Grid.Walkable[index] = true;
            }
        }
    }
}