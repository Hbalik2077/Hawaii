using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleFlyingEnemy : TriggerEvent
{
    public float speed = 1;
    public float liveTime = 5;
    public GameObject destroyFX;
    public AudioClip showUpSound;
    public AudioClip destroySound;

    private void Start()
    {
        SoundManager.PlaySfx(showUpSound);
        Destroy(gameObject, liveTime);
    }

    public override void OnContactPlayer()
    {
        GameManager.Instance.Player.Die();
        SoundManager.PlaySfx(destroySound);
        Instantiate(destroyFX, transform.position, Quaternion.identity);
        Destroy(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime, Space.Self);
    }
}
