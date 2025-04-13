using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.Profiling;

namespace Survivors.Editor
{
    [CustomEditor(typeof(Utilities.NavToCollider))]
    public class NavToCollider : UnityEditor.Editor
    {
        Utilities.NavToCollider m_NavToCollider;
        // List<Vector3>           m_SelectedVerts;
        MeshCollider            m_Collider;
        // Collider                m_SelectedCollider;
        NavMeshTriangulation    m_NavMeshTriangulation;
        List<Vector3>           m_RegionVerts;
       // static Vector3          regionCenter = new(30f, -20f, 30f);
      //  static Vector3          regionSize   = new(100f, 50f, 100f);
        static  int            colliderLayer;
        // private List<Collider> m_Colliders;

        void OnEnable()
        {
            m_NavToCollider        = target as Utilities.NavToCollider;
            // m_SelectedVerts        = new List<Vector3>();
            m_RegionVerts          = new List<Vector3>();
            m_Collider             = m_NavToCollider.GetComponent<MeshCollider>();
            m_NavMeshTriangulation = NavMesh.CalculateTriangulation();
            // m_Colliders            = m_NavToCollider.GetComponentsInChildren<Collider>(true).ToList();
            Tools.hidden           = true;
        }

        public void OnDisable()
        {
            Tools.hidden = false;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.Space();
            EditorGUILayout.Space();

            EditorGUI.BeginChangeCheck();
     //       regionCenter = EditorGUILayout.Vector3Field("Region Center", regionCenter);
      //      regionSize   = EditorGUILayout.Vector3Field("Region Size", regionSize);
            if (EditorGUI.EndChangeCheck()) SceneView.RepaintAll();
            colliderLayer = EditorGUILayout.LayerField("Collider Layer", colliderLayer);

            EditorGUILayout.Space();
            if (GUILayout.Button("Show NavMesh Vertices")) ShowVertices();
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Create Box from Vertices"))
            {
                GenBoxCollider();
            }
            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("Clear Children")) 
            {
                for (int i = 0; i < m_NavToCollider.transform.childCount; i++)
                {
                    DestroyImmediate(m_NavToCollider.transform.GetChild(i).gameObject);
                }
            }
            

            EditorGUILayout.Space();
            if (GUILayout.Button("Build MeshCollider")) BuildMeshCollider();
            
            EditorGUILayout.Space();
            if (GUILayout.Button("Extrude MeshCollider")) ExtrudeMeshCollider();

        }

        void ExtrudeMeshCollider()
        {
            var mesh = m_Collider.sharedMesh;
            if (mesh == null) return;
            
            Undo.RecordObject(m_Collider, "Extrude NavMeshCollider");
            var newMesh = new Mesh();
            newMesh.name = "ExtrudedNavMeshCollider";
            
            Matrix4x4[ ] extMatrix = { Matrix4x4.identity, Matrix4x4.Translate(5f * Vector3.down) };
            
            MeshExtrusion.ExtrudeMesh(mesh, newMesh, extMatrix, false );
            
            mesh.RecalculateNormals();
            
            m_Collider.sharedMesh = newMesh;
            m_Collider.convex = false;
            
            
        }

        void BuildMeshCollider()
        {
            m_NavMeshTriangulation = NavMesh.CalculateTriangulation();
            
            Undo.RecordObject(m_NavToCollider, "Simplify NavMesh");
            Undo.RecordObject(m_Collider, "Build NavMeshCollider");
            
            var vertices = m_NavMeshTriangulation.vertices.ToList();
            var indices  = m_NavMeshTriangulation.indices.ToList();
            
           // SimplifyMeshTopology(vertices, indices, .05f);
            
            m_Collider.sharedMesh = null;
            
            var mesh = new Mesh
            {
                name      = "NavMeshCollider",
                vertices  = vertices.ToArray(),
                triangles = indices.ToArray(),
                bounds    = new Bounds(Vector3.zero, Vector3.one * 1000f),
            };
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            m_Collider.sharedMesh = mesh;
            m_Collider.convex = false;
            
            
        }
        
        public float boxHeight = 1.0f;
        public float voxelSize = 1; 
        
