using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterChecker : MonoBehaviour
{
    public LayerMask layerAsWater;
    public Vector2 checkOffset = new Vector2(0, 0.5f);
    [ReadOnly] public bool isInWater = false;

    private void Update()
    {
        isInWater = Physics.OverlapSphere(transform.position + (Vector3)checkOffset, 0.1f, layerAsWater).Length > 0;
    }
}