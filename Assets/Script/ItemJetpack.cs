using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemJetpack : TriggerEvent
{
    public GameObject collectedFX;
    public AudioClip sound;
    bool isUsed = false;

    public override void OnContactPlayer()
    {
        if (isUsed)
            return;

        isUsed = true;
        
        if (collectedFX)
            Instantiate(collectedFX, transform.position, Quaternion.identity);
        SoundManager.PlaySfx(sound);
        GameManager.Instance.Player.ActiveJetpack(true);
        gameObject.SetActive(false);
    }
}
