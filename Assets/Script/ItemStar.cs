using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemStar : TriggerEvent
{
    public int ID = 1;
    public AudioClip sound;
    public GameObject collectedFX;
    bool isCollected = false;

    private void Start()
    {
        CheckCollected();
    }

    void CheckCollected()
    {
        bool isCollected = GlobalValue.IsScrollLevelAte(ID, GlobalValue.levelPlaying);

        if (isCollected)
        {
            MenuManager.Instance.CollectStar(ID);
            Destroy(gameObject);
        }
    }

    public override void OnContactPlayer()
    {
        if (isCollected)
            return;

        isCollected = true;

        if (GlobalValue.levelPlaying != -1)
        {
            GlobalValue.SetScrollLevelAte(ID, GlobalValue.levelPlaying);
        }

        SoundManager.PlaySfx(sound);
        if (collectedFX)
            Instantiate(collectedFX, transform.position, Quaternion.identity);

        MenuManager.Instance.CollectStar(ID);
        Destroy(gameObject);
    }
}
