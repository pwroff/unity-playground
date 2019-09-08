using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WebcamPlayer : MonoBehaviour
{

    public Material applyTexture;

    WebCamTexture webcam;
    // Start is called before the first frame update
    void Start()
    {
        webcam = new WebCamTexture(1920, 1080);
        webcam.Play();
        applyTexture.mainTexture = webcam;
    }

    private void OnDestroy()
    {
        webcam.Stop();
        Destroy(webcam);
    }
}
