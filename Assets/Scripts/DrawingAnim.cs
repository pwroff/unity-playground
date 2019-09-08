using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawingAnim : MonoBehaviour
{
    public Vector2Int textureSize = new Vector2Int(1024, 1024);
    public uint thicknes = 1;
    public uint radius = 5;

    Texture2D tex;
    MeshRenderer mr;
    // Start is called before the first frame update
    void Start()
    {
        ResetIt();
    }

    void ResetIt()
    {
        mr = GetComponent<MeshRenderer>();
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
        return (x - cx)*(x - cx) + (y - cy) * (y - cy);
    }

    void DrawOnCircle()
    {
        Color32[] pixels = new Color32[textureSize.x * textureSize.y];

        int dotscount = 100;
        float r = radius;
        float cx = textureSize.x / 2f;
        float cy = textureSize.y / 2f;
        for (int y = 0; y < textureSize.y; y++)
        {
            for (int x = 0; x < textureSize.x; x++)
            {
                if (GetPointOnCircle(x, cx, y,cy) <= r*r)
                {
                    pixels[y * textureSize.x + x] = Color.red;
                }
            }
        }


        tex.SetPixels32(pixels);

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
        if (radius > textureSize.x)
            radius = (uint)textureSize.x;
        ResetIt();
        DrawOnCircle();
    }
}
