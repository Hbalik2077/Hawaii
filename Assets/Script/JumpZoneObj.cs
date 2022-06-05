using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpZoneObj : MonoBehaviour
{
    public Color inActiveColor, activeColor;
    [Header("Active Slow motion to guide player")]
    public bool slowMotion = false;
    public GameObject activeHelperObj;
    public AudioClip sound;
    MeshRenderer meshRenderer;
    Animator anim;

    private void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        anim = GetComponent<Animator>();
        if (activeHelperObj)
            activeHelperObj.SetActive(false);
        SetState(false);
    }

    public void SetState(bool active)
    {
        meshRenderer.material.color = active ? activeColor : inActiveColor;
        if (active)
        {
            //SoundManager.PlaySfx(sound);
            //anim.SetTrigger("work");
            if (activeHelperObj)
                activeHelperObj.SetActive(true);
        }
    }

    public void SetStateJump()
    {
        SoundManager.PlaySfx(sound);
        anim.SetTrigger("work");
    }
}
