using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFlowerMonster : SimplePathedMoving
{
     Animator anim;

    [Range(0f,1f)]
    public float activeAttackAnimAtPercent = 0.5f;

    public Vector3 checkLocalOffset = new Vector2(0, 0.5f);
    public float checkRadius = 1.2f;
    public LayerMask layerAsTarget;
    public AudioClip soundAttack;
    public AudioClip soundEat;

    public override IEnumerator Start()
    {
        anim = GetComponent<Animator>();
        StartCoroutine(AttackCo());
        return base.Start();
    }

    IEnumerator AttackCo()
    {
        while (true)
        {
            while (fromWaypointIndex == 0 && percentBetweenWaypoints < activeAttackAnimAtPercent)
            {
                yield return null;
            }

            anim.SetBool("close", true);

            yield return new WaitForSeconds(0.3f);
            CheckDamagePlayer();

            while (fromWaypointIndex == 0)
            {
                yield return null;
            }

            while (fromWaypointIndex != 0)
            {
                yield return null;
            }

            anim.SetBool("close", false);
        }
    }

    void CheckDamagePlayer()
    {
        var hits = Physics.OverlapSphere(transform.position + checkLocalOffset, checkRadius, layerAsTarget);
        if (hits.Length > 0)
        {
            foreach (var hit in hits)
            {
                SoundManager.PlaySfx(soundEat);
                var damage = (ICanTakeDamage)hit.gameObject.GetComponent(typeof(ICanTakeDamage));
                if (damage != null)
                    damage.TakeDamage(1, Vector2.zero, gameObject, transform.position);
            }
        }else
            if (Mathf.Abs(gameObject.transform.position.x - GameManager.Instance.Player.transform.position.x) < 10)
            SoundManager.PlaySfx(soundAttack);
    }

    protected override void OnDrawGizmos()
    {
        base.OnDrawGizmos();
        Gizmos.DrawWireSphere(transform.position + checkLocalOffset, checkRadius);
    }
}
