using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAttack : MonoBehaviour
{
	[Tooltip("What layers should be hit")]
	public LayerMask CollisionMask;
	[Tooltip("Hit more than one enemy at the same time")]
	public bool multiDamage = false;
	[Tooltip("Give damage to the enemy or object")]
	public int damageToGive = 10;
	public Transform MeleePoint;
	public float areaSize = 0.2f;
	public float attackRate = 0.2f;
	[Tooltip("Check target after a delay time, useful to sync the right attack time of the animation")]
	public float attackAfterTime = 0.15f;

	[Tooltip("Force player stand after attack")]
	public float standingTime = 1;
	public GameObject hitFX;
	public AudioClip meleeAttackSound;
	public AudioClip hitSound;
	float nextAttack = 0;

	public bool Attack()
	{
		if (Time.time > nextAttack)
		{
			nextAttack = Time.time + attackRate;
			StartCoroutine(CheckTargetCo(attackAfterTime));
			SoundManager.PlaySfx(meleeAttackSound);
			return true;
		}
		else
			return false;
	}

	IEnumerator CheckTargetCo(float delay)
	{
		yield return new WaitForSeconds(delay);
		var hits = Physics.OverlapSphere(MeleePoint.position, areaSize, CollisionMask);

		if (hits.Length == 0)
			yield break;

		foreach (var hit in hits)
		{
			var damage = (ICanTakeDamage)hit.gameObject.GetComponent(typeof(ICanTakeDamage));
			if (damage == null)
				continue;

			damage.TakeDamage(damageToGive, Vector2.zero, gameObject, MeleePoint.position);
			if (!multiDamage)
				break;

		}

		Instantiate(hitFX, MeleePoint.position, hitFX.transform.rotation);
		SoundManager.PlaySfx(hitSound);
	}

	void OnDrawGizmos()
	{
		if (MeleePoint == null)
			return;

		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(MeleePoint.position, areaSize);
	}
}
