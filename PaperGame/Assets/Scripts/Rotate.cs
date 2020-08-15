using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotate : MonoBehaviour
{
    public Vector3 dynamicRotation; //direction of constant rotation

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(dynamicRotation * Time.deltaTime);
    }
}
