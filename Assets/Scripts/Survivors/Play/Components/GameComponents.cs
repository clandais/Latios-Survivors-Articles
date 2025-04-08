using Latios;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;

namespace Survivors.Play.Components
{
    public struct PauseRequestedTag : IComponentData { }
    public struct DeadTag : IComponentData { }


    public struct HitInfos : IComponentData
    {
        public float3 Position;
        public float3 Normal;
    }

    public struct FloorGridConstructedTag : IComponentData { }

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
        public NativeArray<bool>   Walkable;
        public NativeArray<int>    IntegrationField;
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
            new int2(-1, 0), new int2(1, 0), new int2(0, -1), new int2(0, 1), // Cardinal
            new int2(-1, -1), new int2(1, -1), new int2(-1, 1), new int2(1, 1) // Diagonal
        };

        #region Coordinate Conversion

        public int2 WorldToCell(float2 worldPos) => new int2((int)(worldPos.x - MinX) / CellSize, (int)(worldPos.y - MinY) / CellSize);

        public float2 CellToWorld(int2 cellPos) => new float2(cellPos.x * CellSize + MinX, cellPos.y * CellSize + MinY);

        public int2 IndexToCell(int index) => new int2(index % Width, index / Width);

        public int IndexFromCell(int2 cellPos) => cellPos.y * Width + cellPos.x;

        public float2 IndexToWorld(int index) => CellToWorld(IndexToCell(index));
        public int IndexFromWorld(float2 worldPos) => IndexFromCell(WorldToCell(worldPos));
        public int CellToIndex(int2 cellPos) => cellPos.x + cellPos.y * Width;

        #endregion


        #region Debugging

        /// <summary>
        ///     Draw the grid in the editor
        /// </summary>
        /// <param name="grid">
        ///     The grid to draw
        /// </param>
        public static void Draw(FloorGrid grid)
        {
            if (!grid.Walkable.IsCreated || !grid.VectorField.IsCreated || !grid.IntegrationField.IsCreated)
                return; // Grid not ready

            for (var i = 0; i < grid.CellCount; i++)
            {
                var cellWorldPos = grid.IndexToWorld(i);
                var cellCenter = new float3(cellWorldPos.x + grid.CellSize / 2f, 0.1f, cellWorldPos.y + grid.CellSize / 2f);

                // Draw Cell Borders (Walkable = Green, Non-Walkable = Red)
                var borderColor = grid.Walkable[i] ? Color.green : Color.red;
                var halfSize = grid.CellSize / 2f;
                Debug.DrawLine(cellCenter + new float3(-halfSize, 0, -halfSize), cellCenter + new float3(-halfSize, 0, halfSize), borderColor);
                Debug.DrawLine(cellCenter + new float3(halfSize, 0, -halfSize), cellCenter + new float3(halfSize, 0, halfSize), borderColor);
                Debug.DrawLine(cellCenter + new float3(-halfSize, 0, -halfSize), cellCenter + new float3(halfSize, 0, -halfSize), borderColor);
                Debug.DrawLine(cellCenter + new float3(-halfSize, 0, halfSize), cellCenter + new float3(halfSize, 0, halfSize), borderColor);

                // Draw Vector Field (Blue Arrows)
                if (grid.Walkable[i] && grid.IntegrationField[i] != UnreachableIntegrationCost) // Only draw vectors for reachable cells
                {
                    var vector = grid.VectorField[i];
                    // Scale vector for visibility, but not excessively
                    var vectorScale = math.min(grid.CellSize * 0.4f, 1.0f);
                    Debug.DrawLine(
                        cellCenter,
                        cellCenter + new float3(vector.x, 0, vector.y) * vectorScale,
                        Color.blue);
                }
            }
        }

        #endregion


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
    }
}