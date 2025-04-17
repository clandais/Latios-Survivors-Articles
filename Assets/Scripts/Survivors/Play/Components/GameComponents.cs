using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Components
{
    public struct PauseRequestedTag : IComponentData
    {
    }

    public struct DeadTag : IComponentData
    {
    }


    public struct HitInfos : IComponentData
    {
        public float3 Position;
        public float3 Normal;
    }

    public partial struct SfxSpawnQueue : ICollectionComponent
    {
        public struct SfxSpawnData
        {
            public int EventHash;
            public EntityWith<Prefab> SfxPrefab;
            public float3 Position;
        }

        public NativeQueue<SfxSpawnData> SfxQueue;

        public JobHandle TryDispose(JobHandle inputDeps)
        {
            if (!SfxQueue.IsCreated)
                return inputDeps;

            return SfxQueue.Dispose(inputDeps);
        }
    }

    #region Navigation

    public struct FloorGridConstructedTag : IComponentData
    {
    }

    /// <summary>
    ///     Settings for the flow field system.
    /// </summary>
    public struct FlowFieldSettings : IComponentData
    {
        public int CellSize;
    }


    /// <summary>
    ///     Represents a grid of cells for pathfinding.
    /// </summary>
    public partial struct FloorGrid : ICollectionComponent
    {
        public NativeArray<bool> Walkable;
        public NativeArray<int> IntegrationField;
        public NativeArray<float2> VectorField;

        public int Width;
        public int Height;

        public int CellSize;
        public int CellCount => Width * Height;

        public int MinX;
        public int MinY;
        public int MaxX;
        public int MaxY;

        public const int UnreachableIntegrationCost = int.MaxValue;

        public static readonly int2[] AllDirections =
        {
            new(-1, 0), new(1, 0), new(0, -1), new(0, 1), // Cardinal
            new(-1, -1), new(1, -1), new(-1, 1), new(1, 1) // Diagonal
        };

        #region Coordinate Conversion

        public int2 WorldToCell(float2 worldPos)
        {
            return new int2((int)(worldPos.x - MinX) / CellSize, (int)(worldPos.y - MinY) / CellSize);
        }

        public float2 CellToWorld(int2 cellPos)
        {
            return new float2(cellPos.x * CellSize + MinX, cellPos.y * CellSize + MinY);
        }

        public int2 IndexToCell(int index)
        {
            return new int2(index % Width, index / Width);
        }

        public int IndexFromCell(int2 cellPos)
        {
            return cellPos.y * Width + cellPos.x;
        }

        public float2 IndexToWorld(int index)
        {
            return CellToWorld(IndexToCell(index));
        }

        public int IndexFromWorld(float2 worldPos)
        {
            return IndexFromCell(WorldToCell(worldPos));
        }

        public int CellToIndex(int2 cellPos)
        {
            return cellPos.x + cellPos.y * Width;
        }

        #endregion


        #region Interface Implementation

        /// <summary>
        ///     Interface implementation for collection component.
        /// </summary>
        /// <param name="inputDeps">
        ///     The job handle to combine with the dispose job.
        /// </param>
        /// <returns>
        ///     The combined job handle.
        /// </returns>
        public JobHandle TryDispose(JobHandle inputDeps)
        {
            var combinedDeps = inputDeps;
            if (Walkable.IsCreated)
                combinedDeps = JobHandle.CombineDependencies(combinedDeps, Walkable.Dispose(combinedDeps));
            if (IntegrationField.IsCreated)
                combinedDeps = JobHandle.CombineDependencies(combinedDeps, IntegrationField.Dispose(combinedDeps));
            if (VectorField.IsCreated)
                combinedDeps = JobHandle.CombineDependencies(combinedDeps, VectorField.Dispose(combinedDeps));

            return combinedDeps;
        }

        #endregion
    }


    public struct VectorFieldAspect : ICollectionAspect<VectorFieldAspect>
    {
        [ReadOnly] public FloorGrid Grid;

        public VectorFieldAspect CreateCollectionAspect(LatiosWorldUnmanaged latiosWorld,
            EntityManager entityManager,
            Entity entity)
        {
            var grid = latiosWorld.sceneBlackboardEntity.GetCollectionComponent<FloorGrid>();
            return new VectorFieldAspect
            {
                Grid = grid
            };
        }

        public FluentQuery AppendToQuery(FluentQuery query)
        {
            return query.With<FloorGrid.ExistComponent>(true);
        }

        #region Methods

        /// <summary>
        ///     Interpolates the vector field at a given world position.
        /// </summary>
        /// <param name="worldPos">
        ///     The world position to interpolate the vector field at.
        /// </param>
        /// <returns>
        ///     The interpolated vector at the given world position.
        /// </returns>
        public float2 InterpolatedVectorAt(float2 worldPos)
        {
            var relX = worldPos.x - Grid.MinX;
            var relY = worldPos.y - Grid.MinY;

            var fx = relX / Grid.CellSize;
            var fy = relY / Grid.CellSize;

            var ix = (int)math.floor(fx);
            var iy = (int)math.floor(fy);

            var fracX = fx - ix;
            var fracY = fy - iy;

            var cell00 = new int2(ix, iy);
            var cell10 = new int2(ix + 1, iy);
            var cell01 = new int2(ix, iy + 1);
            var cell11 = new int2(ix + 1, iy + 1);

            var v00 = GetVectorSafe(cell00);
            var v10 = GetVectorSafe(cell10);
            var v01 = GetVectorSafe(cell01);
            var v11 = GetVectorSafe(cell11);

            var interpX1 = math.lerp(v00, v10, fracX);
            var interpX2 = math.lerp(v01, v11, fracX);
            var finalVec = math.lerp(interpX1, interpX2, fracY);
            return math.normalizesafe(finalVec);
        }

        public float2 GetVectorSafe(int2 cellPos)
        {
            if (cellPos.x < 0 || cellPos.x >= Grid.Width || cellPos.y < 0 || cellPos.y >= Grid.Height)
                return float2.zero;
            var index = Grid.IndexFromCell(cellPos);
            return Grid.VectorField[index];
        }

        #endregion

        #region Debugging

        /// <summary>
        ///     Draw the grid in the editor
        /// </summary>
        public void Draw()
        {
            if (!Grid.Walkable.IsCreated || !Grid.VectorField.IsCreated || !Grid.IntegrationField.IsCreated)
                return; // Grid not ready

            for (var i = 0; i < Grid.CellCount; i++)
            {
                var cellWorldPos = Grid.IndexToWorld(i);
                var cellCenter = new float3(cellWorldPos.x + Grid.CellSize / 2f, 0.1f,
                    cellWorldPos.y + Grid.CellSize / 2f);

                // Draw Cell Borders (Walkable = Green, Non-Walkable = Red)
                var borderColor = Grid.Walkable[i] ? Color.green : Color.red;
                var halfSize = Grid.CellSize / 2f;
                Debug.DrawLine(cellCenter + new float3(-halfSize, 0, -halfSize),
                    cellCenter + new float3(-halfSize, 0, halfSize), borderColor);
                Debug.DrawLine(cellCenter + new float3(halfSize, 0, -halfSize),
                    cellCenter + new float3(halfSize, 0, halfSize), borderColor);
                Debug.DrawLine(cellCenter + new float3(-halfSize, 0, -halfSize),
                    cellCenter + new float3(halfSize, 0, -halfSize), borderColor);
                Debug.DrawLine(cellCenter + new float3(-halfSize, 0, halfSize),
                    cellCenter + new float3(halfSize, 0, halfSize), borderColor);

                // Draw Vector Field (Blue Arrows)
                // if (grid.Walkable[i] && grid.IntegrationField[i] != UnreachableIntegrationCost) // Only draw vectors for reachable cells
                // {
                var vector = Grid.VectorField[i];
                // Scale vector for visibility, but not excessively
                var vectorScale = math.min(Grid.CellSize * 0.4f, 1.0f);
                Debug.DrawLine(
                    cellCenter,
                    cellCenter + new float3(vector.x, 0, vector.y) * vectorScale,
                    Grid.Walkable[i] ? Color.blue : Color.magenta);
                // }
            }
        }

        #endregion
    }

    #endregion
}