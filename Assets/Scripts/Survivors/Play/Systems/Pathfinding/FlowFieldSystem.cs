using System.Collections.Generic;
using Latios;
using Survivors.Play.Authoring;
using Survivors.Play.Components;
using Survivors.Utilities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;

namespace Survivors.Play.Systems.Pathfinding
{
    [RequireMatchingQueriesForUpdate]
    public partial struct FlowFieldSystem : ISystem
    {
        LatiosWorldUnmanaged m_worldUnmanaged;
        EntityQuery          m_query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            m_worldUnmanaged = state.GetLatiosWorldUnmanaged();
            m_query          = state.Fluent().With<LevelTagAuthoring.LevelTag>().Build();
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var grid = m_worldUnmanaged.sceneBlackboardEntity.GetCollectionComponent<FloorGrid>();


            var playerPos = m_worldUnmanaged.sceneBlackboardEntity.GetComponentData<PlayerPosition>();
            var targetCell = grid.WorldToCell(playerPos.Position.xz);

            // Clamp target cell to be within grid bounds
            targetCell.x = math.clamp(targetCell.x, 0, grid.Width - 1);
            targetCell.y = math.clamp(targetCell.y, 0, grid.Height - 1);
            var targetIndex = grid.IndexFromCell(targetCell);


            var integrationJob = new BuildIntegrationFieldJob
            {
                TargetIndex = targetIndex,
                Grid        = grid
            };

            state.Dependency = integrationJob.Schedule(state.Dependency);

            var vectorFieldJob = new CalculateVectorFieldJob
            {
                Grid = grid
            };

            state.Dependency = vectorFieldJob.ScheduleParallel(grid.CellCount, 128, state.Dependency);
        }


        [BurstCompile]
        struct BuildIntegrationFieldJob : IJob
        {
            [ReadOnly] public int       TargetIndex;
            public            FloorGrid Grid;

            // Cost for moving horizontally or vertically
            const int CardinalCost = 10;

            // Cost for moving diagonally (approx. 10 * sqrt(2))
            const int DiagonalCost = 14;

            [BurstCompile]
            struct QueueElement
            {
                public int Index;
                public int Cost;
            }

            [BurstCompile]
            struct CostComparer : IComparer<QueueElement>
            {
                public int Compare(QueueElement x,
                    QueueElement y) => x.Cost.CompareTo(y.Cost);
            }

            public void Execute()
            {
                for (var i = 0; i < Grid.CellCount; ++i) Grid.IntegrationField[i] = FloorGrid.UnreachableIntegrationCost;


                if (TargetIndex < 0 || TargetIndex >= Grid.CellCount || !Grid.Walkable[TargetIndex])
                {
                    UnityEngine.Debug.LogWarning($"IntegrationFieldJob: Invalid target index {TargetIndex}");

                    return;
                }


                var priorityQueue = new NativeMinHeap<QueueElement, CostComparer>(
                    Grid.Width,
                    Allocator.Temp
                );

                Grid.IntegrationField[TargetIndex] = 0;
                priorityQueue.Enqueue(new QueueElement { Index = TargetIndex, Cost = 0 });

                while (priorityQueue.TryDequeue(out var currentElement))
                {
                    var currentIdx = currentElement.Index;
                    var currentCost = currentElement.Cost;

                    if (currentCost > Grid.IntegrationField[currentIdx]) continue;

                    var currentCell = Grid.IndexToCell(currentIdx);

                    foreach (var direction in FloorGrid.AllDirections)
                    {
                        var neighborCell = currentCell + direction;

                        if (neighborCell.x < 0 || neighborCell.x >= Grid.Width || neighborCell.y < 0 || neighborCell.y >= Grid.Height) continue;

                        var neighborIndex = Grid.IndexFromCell(neighborCell);

                        if (!Grid.Walkable[neighborIndex]) continue;

                        var stepCost = math.abs(direction.x) + math.abs(direction.y) == 1 ? CardinalCost : DiagonalCost;
                        var newCost = currentCost + stepCost;

                        if (newCost < Grid.IntegrationField[neighborIndex])
                        {
                            Grid.IntegrationField[neighborIndex] = newCost;
                            priorityQueue.Enqueue(new QueueElement { Index = neighborIndex, Cost = newCost });
                        }
                    }
                }


                priorityQueue.Dispose();
            }
        }

        [BurstCompile]
        struct CalculateVectorFieldJob : IJobFor
        {
            [NativeDisableParallelForRestriction] // Needed because we write to VectorField
            public FloorGrid Grid;

            public void Execute(int index)
            {
                if (!Grid.Walkable[index] || Grid.IntegrationField[index] == FloorGrid.UnreachableIntegrationCost)
                {
                    Grid.VectorField[index] = float2.zero;

                    return;
                }


                var currentCell = Grid.IndexToCell(index);
                var currentCost = Grid.IntegrationField[index];

                var bestCost = currentCost;
                var bestDir = int2.zero;

                foreach (var direction in FloorGrid.AllDirections)
                {
                    var neighborCell = currentCell + direction;

                    // Bounds Check
                    if (neighborCell.x < 0 || neighborCell.x >= Grid.Width || neighborCell.y < 0 || neighborCell.y >= Grid.Height)
                        continue;

                    var neighborIndex = Grid.IndexFromCell(neighborCell);

                    // Check if neighbor is reachable and has lower cost
                    if (Grid.IntegrationField[neighborIndex] < bestCost)
                    {
                        bestCost = Grid.IntegrationField[neighborIndex];
                        bestDir  = direction;
                    }
                }


                // If the best direction is zero (no lower cost neighbor found, should only happen at target or isolated minima)
                // or if we are at the target itself (cost 0)
                if (math.all(bestDir == int2.zero) || currentCost == 0)
                    Grid.VectorField[index] = float2.zero;
                else
                    Grid.VectorField[index] = math.normalize(bestDir);
            }
        }
    }
}