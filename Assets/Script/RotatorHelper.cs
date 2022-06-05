using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotatorHelper : MonoBehaviour
{
    public float rotateSpeed = 100;
    public Vector3 rotateVector = new Vector3(0, 0, 1);     //z = 1 mean rotate around Z axis

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(rotateVector, rotateSpeed * Time.deltaTime, Space.Self);
    }
}
