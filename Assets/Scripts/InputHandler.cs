using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    // Update is called once per frame
    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hitInfo;
        if (Physics.Raycast(ray, out hitInfo))
            CharactersProperties.SetCursorAt(hitInfo.point);
        Vector3 velocity = Vector3.zero;
        velocity.z = Input.GetAxis("Vertical");
        velocity.x = Input.GetAxis("Horizontal");
        CharactersProperties.SetNextVelocity(velocity);
    }
}
