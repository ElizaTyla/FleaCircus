using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    public Transform target;
    public float xOffset;
    public float yOffset;

    void LateUpdate()
    {
        transform.position =
            new Vector3(target.position.x + xOffset, target.position.y + yOffset, transform.position.z);
    }
}