        private void GenBoxCollider(bool xAxis = false)
        {





            for (int i = 0; i < m_NavToCollider.transform.childCount; i++)
            {
                DestroyImmediate(m_NavToCollider.transform.GetChild(i).gameObject);
            }


            
            
            // 2. Get NavMesh data
            NavMeshTriangulation triangulation = NavMesh.CalculateTriangulation();
            
            
            if (triangulation.vertices == null || triangulation.vertices.Length == 0)
            {
                Debug.LogWarning("No NavMesh data found.");
                return;
            }
            
            // 3. Calculate Bounds
            Bounds bounds = new Bounds(triangulation.vertices[0], Vector3.zero);
            foreach (Vector3 vertex in triangulation.vertices)
            {
                bounds.Encapsulate(vertex);
            }
            // Adjust bounds slightly
            bounds.Expand(voxelSize*2f);
           
            int gridWidth = Mathf.CeilToInt((bounds.max.x - bounds.min.x) / voxelSize);
            int gridHeight = Mathf.CeilToInt((bounds.max.z - bounds.min.z) / voxelSize);
            
            bool[,] occupiedGrid = new bool[gridWidth, gridHeight];
            
            
            
            for (float y = bounds.min.z; y < bounds.max.z; y+=voxelSize)
            {
                for (float x = bounds.min.x;  x <bounds.max.x; x += voxelSize)
                {
                    var samplePos = new Vector3(x + voxelSize * .5f, 0, y + voxelSize * .5f);
                    
                    Debug.DrawLine(samplePos, samplePos + Vector3.up * voxelSize, Color.red, 5f);
                    
                    if (NavMesh.SamplePosition(samplePos, out NavMeshHit hit, voxelSize, NavMesh.AllAreas))
                    {

                        var gridX = Mathf.FloorToInt((x - bounds.min.x) / voxelSize);
                        var gridY = Mathf.FloorToInt((y - bounds.min.z) / voxelSize);
                        
                        occupiedGrid[gridX, gridY] = true;
                        

                        
                        // if (hit.distance > voxelSize*.8f) continue;
                        //
                        // var go = new GameObject($"voxel {x} {y}");
                        // go.transform.parent = m_NavToCollider.transform;
                        // BoxCollider boxCollider = go.AddComponent<BoxCollider>( );
                        // boxCollider.center = new Vector3(samplePos.x, -boxHeight*.5f, samplePos.z);
                        // boxCollider.size   = new Vector3(voxelSize, boxHeight, voxelSize);
                    }
                }
            }
            
            
            // count occupied cells
            int occupiedCount = 0;
            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                    if (occupiedGrid[x, y])
                    {
                        occupiedCount++;
                    }
                }
            }
            
            Debug.Log($"Occupied cells: {occupiedCount} / {gridWidth * gridHeight}");
            
            
            bool[,] processedGrid = new bool[gridWidth, gridHeight];
            List<RectInt> foundRectangles = new List<RectInt>();

            for (int y = 0; y < gridHeight; y++)
            {
                for (int x = 0; x < gridWidth; x++)
                {
                   // Is this an unprocessed, occupied cell?
                    if (occupiedGrid[x, y] && !processedGrid[x, y])
                    {
                        // Found a potential top-left corner

                        // --- Expand Right ---
                        int currentWidth = 0;
                        for (int cx = x; cx < gridWidth; cx++)
                        {
                            if (occupiedGrid[cx, y] && !processedGrid[cx, y])
                            {
                                currentWidth++;
                            }
                            else
                            {
                                break; // Hit boundary or non-valid cell
                            }
                        }

                        // --- Expand Down ---
                        int currentHeight = 0;
                        for (int cy = y; cy < gridHeight; cy++)
                        {
                            bool rowIsValid = true;
                            // Check all cells in this row across the determined width
                            for (int cx = x; cx < x + currentWidth; cx++)
                            {
                                if (!occupiedGrid[cx, cy] || processedGrid[cx, cy])
                                {
                                    rowIsValid = false;
                                    break; // This row cannot be part of the rectangle
                                }
                            }

                            if (rowIsValid)
                            {
                                currentHeight++;
                            }
                            else
                            {
                                break; // Cannot expand further down
                            }
                        }

                        // --- Store Rectangle and Mark Processed ---
                        RectInt newRect = new RectInt(x, y, currentWidth, currentHeight);
                        foundRectangles.Add(newRect);

                        for (int py = y; py < y + currentHeight; py++)
                        {
                            for (int px = x; px < x + currentWidth; px++)
                            {
                                processedGrid[px, py] = true;
                            }
                        }
                    }
                }
                
                
            }


            foreach (RectInt rectangle in foundRectangles)
            {
                var centerXgrid = rectangle.x + rectangle.width / 2f;
                var centerYgrid = rectangle.y + rectangle.height / 2f;
                
                var centerX = bounds.min.x + centerXgrid * voxelSize;
                var centerY = bounds.min.z + centerYgrid * voxelSize;
                
                var worldSizeX = rectangle.width * voxelSize;
                var worldSizeZ = rectangle.height * voxelSize;
                var worldSizeY = boxHeight;
                
                var go = new GameObject($"voxel {rectangle.x} {rectangle.y}");
                go.transform.parent = m_NavToCollider.transform;
                BoxCollider boxCollider = go.AddComponent<BoxCollider>( );
                boxCollider.center = new Vector3(centerX, -boxHeight*.5f, centerY);
                boxCollider.size   = new Vector3(worldSizeX, worldSizeY, worldSizeZ);
                
            }
        }


        void OnSceneGUI()
        {

            if (Event.current.type == EventType.Repaint)
            {
                OnShowVerts();
            }
        }

        void ShowVertices()
        {
            m_RegionVerts.Clear();
            foreach (var vertex in m_NavMeshTriangulation.vertices)
                    m_RegionVerts.Add(vertex);


            SceneView.RepaintAll();
        }

        
        void OnShowVerts()
        {
            Handles.color = Color.blue;

            for (var i = 0; i < m_RegionVerts.Count; i++)
            {
                var vertex = m_RegionVerts[i];
                

                var pointHandleSize = HandleUtility.GetHandleSize(vertex) * 0.04f;
                var pointPickSize = pointHandleSize * 0.7f;

                if (Handles.Button(vertex, Quaternion.identity, pointHandleSize, pointPickSize, Handles.DotHandleCap))
                {
                    Repaint();

                    return;
                }
            }

            
        }
        
        
        public static void SimplifyMeshTopology(List<Vector3> vertices, List<int> indices, float weldThreshold)
        {
            Profiler.BeginSample("SimplifyNavMeshTopology");
            float startTime = Time.realtimeSinceStartup;

            int startingVerts = vertices.Count;
           
            // Put vertex indices into buckets based on their position
            Dictionary<Vector3Int, List<int>> vertexBuckets = new Dictionary<Vector3Int, List<int>>(vertices.Count);
            Dictionary<int, int> shiftedIndices = new Dictionary<int, int>(indices.Count);
            List<Vector3> uniqueVertices = new List<Vector3>();
            int weldThresholdMultiplier = Mathf.RoundToInt(1 / weldThreshold);

            // Heuristic for skipping indices that relies on the fact that the first time a vertex index appears on the indices array, it will always be the highest-numbered
            // index up to that point (e.g. if there's a 5 in the indices array, all the indices to the left of it are in the range [0, 4])
            int minRepeatedIndex = 0; 

            for (int i = 0; i < vertices.Count; ++i)
            {
                var currentVertex = Vector3Int.RoundToInt(vertices[i] * weldThresholdMultiplier);
                if (vertexBuckets.TryGetValue(currentVertex, out var indexRefs))
                {
                    indexRefs.Add(i);
                    shiftedIndices[i] = shiftedIndices[indexRefs[0]];
                    if (minRepeatedIndex == 0)
                    {
                        minRepeatedIndex = i;
                    }
                }
                else
                {
                    indexRefs = new List<int> {i};
                    vertexBuckets.Add(currentVertex, indexRefs);
                    shiftedIndices[i] = uniqueVertices.Count;
                    uniqueVertices.Add(vertices[i]);
                }
            }
           
            // Walk indices array and replace any repeated vertex indices with their corresponding unique one
            for (int i = 0; i < indices.Count; ++i)
            {
                var currentIndex = indices[i];
                if (currentIndex < minRepeatedIndex)
                {
                    // Can't be a repeated index, skip.
                    continue;
                }
               
                indices[i] = shiftedIndices[currentIndex];
            }
           
            vertices.Clear();
            vertices.AddRange(uniqueVertices);
           
            Debug.Log($"Finished simplifying mesh topology. Time: {Time.realtimeSinceStartup - startTime}. initVerts: {startingVerts}, endVerts: {vertices.Count}");
            Profiler.EndSample();
        }
    }
}