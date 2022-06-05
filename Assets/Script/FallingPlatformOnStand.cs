using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FallingPlatformOnStand : MonoBehaviour, IPlayerStandOn
{
    public float delay = 1;
    public float respawnAfterTime = 4;
    bool isWorking = false;
    Rigidbody m_rig;
    Vector3 originalPos, originalRot;

    private void Start()
    {
        m_rig = GetComponent<Rigidbody>();
        originalPos = transform.position;
        originalRot = transform.rotation.eulerAngles;
    }

    public void OnPlayerStandOn()
    {
        if (isWorking)
            return;

        StartCoroutine(WorkingCo());
    }

    IEnumerator WorkingCo()
    {
        isWorking = true;
        yield return new WaitForSeconds(delay);
        m_rig.isKinematic = false;

        yield return new WaitForSeconds(respawnAfterTime);
        transform.position = originalPos;
        transform.rotation = Quaternion.Euler(originalRot);
        m_rig.isKinematic = true;
        isWorking = false;
    }
}
