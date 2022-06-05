using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemCoin : TriggerEvent
{
    public int amount = 1;
    public GameObject collectedFX;
    public AudioClip sound;
    bool isUsed = false;

    public override void OnContactPlayer()
    {
        if (isUsed)
            return;

        isUsed = true;
        GlobalValue.SavedCoins += amount;
        if (collectedFX)
            Instantiate(collectedFX, transform.position, Quaternion.identity);
        SoundManager.PlaySfx(sound);
        Destroy(gameObject);
    }
}