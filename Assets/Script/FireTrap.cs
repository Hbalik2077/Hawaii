using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireTrap : MonoBehaviour
{
    public float showTime = 2;
    public float hideTime = 1.5f;
  
    public ParticleSystem[] fxList;

    Collider collider;
    private void Start()
    {
        collider = GetComponent<Collider>();
        StartCoroutine(WorkingCo());
    }

    IEnumerator WorkingCo()
    {
        while (true)
        {
            UpdateJetPackStatus(true);
            collider.enabled = true;
            yield return new WaitForSeconds(showTime);
            UpdateJetPackStatus(false);
            collider.enabled = false;
            yield return new WaitForSeconds(hideTime);
        }
    }

    void UpdateJetPackStatus(bool on)
    {
        for (int i = 0; i < fxList.Length; i++)
        {
            var emission = fxList[i].emission;
            emission.enabled = on;
        }
    }
}
