using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterMovement : MonoBehaviour
{
    [Tooltip("In meters per second")]
    public float movementSpeed = 3f;
    public Transform movingPart;
    Rigidbody rb;
    // Start is called before the first frame update
    void Start()
    {
        if (!movingPart)
            movingPart = transform;
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        var rot = transform.rotation.eulerAngles;
        transform.LookAt(CharactersProperties.CursorAt);
        rot.y = transform.rotation.eulerAngles.y;
        transform.rotation = Quaternion.Euler(rot);
        movingPart.position = movingPart.position + Vector3.forward * (CharactersProperties.NextVelocity.z * movementSpeed) * Time.deltaTime + Vector3.right * (CharactersProperties.NextVelocity.x * movementSpeed) * Time.deltaTime;
    }
}
