using System.Collections;
using UnityEngine;

namespace Survivors.Editor
{
	/// <summary>
	///     <see href="https://github.com/nickhall/Unity-Procedural/blob/master/Splines/Assets/Plugins/MeshExtrusion.cs" />
	/// </summary>
	public class MeshExtrusion
    {
        public class Edge
        {
            // The indiex to each vertex
            public int[] vertexIndex = new int[2];

            // The index into the face.
            // (faceindex[0] == faceindex[1] means the edge connects to only one triangle)
            public int[] faceIndex = new int[2];
        }

        public static void ExtrudeMesh(Mesh srcMesh,
            Mesh extrudedMesh,
            Matrix4x4[] extrusion,
            bool invertFaces)
        {
            var edges = BuildManifoldEdges(srcMesh);
            ExtrudeMesh(srcMesh, extrudedMesh, extrusion, edges, invertFaces);
        }

        public static void ExtrudeMesh(Mesh srcMesh,
            Mesh extrudedMesh,
            Matrix4x4[] extrusion,
            Edge[] edges,
            bool invertFaces)
        {
            var extrudedVertexCount = edges.Length * 2 * extrusion.Length;
            var triIndicesPerStep = edges.Length * 6;
            var extrudedTriIndexCount = triIndicesPerStep * (extrusion.Length - 1);

            var inputVertices = srcMesh.vertices;
            var inputUV = srcMesh.uv;
            var inputTriangles = srcMesh.triangles;

            var vertices = new Vector3[extrudedVertexCount + srcMesh.vertexCount * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[extrudedTriIndexCount + inputTriangles.Length * 2];

            // Build extruded vertices
            var v = 0;

            for (var i = 0; i < extrusion.Length; i++)
            {
                var matrix = extrusion[i];
                var vcoord = (float)i / (extrusion.Length - 1);

                foreach (var e in edges)
                {
                    vertices[v + 0] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[0]]);
                    vertices[v + 1] = matrix.MultiplyPoint(inputVertices[e.vertexIndex[1]]);

                    // skip uv generation for now
                    // uvs[v + 0] = new Vector2(inputUV[e.vertexIndex[0]].x, vcoord);
                    // uvs[v + 1] = new Vector2(inputUV[e.vertexIndex[1]].x, vcoord);

                    v += 2;
                }
            }


            // Build cap vertices
            // * The bottom mesh we scale along it's negative extrusion direction. This way extruding a half sphere results in a capsule.
            for (var c = 0; c < 2; c++)
            {
                var matrix = extrusion[c == 0 ? 0 : extrusion.Length - 1];
                var firstCapVertex = c == 0 ? extrudedVertexCount : extrudedVertexCount + inputVertices.Length;

                for (var i = 0; i < inputVertices.Length; i++)
                {
                    vertices[firstCapVertex + i] = matrix.MultiplyPoint(inputVertices[i]);
                    // uvs[firstCapVertex + i]      = inputUV[i];
                }
            }


            // Build extruded triangles
            for (var i = 0; i < extrusion.Length - 1; i++)
            {
                var baseVertexIndex = edges.Length * 2 * i;
                var nextVertexIndex = edges.Length * 2 * (i + 1);

                for (var e = 0; e < edges.Length; e++)
                {
                    var triIndex = i * triIndicesPerStep + e * 6;

                    triangles[triIndex + 0] = baseVertexIndex + e * 2;
                    triangles[triIndex + 1] = nextVertexIndex + e * 2;
                    triangles[triIndex + 2] = baseVertexIndex + e * 2 + 1;
                    triangles[triIndex + 3] = nextVertexIndex + e * 2;
                    triangles[triIndex + 4] = nextVertexIndex + e * 2 + 1;
                    triangles[triIndex + 5] = baseVertexIndex + e * 2 + 1;
                }
            }


            // build cap triangles
            var triCount = inputTriangles.Length / 3;
            // Top
            {
                var firstCapVertex = extrudedVertexCount;
                var firstCapTriIndex = extrudedTriIndexCount;

                for (var i = 0; i < triCount; i++)
                {
                    triangles[i * 3 + firstCapTriIndex + 0] = inputTriangles[i * 3 + 1] + firstCapVertex;
                    triangles[i * 3 + firstCapTriIndex + 1] = inputTriangles[i * 3 + 2] + firstCapVertex;
                    triangles[i * 3 + firstCapTriIndex + 2] = inputTriangles[i * 3 + 0] + firstCapVertex;
                }
            }

            // Bottom
            {
                var firstCapVertex = extrudedVertexCount + inputVertices.Length;
                var firstCapTriIndex = extrudedTriIndexCount + inputTriangles.Length;

                for (var i = 0; i < triCount; i++)
                {
                    triangles[i * 3 + firstCapTriIndex + 0] = inputTriangles[i * 3 + 0] + firstCapVertex;
                    triangles[i * 3 + firstCapTriIndex + 1] = inputTriangles[i * 3 + 2] + firstCapVertex;
                    triangles[i * 3 + firstCapTriIndex + 2] = inputTriangles[i * 3 + 1] + firstCapVertex;
                }
            }

            if (invertFaces)
                for (var i = 0; i < triangles.Length / 3; i++)
                {
                    var temp = triangles[i * 3 + 0];
                    triangles[i * 3 + 0] = triangles[i * 3 + 1];
                    triangles[i * 3 + 1] = temp;
                }


