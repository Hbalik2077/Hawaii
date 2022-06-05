using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrailEffectController : MonoBehaviour
{
   TrailRenderer trailRenderer;
    public float min = 0.05f;
    public float max = 0.3f;

    IEnumerator Start()
    {
        trailRenderer = GetComponent<TrailRenderer>();
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(min, max));
            trailRenderer.emitting = !trailRenderer.emitting;
        }
    }
}
