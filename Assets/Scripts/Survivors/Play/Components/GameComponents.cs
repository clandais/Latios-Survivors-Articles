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


        #region Methods

        
        /// <summary>
        ///       Interpolates the vector field at a given world position.
        /// </summary>
        /// <param name="worldPos">
        ///      The world position to interpolate the vector field at.
        /// </param>
        /// <returns>
        ///      The interpolated vector at the given world position.
        /// </returns>
        public float2 InterpolatedVectorAt(float2 worldPos)
        {
            float relX = worldPos.x - MinX;
            float relY = worldPos.y - MinY;
            
            float fx = relX / CellSize;
            float fy = relY / CellSize;
            
            int ix = (int)math.floor(fx);
            int iy = (int)math.floor(fy);
            
            float fracX = fx - ix;
            float fracY = fy - iy;
            
            int2 cell00 = new int2(ix, iy);
            int2 cell10 = new int2(ix + 1, iy);
            int2 cell01 = new int2(ix, iy + 1);
            int2 cell11 = new int2(ix + 1, iy + 1);
            
            float2 v00 = GetVectorSafe(cell00);
            float2 v10 = GetVectorSafe(cell10);
            float2 v01 = GetVectorSafe(cell01);
            float2 v11 = GetVectorSafe(cell11);
            
            float2 interpX1 = math.lerp(v00, v10, fracX);
            float2 interpX2 = math.lerp(v01, v11, fracX);
            float2 finalVec = math.lerp(interpX1, interpX2, fracY);
            return math.normalizesafe(finalVec);
        }
        
        public float2 GetVectorSafe(int2 cellPos)
        {
            if (cellPos.x < 0 || cellPos.x >= Width || cellPos.y < 0 || cellPos.y >= Height)
            {
                return float2.zero;
            }
            int index = IndexFromCell(cellPos);
            return VectorField[index];
        }
        

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
                // if (grid.Walkable[i] && grid.IntegrationField[i] != UnreachableIntegrationCost) // Only draw vectors for reachable cells
                // {
                    var vector = grid.VectorField[i];
                    // Scale vector for visibility, but not excessively
                    var vectorScale = math.min(grid.CellSize * 0.4f, 1.0f);
                    Debug.DrawLine(
                        cellCenter,
                        cellCenter + new float3(vector.x, 0, vector.y) * vectorScale,
                        grid.Walkable[i] ? Color.blue : Color.magenta);
                // }
            }
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
}