            extrudedMesh.Clear();
            extrudedMesh.name      = "extruded";
            extrudedMesh.vertices  = vertices;
            // extrudedMesh.uv        = uvs;
            extrudedMesh.triangles = triangles;
            extrudedMesh.RecalculateNormals();
        }

        /// Builds an array of edges that connect to only one triangle.
        /// In other words, the outline of the mesh	
        public static Edge[] BuildManifoldEdges(Mesh mesh)
        {
            // Build a edge list for all unique edges in the mesh
            var edges = BuildEdges(mesh.vertexCount, mesh.triangles);

            // We only want edges that connect to a single triangle
            var culledEdges = new ArrayList();
            foreach (var edge in edges)
                if (edge.faceIndex[0] == edge.faceIndex[1])
                    culledEdges.Add(edge);


            return culledEdges.ToArray(typeof(Edge)) as Edge[];
        }

        /// Builds an array of unique edges
        /// This requires that your mesh has all vertices welded. However on import, Unity has to split
        /// vertices at uv seams and normal seams. Thus for a mesh with seams in your mesh you
        /// will get two edges adjoining one triangle.
        /// Often this is not a problem but you can fix it by welding vertices 
        /// and passing in the triangle array of the welded vertices.
        public static Edge[] BuildEdges(int vertexCount,
            int[] triangleArray)
        {
            var maxEdgeCount = triangleArray.Length;
            var firstEdge = new int[vertexCount + maxEdgeCount];
            var nextEdge = vertexCount;
            var triangleCount = triangleArray.Length / 3;

            for (var a = 0; a < vertexCount; a++)
                firstEdge[a] = -1;

            // First pass over all triangles. This finds all the edges satisfying the
            // condition that the first vertex index is less than the second vertex index
            // when the direction from the first vertex to the second vertex represents
            // a counterclockwise winding around the triangle to which the edge belongs.
            // For each edge found, the edge index is stored in a linked list of edges
            // belonging to the lower-numbered vertex index i. This allows us to quickly
            // find an edge in the second pass whose higher-numbered vertex index is i.
            var edgeArray = new Edge[maxEdgeCount];

            var edgeCount = 0;

            for (var a = 0; a < triangleCount; a++)
            {
                var i1 = triangleArray[a * 3 + 2];

                for (var b = 0; b < 3; b++)
                {
                    var i2 = triangleArray[a * 3 + b];

                    if (i1 < i2)
                    {
                        var newEdge = new Edge();
                        newEdge.vertexIndex[0] = i1;
                        newEdge.vertexIndex[1] = i2;
                        newEdge.faceIndex[0]   = a;
                        newEdge.faceIndex[1]   = a;
                        edgeArray[edgeCount]   = newEdge;

                        var edgeIndex = firstEdge[i1];

                        if (edgeIndex == -1)
                            firstEdge[i1] = edgeCount;
                        else
                            while (true)
                            {
                                var index = firstEdge[nextEdge + edgeIndex];

                                if (index == -1)
                                {
                                    firstEdge[nextEdge + edgeIndex] = edgeCount;

                                    break;
                                }


                                edgeIndex = index;
                            }


                        firstEdge[nextEdge + edgeCount] = -1;
                        edgeCount++;
                    }


                    i1 = i2;
                }
            }


            // Second pass over all triangles. This finds all the edges satisfying the
            // condition that the first vertex index is greater than the second vertex index
            // when the direction from the first vertex to the second vertex represents
            // a counterclockwise winding around the triangle to which the edge belongs.
            // For each of these edges, the same edge should have already been found in
            // the first pass for a different triangle. Of course we might have edges with only one triangle
            // in that case we just add the edge here
            // So we search the list of edges
            // for the higher-numbered vertex index for the matching edge and fill in the
            // second triangle index. The maximum number of comparisons in this search for
            // any vertex is the number of edges having that vertex as an endpoint.

            for (var a = 0; a < triangleCount; a++)
            {
                var i1 = triangleArray[a * 3 + 2];

                for (var b = 0; b < 3; b++)
                {
                    var i2 = triangleArray[a * 3 + b];

                    if (i1 > i2)
                    {
                        var foundEdge = false;

                        for (var edgeIndex = firstEdge[i2]; edgeIndex != -1; edgeIndex = firstEdge[nextEdge + edgeIndex])
                        {
                            var edge = edgeArray[edgeIndex];

                            if (edge.vertexIndex[1] == i1 && edge.faceIndex[0] == edge.faceIndex[1])
                            {
                                edgeArray[edgeIndex].faceIndex[1] = a;
                                foundEdge                         = true;

                                break;
                            }
                        }


                        if (!foundEdge)
                        {
                            var newEdge = new Edge();
                            newEdge.vertexIndex[0] = i1;
                            newEdge.vertexIndex[1] = i2;
                            newEdge.faceIndex[0]   = a;
                            newEdge.faceIndex[1]   = a;
                            edgeArray[edgeCount]   = newEdge;
                            edgeCount++;
                        }
                    }


                    i1 = i2;
                }
            }


            var compactedEdges = new Edge[edgeCount];
            for (var e = 0; e < edgeCount; e++)
                compactedEdges[e] = edgeArray[e];

            return compactedEdges;
        }
    }
}