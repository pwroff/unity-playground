using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlicerPlane : MonoBehaviour
{
    class Triangle
    {
        public Vector3 va;
        public Vector3 vb;
        public Vector3 vc;

        public Vector3 GetNormal() // gets normal of the triangle
        {
            return Vector3.Cross(va - vb, va - vc).normalized;
        }

        public void MatchDirection(Vector3 dir) // flips direction if doesn't match with dir
        {
            if (Vector3.Dot(GetNormal(), dir) > 0)
            {
                return;
            }
            else
            {
                Vector3 vsa = va;
                va = vc;
                vc = vsa;
            }
        }
    }
    public float planeMinWidth = 1;
    public float planeDepth = 10;
    bool startedSlice = false;
    Vector3 startAt;
    Vector3 endAt;
    Camera mainCamera;

    Vector3 GetMouseAtWorldPos()
    {
        var pos = Input.mousePosition;
        pos.z = Camera.main.nearClipPlane;
        return Camera.main.ScreenToWorldPoint(pos);
    }

    // Start is called before the first frame update
    void Start()
    {
        mainCamera = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            startedSlice = true;
            hitMeshes.Clear();
            startAt = GetMouseAtWorldPos();
        }
        if (startedSlice)
        {
            if (Input.GetMouseButtonUp(0))
            {
                startedSlice = false;
                endAt = GetMouseAtWorldPos();
                RecreateSlicePlane();
            }
            else
                CollectSliceable();
        }
        Debug.DrawLine(Vector3.zero, plane.normal * 5);
    }
    List<MeshFilter> hitMeshes = new List<MeshFilter>();

    void CollectSliceable()
    {
        RaycastHit[] hit = Physics.RaycastAll(GetMouseAtWorldPos(), mainCamera.transform.forward, planeDepth);
        for (int i = 0; i < hit.Length; i++)
        {
            var mf = hit[i].transform.GetComponent<MeshFilter>();
            if (mf != null && mf.sharedMesh != null && !hitMeshes.Contains(mf))
            {
                hitMeshes.Add(mf);
            }
        }
    }

    void RecreateSlicePlane()
    {
        var dir = (endAt - startAt).normalized;
        Debug.Log("Start slice: " + startAt);
        Debug.Log("End slice: " + (startAt + dir * planeMinWidth));
        Vector3 maxSlice = (startAt + dir * planeMinWidth);

        plane = new Plane(startAt, startAt + (Vector3.forward) * planeDepth, maxSlice);
        for (int i = hitMeshes.Count; i > 0; i--)
        {
            SliceMesh(hitMeshes[i - 1], ref dir);
            hitMeshes.RemoveAt(i - 1);
        }
    }
    public Plane plane;
    void SliceMesh(MeshFilter mf, ref Vector3 dir)
    {
        Mesh mesh = mf.sharedMesh;
        int count = mesh.vertexCount;
        var vertices = mesh.vertices;

        List<Triangle> aTris = new List<Triangle>();
        List<Triangle> bTris = new List<Triangle>();
        List<Vector3> intersections = new List<Vector3>();
        for (int i = 0; i < mesh.triangles.Length; i += 3)
        {
            var tri = new Triangle()
            {
                va = vertices[mesh.triangles[i]],
                vb = vertices[mesh.triangles[i + 1]],
                vc = vertices[mesh.triangles[i + 2]]
            };
            var norm = tri.GetNormal();
            var pa = mf.transform.TransformPoint(tri.va);
            var pb = mf.transform.TransformPoint(tri.vb);
            var pc = mf.transform.TransformPoint(tri.vc);
            var pointsL = new List<Vector3>();
            var pointsR = new List<Vector3>();
            if (plane.GetSide(pa))
            {
                pointsL.Add(tri.va);
            } else
            {
                pointsR.Add(tri.va);
            }
            if (plane.GetSide(pb))
            {
                pointsL.Add(tri.vb);
            } else
            {
                pointsR.Add(tri.vb);
            }
            if (plane.GetSide(pc))
            {
                pointsL.Add(tri.vc);
            }
            else
            {
                pointsR.Add(tri.vc);
            }
            if (pointsL.Count == 3)
            {
                aTris.Add(tri);
                continue;
            }
            if (pointsR.Count == 3)
            {
                bTris.Add(tri);
                continue;
            }
            if (pointsL.Count == 2)
            {
                Vector3 tpos = mf.transform.position;
                Vector3 worldA = pointsL[0] + tpos;
                Vector3 worldB = pointsL[1] + tpos;
                Vector3 worldC = pointsR[0] + tpos;
                
                Vector3 vcA = plane.LineIntersection(worldA, worldC) - tpos;
                Vector3 vcB = plane.LineIntersection(worldB, worldC) - tpos;
                intersections.Add(vcA);
                intersections.Add(vcB);
                var triA = new Triangle()
                {
                    va = pointsL[0],
                    vb = pointsL[1],
                    vc = vcA
                };
                
                triA.MatchDirection(norm);
                var triB = new Triangle()
                {
                    va = pointsL[1],
                    vb = vcB,
                    vc = vcA
                };
                triB.MatchDirection(norm);
                aTris.Add(triA);
                aTris.Add(triB);
                

                var triC = new Triangle()
                {
                    va = vcA,
                    vb = vcB,
                    vc = pointsR[0]
                };
                triC.MatchDirection(norm);
                bTris.Add(triC);
            } else
            {
                Vector3 tpos = mf.transform.position;
                Vector3 worldA = pointsR[0] + tpos;
                Vector3 worldB = pointsR[1] + tpos;
                Vector3 worldC = pointsL[0] + tpos;

                Vector3 vcA = plane.LineIntersection(worldA, worldC) - tpos;
                Vector3 vcB = plane.LineIntersection(worldB, worldC) - tpos;
                intersections.Add(vcA);
                intersections.Add(vcB);
                var triA = new Triangle()
                {
                    va = pointsR[0],
                    vb = pointsR[1],
                    vc = vcA
                };

                triA.MatchDirection(norm);
                var triB = new Triangle()
                {
                    va = pointsR[1],
                    vb = vcB,
                    vc = vcA
                };
                triB.MatchDirection(norm);
                bTris.Add(triA);
                bTris.Add(triB);

                var triC = new Triangle()
                {
                    va = vcA,
                    vb = vcB,
                    vc = pointsL[0]
                };
                triC.MatchDirection(norm);
                aTris.Add(triC);
            }
        }

        CreateMesh(aTris, mf);
        CreateMesh(bTris, mf);
        Destroy(mf.gameObject);
    }

    void CreateMesh(List<Triangle> tris, MeshFilter mf)
    {
        var go = new GameObject();
        var mfr = go.AddComponent<MeshRenderer>();
        
        var mfc = go.AddComponent<MeshFilter>();
        mfr.sharedMaterial = mf.GetComponent<MeshRenderer>().sharedMaterial;
        go.transform.position = mf.transform.position;
        go.transform.rotation = mf.transform.rotation;
        go.transform.localScale = mf.transform.localScale;

        Mesh m = new Mesh();
        Vector3[] vertices = new Vector3[tris.Count * 3];
        int[] triangles = new int[tris.Count * 3];
        for (int j = 0; j < tris.Count; j++)
        {
            int i = j * 3;
            vertices[i] = tris[j].va;
            vertices[i + 1] = tris[j].vb;
            vertices[i + 2] = tris[j].vc;
            triangles[i] = i;
            triangles[i + 1] = i + 1;
            triangles[i + 2] = i + 2;
        }

        m.vertices = vertices;
        m.triangles = triangles;
        m.RecalculateNormals();
        m.RecalculateBounds();
        var coll = go.AddComponent<MeshCollider>();
        mfc.sharedMesh = m;
        coll.convex = true;
        coll.sharedMesh = m;
        var rb = go.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.AddExplosionForce(20, Vector3.zero, 1);
    }
}

public static class PlaneExtension
{
    public static Vector3 LineIntersection(this Plane plane, Vector3 linePointa, Vector3 linePointb)
    {
        Vector3 ba = linePointb - linePointa;
        Vector3 n = plane.normal;
        float d = plane.distance;
        float nDotA = Vector3.Dot(n, linePointa);
        float nDotBA = Vector3.Dot(n, ba);

        return linePointa + (((d - nDotA) / nDotBA) * ba);
        //return linePoint + lineDirection * plane.DistanceToLineIntersection(linePoint, lineDirection);
    }

    public static float DistanceToLineIntersection(this Plane plane, Vector3 linePoint, Vector3 lineDirection, float maxLength = float.MaxValue)
    {
        Vector3 intersection = plane.LineIntersection(linePoint, linePoint + lineDirection*maxLength);

        return Vector3.Distance(linePoint, intersection);
    }
}
