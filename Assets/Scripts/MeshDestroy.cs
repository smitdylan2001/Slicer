using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshDestroy : MonoBehaviour
{
    private bool edgeSet = false;
    private Vector3 edgeVertex = Vector3.zero;
    private Vector2 edgeUV = Vector2.zero;
    private Plane edgePlane = new Plane();

    public int CutCascades = 1;
    public float ExplodeForce = 0;
    static AudioSource ac;
    private bool isDone;
    private bool started = true;
    private Bounds bounds;
    Ball ball;

    // Start is called before the first frame update
    void Start()
    {
        isDone = false;
        ac = GameObject.Find("Audio Source").GetComponent<AudioSource>();
        ball = GameObject.Find("Sphere").GetComponent<Ball>();
    }

    public void IncomingCollision(int cascades, float force, Vector3 forceDirection, Vector3 startPos)
    {
        DestroyMesh(cascades, force, forceDirection, startPos);
        ac.volume += 0.07f * cascades;
        if (!ac.isPlaying) ac.Play();
        for (int i = 0; i<cascades; i++)
		{
            ball.StartRoutine();
		}
    }

    private void DestroyMesh(int cascades, float force, Vector3 forceDirection, Vector3 startPos)
    {
        CutCascades = cascades;
        ExplodeForce = force;

        var originalMesh = GetComponent<MeshFilter>().mesh;
        originalMesh.RecalculateBounds();
        var parts = new List<PartMesh>();
        var subParts = new List<PartMesh>();

        var mainPart = new PartMesh()
        {
            UV = originalMesh.uv,
            Vertices = originalMesh.vertices,
            Normals = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };
        for (int i = 0; i < originalMesh.subMeshCount; i++)
            mainPart.Triangles[i] = originalMesh.GetTriangles(i);

        parts.Add(mainPart);

        for (var c = 0; c < CutCascades; c++)
        {
            for (var i = 0; i < parts.Count; i++)
            {
                if (!isDone)
				{
                    bounds = parts[i].Bounds;
                    bounds.Expand(0.5f);

                    isDone = true;
				}

                var pos = transform.position;
                var min = new Vector3(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.min.y), Mathf.Abs(bounds.min.z));
                min.Scale(transform.localScale);
                min = pos - (min);
                var max = new Vector3(Mathf.Abs(bounds.max.x), Mathf.Abs(bounds.max.y), Mathf.Abs(bounds.max.z));
                max.Scale(transform.localScale);
                max = pos + (max);
                var sizeVector = max - min;
                var test = new Vector3((startPos.x - min.x) / sizeVector.x,
                    (startPos.y - min.y) / sizeVector.y,
                    (startPos.z - min.z) / sizeVector.z);
                test.Scale(bounds.max - bounds.min);
                var answer = bounds.min + test;

                var plane = new Plane(new Vector3(UnityEngine.Random.Range(0.2f, 0.8f),
                                                    UnityEngine.Random.Range(0.2f, 0.8f),
                                                    UnityEngine.Random.Range(0.2f, 0.8f)).normalized,
                                                     answer);

                subParts.Add(GenerateMesh(parts[i], plane, true));
                subParts.Add(GenerateMesh(parts[i], plane, false));
            }
            parts = new List<PartMesh>(subParts);
            subParts.Clear();
        }
        isDone = true;

        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].MakeGameobject(this);
            parts[i].GameObject.GetComponent<Rigidbody>().AddForceAtPosition(forceDirection * ExplodeForce, transform.position);
        }

        Destroy(gameObject);
    }
    private void DestroyMesh(int cascades, float force, Vector3 forceDirection)
    {
        CutCascades = cascades;
        ExplodeForce = force;

        var originalMesh = GetComponent<MeshFilter>().mesh;
        originalMesh.RecalculateBounds();
        var parts = new List<PartMesh>();
        var subParts = new List<PartMesh>();

        var mainPart = new PartMesh()
        {
            UV = originalMesh.uv,
            Vertices = originalMesh.vertices,
            Normals = originalMesh.normals,
            Triangles = new int[originalMesh.subMeshCount][],
            Bounds = originalMesh.bounds
        };
        for (int i = 0; i < originalMesh.subMeshCount; i++)
            mainPart.Triangles[i] = originalMesh.GetTriangles(i);

        parts.Add(mainPart);

        for (var c = 0; c < CutCascades; c++)
        {
            for (var i = 0; i < parts.Count; i++)
            {
                var bounds = parts[i].Bounds;
                bounds.Expand(0.5f);

                var plane = new Plane(UnityEngine.Random.onUnitSphere, new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x),
                                                                                   UnityEngine.Random.Range(bounds.min.y, bounds.max.y),
                                                                                   UnityEngine.Random.Range(bounds.min.z, bounds.max.z)));


                subParts.Add(GenerateMesh(parts[i], plane, true));
                subParts.Add(GenerateMesh(parts[i], plane, false));
            }
            parts = new List<PartMesh>(subParts);
            subParts.Clear();
        }

        for (var i = 0; i < parts.Count; i++)
        {
            parts[i].MakeGameobject(this);
            parts[i].GameObject.GetComponent<Rigidbody>().AddForceAtPosition(forceDirection * ExplodeForce, transform.position);
        }

        Destroy(gameObject);
    }
    private PartMesh GenerateMesh(PartMesh original, Plane plane, bool left)
    {
        var partMesh = new PartMesh() { };
        var ray1 = new Ray();
        var ray2 = new Ray();


        for (var i = 0; i < original.Triangles.Length; i++)
        {
            var triangles = original.Triangles[i];
            edgeSet = false;

            for (var j = 0; j < triangles.Length; j = j + 3)
            {
                var sideA = plane.GetSide(original.Vertices[triangles[j]]) == left;
                var sideB = plane.GetSide(original.Vertices[triangles[j + 1]]) == left;
                var sideC = plane.GetSide(original.Vertices[triangles[j + 2]]) == left;

                var sideCount = (sideA ? 1 : 0) +
                                (sideB ? 1 : 0) +
                                (sideC ? 1 : 0);
                if (sideCount == 0)
                {
                    continue;
                }
                if (sideCount == 3)
                {
                    partMesh.AddTriangle(i,
                                         original.Vertices[triangles[j]], original.Vertices[triangles[j + 1]], original.Vertices[triangles[j + 2]],
                                         original.Normals[triangles[j]], original.Normals[triangles[j + 1]], original.Normals[triangles[j + 2]],
                                         original.UV[triangles[j]], original.UV[triangles[j + 1]], original.UV[triangles[j + 2]]);
                    continue;
                }

                //cut points
                var singleIndex = sideB == sideC ? 0 : sideA == sideC ? 1 : 2;

                ray1.origin = original.Vertices[triangles[j + singleIndex]];
                var dir1 = original.Vertices[triangles[j + ((singleIndex + 1) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                ray1.direction = dir1;
                plane.Raycast(ray1, out var enter1);
                var lerp1 = enter1 / dir1.magnitude;

                ray2.origin = original.Vertices[triangles[j + singleIndex]];
                var dir2 = original.Vertices[triangles[j + ((singleIndex + 2) % 3)]] - original.Vertices[triangles[j + singleIndex]];
                ray2.direction = dir2;
                plane.Raycast(ray2, out var enter2);
                var lerp2 = enter2 / dir2.magnitude;

                //first vertex = ancor
                AddEdge(i,
                        partMesh,
                        left ? plane.normal * -1f : plane.normal,
                        ray1.origin + ray1.direction.normalized * enter1,
                        ray2.origin + ray2.direction.normalized * enter2,
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                if (sideCount == 1)
                {
                    partMesh.AddTriangle(i,
                                        original.Vertices[triangles[j + singleIndex]],
                                        //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        //Vector3.Lerp(originalMesh.vertices[triangles[j + singleIndex]], originalMesh.vertices[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                        ray1.origin + ray1.direction.normalized * enter1,
                                        ray2.origin + ray2.direction.normalized * enter2,
                                        original.Normals[triangles[j + singleIndex]],
                                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                        original.UV[triangles[j + singleIndex]],
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));

                    continue;
                }

                if (sideCount == 2)
                {
                    partMesh.AddTriangle(i,
                                        ray1.origin + ray1.direction.normalized * enter1,
                                        original.Vertices[triangles[j + ((singleIndex + 1) % 3)]],
                                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.Normals[triangles[j + ((singleIndex + 1) % 3)]],
                                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.UV[triangles[j + ((singleIndex + 1) % 3)]],
                                        original.UV[triangles[j + ((singleIndex + 2) % 3)]]);
                    partMesh.AddTriangle(i,
                                        ray1.origin + ray1.direction.normalized * enter1,
                                        original.Vertices[triangles[j + ((singleIndex + 2) % 3)]],
                                        ray2.origin + ray2.direction.normalized * enter2,
                                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.Normals[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector3.Lerp(original.Normals[triangles[j + singleIndex]], original.Normals[triangles[j + ((singleIndex + 2) % 3)]], lerp2),
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 1) % 3)]], lerp1),
                                        original.UV[triangles[j + ((singleIndex + 2) % 3)]],
                                        Vector2.Lerp(original.UV[triangles[j + singleIndex]], original.UV[triangles[j + ((singleIndex + 2) % 3)]], lerp2));
                    continue;
                }


            }
        }

        partMesh.FillArrays();

        return partMesh;
    }

    private void AddEdge(int subMesh, PartMesh partMesh, Vector3 normal, Vector3 vertex1, Vector3 vertex2, Vector2 uv1, Vector2 uv2)
    {
        if (!edgeSet)
        {
            edgeSet = true;
            edgeVertex = vertex1;
            edgeUV = uv1;
        }
        else
        {
            edgePlane.Set3Points(edgeVertex, vertex1, vertex2);

            partMesh.AddTriangle(subMesh,
                                edgeVertex,
                                edgePlane.GetSide(edgeVertex + normal) ? vertex1 : vertex2,
                                edgePlane.GetSide(edgeVertex + normal) ? vertex2 : vertex1,
                                normal,
                                normal,
                                normal,
                                edgeUV,
                                uv1,
                                uv2);
        }
    }

	private void OnCollisionEnter(Collision collision)
	{
        Rigidbody rb = gameObject.GetComponent<Rigidbody>();

        if (rb.velocity.magnitude > 12 && started)
		{
            DestroyMesh(5, 10, rb.velocity*3f);
            started = false;
            ac.volume += 0.04f * CutCascades;
            if(!ac.isPlaying) ac.Play();

            for (int i = 0; i <  CutCascades; i++)
            {
                ball.StartRoutine();
            }
        }
    }

    
}