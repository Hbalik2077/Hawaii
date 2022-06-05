using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TheSpring : MonoBehaviour
{
    public float pushHeight = 5;
    public AudioClip sound;
    Animator anim;
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        GameManager.playerRebornEvent += OnPlayerReborn;
    }

    private void OnDisable()
    {
        GameManager.playerRebornEvent -= OnPlayerReborn;
    }

    void OnPlayerReborn()
    {
        anim.SetBool("isWorked", false);
    }

    public void Action()
    {
        anim.SetBool("isWorked", true);
        SoundManager.PlaySfx(sound);

        Invoke("OnPlayerReborn", 0.5f);
    }
}
