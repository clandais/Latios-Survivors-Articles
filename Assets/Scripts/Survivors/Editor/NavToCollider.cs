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
        List<Vector3>           m_SelectedVerts;
        MeshCollider            m_Collider;
        Collider                m_SelectedCollider;
        NavMeshTriangulation    m_NavMeshTriangulation;
        List<Vector3>           m_RegionVerts;
       // static Vector3          regionCenter = new(30f, -20f, 30f);
      //  static Vector3          regionSize   = new(100f, 50f, 100f);
        static int              colliderLayer;

        void OnEnable()
        {
            m_NavToCollider        = target as Utilities.NavToCollider;
            m_SelectedVerts        = new List<Vector3>();
            m_RegionVerts          = new List<Vector3>();
            m_Collider            = m_NavToCollider.GetComponent<MeshCollider>();
            m_NavMeshTriangulation = NavMesh.CalculateTriangulation();
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
            
            m_Collider.sharedMesh = newMesh;
            m_Collider.convex = false;
        }

        void BuildMeshCollider()
        {
            
            Undo.RecordObject(m_NavToCollider, "Simplify NavMesh");
            Undo.RecordObject(m_Collider, "Build NavMeshCollider");
            
            var vertices = m_NavMeshTriangulation.vertices.ToList();
            var indices  = m_NavMeshTriangulation.indices.ToList();
            
            SimplifyMeshTopology(vertices, indices, .05f);
            
            m_Collider.sharedMesh = null;
            m_Collider.sharedMesh = new Mesh
            {
                name = "NavMeshCollider",
                vertices = vertices.ToArray(),
                triangles = indices.ToArray(),
                bounds    = new Bounds(Vector3.zero, Vector3.one * 1000f),
            };
            
            
            m_Collider.convex = false;
            
            
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

                if (m_SelectedVerts.Contains(vertex)) continue;

                var pointHandleSize = HandleUtility.GetHandleSize(vertex) * 0.04f;
                var pointPickSize = pointHandleSize * 0.7f;

                if (Handles.Button(vertex, Quaternion.identity, pointHandleSize, pointPickSize, Handles.DotHandleCap))
                {
                    m_SelectedVerts.Add(vertex);
                    Repaint();

                    return;
                }
            }


            Handles.color = Color.green;

            foreach (var vertex in m_SelectedVerts)
            {
                var pointHandleSize = HandleUtility.GetHandleSize(vertex) * 0.04f;
                var pointPickSize = pointHandleSize * 0.7f;

                Handles.DotHandleCap(0, vertex, Quaternion.identity, pointPickSize, EventType.Repaint);
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