using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeCell : MonoBehaviour
{
    [SerializeField, HideInInspector]
    public float value = 0;

    [SerializeField, HideInInspector]
    int cubeIndex = 0;
    [SerializeField, HideInInspector]
    public int chunkIndex = 0;

    public Material MatToUse;

    MeshRenderer _mr;
    MeshRenderer mr
    {
        get
        {
            if (_mr == null)
            {
                _mr = GetOrCreate<MeshRenderer>();
            }
            return _mr;
        }
    }
    MeshFilter _mf;
    MeshFilter mf
    {
        get
        {
            if (_mf == null)
            {
                _mf = GetOrCreate<MeshFilter>();
            }
            return _mf;
        }
    }

    [SerializeField]
    Mesh mesh;

    T GetOrCreate<T>() where T : Component
    {
        T o = GetComponent<T>();
        if (o == null)
        {
            o = gameObject.AddComponent<T>();
        }
        return o;
    }

    public void UpdateChunk(int[,,] values, Vector3Int offset, int isolevel = 20)
    {
        cubeIndex = 0;
        var prng = new System.Random((int)(chunkIndex * value));
        int[] vals = new int[12];
        float[] interpValue = new float[12];
        for (int i = 0; i < 8; i++)
        {
            var cp = Defs.CornerPos[i];
            vals[i] = values[offset.x + cp.x, offset.y + cp.y, offset.z + cp.z];
            if (vals[i] <= isolevel)
                cubeIndex |= 1 << i;
            interpValue[i] = Mathf.Max(0, 1 - (vals[i] / isolevel));
        }
        /*
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 1;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 2;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 4;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 8;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 16;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 32;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 64;
        if (prng.Next(0, 30) < isolevel) cubeIndex |= 128;
        */
        if (Defs.EdgeTable[cubeIndex] == 0 || Defs.EdgeTable[cubeIndex] == 0xff)
        {
            return;
        }
        List < Vector3> _vertices = new List<Vector3>();
        List<int> _indices = new List<int>();
        int _currentIndex = 0;
        for (int iTriangle = 0; iTriangle < 5; iTriangle++)
        {
            int edgeIndex = Defs.TriangleConnectionTable[cubeIndex, 3 * iTriangle];
            if (edgeIndex < 0)
                break;

            for (int iCorner = 0; iCorner < 3; iCorner++)
            {
                int iVertex = Defs.TriangleConnectionTable[cubeIndex, 3 * iTriangle + iCorner];

                Vector3 edge1 = Defs.edgeVertexOffsets[iVertex, 0];
                Vector3 edge2 = Defs.edgeVertexOffsets[iVertex, 1];

                Vector3 middlePoint = (edge1 + edge2) * 0.5f;
                /*
                if (interpolate)
                {
                    float offset;
                    float s1 = sampleProc(edge1);
                    float delta = s1 - sampleProc(edge2);
                    if (delta == 0.0f)
                        offset = 0.5f;
                    else
                        offset = s1 / delta;
                    middlePoint = edge1 + offset * (edge2 - edge1); // lerp
                }
                else
                {
                    middlePoint = (edge1 + edge2) * 0.5f;
                }
                */
                // smoothed version would be:

                _vertices.Add(middlePoint);
                _indices.Add(_currentIndex++);
            }
        }

        if (mesh)
        {
            mesh.Clear();
        } else
        {
            mesh = new Mesh();
        }

        mesh.vertices = _vertices.ToArray();
        mesh.triangles = _indices.ToArray();
        mesh.RecalculateNormals();
        mesh.UploadMeshData(false);
        mf.sharedMesh = mesh;
        mr.sharedMaterial = MatToUse;
    }

    
}
