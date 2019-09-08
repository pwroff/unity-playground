using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatingPoints : MonoBehaviour
{
    public Vector2Int textureSize = new Vector2Int(50, 50);
    public int pointOffset = 10;
    public uint thicknes = 1;
    [Range(-360, 360)]
    public int angle = 0;

    Texture2D tex;
    MeshRenderer mr;
    Vector2Int[] points;
    // Start is called before the first frame update
    void Start()
    {
        ResetIt();
    }

    void ResetIt()
    {
        mr = GetComponent<MeshRenderer>();
        points = new Vector2Int[]
        {
            new Vector2Int(pointOffset, pointOffset),
            new Vector2Int(textureSize.x - pointOffset, pointOffset),
            new Vector2Int(pointOffset, textureSize.y - pointOffset),
            new Vector2Int(textureSize.x - pointOffset, textureSize.y - pointOffset)
        };
        ClearTexture();
    }

    void ClearTexture()
    {
        if (tex != null)
            Destroy(tex);
        tex = new Texture2D(textureSize.x, textureSize.y);
        mr.sharedMaterial.mainTexture = tex;

    }

    float GetPointOnCircle(float x, float cx, float y, float cy)
    {
        return (x - cx) * (x - cx) + (y - cy) * (y - cy);
    }

    void DrawOnCircle()
    {
        float alpha = (float)angle * (Mathf.PI / 180f);
        float cosAlpha = Mathf.Cos(alpha);
        float sinAlpha = Mathf.Sin(alpha);
        var center = new Vector2Int(textureSize.x / 2, textureSize.y / 2);
        for (int i = 0; i < 4; i++)
        {
            var point = points[i];
            float cx = point.x - center.x;
            float cy = point.y - center.y;
            float x = cx * cosAlpha - cy * sinAlpha;
            float y = cy * cosAlpha + cx * sinAlpha;
            var rotatedPoint = new Vector2Int(center.x + (int)x, center.y + (int)y);
            Debug.Log(rotatedPoint);
            tex.SetPixel(rotatedPoint.x, rotatedPoint.y, Color.red);
        }

        tex.Apply();
    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnValidate()
    {
        if (tex)
            DestroyImmediate(tex);
        tex = null;
        ResetIt();
        DrawOnCircle();
    }
}
