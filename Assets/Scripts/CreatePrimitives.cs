using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreatePrimitives : MonoBehaviour
{
    public PrimitiveType type = PrimitiveType.Sphere;
    public uint primitivesCount = 50;
    public float maxDistance = 10f;
    [SerializeField, HideInInspector]
    GameObject[] primitives;

    void DestoryChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            DestroyImmediate(transform.GetChild(i));
        }
    }

    void UpdatePrimitivesPosition()
    {
        foreach (var go in primitives)
        {
            go.transform.localPosition = Random.insideUnitSphere * maxDistance;
        }
    }

    GameObject CreatePrimitive()
    {
        var go = GameObject.CreatePrimitive(type);
        Material mat = new Material(go.GetComponent<MeshRenderer>().sharedMaterial);
        mat.color = Random.ColorHSV();
        go.GetComponent<MeshRenderer>().sharedMaterial = mat;
        return go;
    }

    private void OnValidate()
    {
        if (primitives == null || primitives.Length != primitivesCount)
        {
            DestoryChildren();
            primitives = new GameObject[primitivesCount];
            for (int i = 0; i < primitivesCount; i++)
            {
                primitives[i] = CreatePrimitive();
                primitives[i].transform.parent = transform;
            }

            UpdatePrimitivesPosition();
        }
    }
}
