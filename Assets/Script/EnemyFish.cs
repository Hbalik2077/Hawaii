using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyFish : MonoBehaviour, ICanTakeDamage
{
	public GameObject dieFX;
	public AudioClip dieSound;
	public bool isLoop = true;
	public List<Vector3> localWaypoints;
	public float moveSpeed = 1;
	[Range(0.1f, 1f)]
	public float smooth = 0.5f;
	Vector3[] globalWaypoints;
	int toWaypointIndex;

	public float speed = 3;
	public bool cyclic;
	public float waitTime = 1;
	[Range(0, 2)]
	public float easeAmount;
	int fromWaypointIndex;
	float percentBetweenWaypoints;
	float nextMoveTime;
	[ReadOnly] public Vector3 velocity;
	Animator anim;

	bool isFacingRight { get { return transform.rotation.eulerAngles.y == 0 ? true : false; } }

	void Start()
	{
		globalWaypoints = new Vector3[localWaypoints.Count];
		for (int i = 0; i < localWaypoints.Count; i++)
		{
			globalWaypoints[i] = localWaypoints[i] + transform.position;
		}

		anim = GetComponent<Animator>();
	}

	void LateUpdate()
	{
		velocity = CalculatePlatformMovement();
		if (velocity.x != 0)
			transform.forward = new Vector3(velocity.x > 0 ? 1 : -1, 0, 0);

		transform.Translate(velocity, Space.World);

		//if ((isFacingRight && velocity.x < 0) || (!isFacingRight && velocity.x > 0))
		//{
		//	Flip();
		//}

		anim.SetBool("move", Mathf.Abs(velocity.x) > 0);
	}

	//void Flip()
	//{
	//	transform.rotation = Quaternion.Euler(new Vector3(transform.rotation.x, isFacingRight ? 180 : 0, transform.rotation.z));
	//}

	float Ease(float x)
	{
		float a = easeAmount + 1;
		return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
	}

	Vector3 CalculatePlatformMovement()
	{

		if (Time.time < nextMoveTime)
		{
			return Vector3.zero;
		}

		fromWaypointIndex %= globalWaypoints.Length;
		toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
		float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
		percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
		percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
		float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

		Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);


		if (percentBetweenWaypoints >= 1)
		{
			percentBetweenWaypoints = 0;
			fromWaypointIndex++;

			if (!isLoop && (fromWaypointIndex >= globalWaypoints.Length - 1))
			{
				enabled = false;
			}
			else if (!cyclic)
			{
				if (fromWaypointIndex >= globalWaypoints.Length - 1)
				{
					fromWaypointIndex = 0;
					System.Array.Reverse(globalWaypoints);


					Invoke("AllowLookAgain", 0.1f);
				}
			}
			nextMoveTime = Time.time + waitTime;
		}

		return newPos - transform.position;
	}

	void OnDrawGizmos()
	{
		if (localWaypoints != null && this.enabled)
		{
			for (int i = 0; i < localWaypoints.Count; i++)
			{
				Gizmos.color = Color.red;

				Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;
				Gizmos.DrawWireCube(globalWaypointPos, Vector3.one * 0.5f);

				if (Application.isPlaying)
					Gizmos.DrawLine(globalWaypoints[i], globalWaypoints[0]);
				else
					Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[0] + transform.position);
			}
		}
	}

	public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
	{
		if (dieFX)
			Instantiate(dieFX, transform.position, Quaternion.identity);

		SoundManager.PlaySfx(dieSound);
		gameObject.SetActive(false);

	}
}
