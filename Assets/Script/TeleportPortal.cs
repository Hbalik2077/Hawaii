using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TeleportPortal : TriggerEvent
{
    public Vector2 localNextPoint = new Vector2(2, 0);
    public AudioClip sound;
    public override void OnContactPlayer()
    {
        base.OnContactPlayer();
        //Debug.LogError(GameManager.Instance.Player.transform.position);
        //GameManager.Instance.Player.transform.position = transform.position + (Vector3)localNextPoint;
        //Debug.LogError("-" + GameManager.Instance.Player.transform.position);

        //GameManager.Instance.Player.transform.position = transform.position;
        //GameManager.Instance.Player.characterController.Move((Vector3)localNextPoint);
        GameManager.Instance.Player.TeleportTo(transform.position + (Vector3)localNextPoint);
        SoundManager.PlaySfx(sound);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(transform.position + (Vector3)localNextPoint, 0.3f);
        Gizmos.DrawLine(transform.position, transform.position + (Vector3)localNextPoint);
    }
}