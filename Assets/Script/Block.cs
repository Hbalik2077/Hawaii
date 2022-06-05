using UnityEngine;
using System.Collections;

public class Block : MonoBehaviour, ICanTakeDamage {
	public enum BlockTyle{Destroyable, Rocky}
	public BlockTyle blockTyle;
	public LayerMask enemiesLayer;

	public int maxHit = 1;
	public float pushEnemyUp = 7f;

	[Header("Destroyable")]
	public GameObject DestroyEffect;

	[Header("HidenTreasure")]
	public GameObject[] Treasure;
	public Vector3 spawnLocalPos = new Vector3(0,1,0);

	public GameObject rockyBock;
	[Header("Sound")]
	public AudioClip soundDestroy;
	[Range(0,1)]
	public float soundDestroyVolume = 0.5f;
	public AudioClip soundSpawn;
	[Range(0,1)]
	public float soundSpawnVolume = 0.5f;

	Animator anim;
	int currentHitLeft;

	void Start () {
		anim = GetComponent<Animator> ();
		currentHitLeft = Mathf.Clamp (maxHit, 1, int.MaxValue);
	}

    public void BoxHit()
    {
        if (isWaitNextHit)
            return;


        if (currentHitLeft <= 0)
            return;

        StartCoroutine(BoxHitCo());
    }

    bool isWaitNextHit = false;

    IEnumerator BoxHitCo() {
        isWaitNextHit = true;

		var random = Treasure.Length > 0 ? Treasure [Random.Range (0, Treasure.Length)] : null;
		if (random != null) {
			var item =  Instantiate (random, transform.position + spawnLocalPos, Quaternion.identity) as GameObject;
			var rig = item.AddComponent<Rigidbody>();
			rig.freezeRotation = true;
			rig.velocity = new Vector3(Random.Range(-2f, 2f), Random.Range(3f,6f),0);
			var boxCol = item.AddComponent<BoxCollider>();
			boxCol.size = Vector3.one * 0.5f;

			SoundManager.PlaySfx (soundSpawn, soundSpawnVolume);
		}
		
		CheckEnemiesOnTop ();

		if (anim)
			anim.SetTrigger ("hit");

        currentHitLeft--;
        if (currentHitLeft > 0)
        {
            yield return null;
            isWaitNextHit = false;
            yield break;
        }

        if (blockTyle == BlockTyle.Destroyable) {
			if (random == null)		//only play destroy sound when there are no treasure to spawn
						SoundManager.PlaySfx (soundDestroy, soundDestroyVolume);
					
			if (DestroyEffect != null)
				Instantiate (DestroyEffect, transform.position, Quaternion.identity);

			Destroy(gameObject);
		}else if(blockTyle == BlockTyle.Rocky)
        {
			Instantiate(rockyBock, transform.position, Quaternion.identity);
			Destroy(gameObject);
        }

        yield return null;
        isWaitNextHit = false;
    }

	void CheckEnemiesOnTop()
	{
		RaycastHit[] hits = Physics.BoxCastAll(transform.position, GetComponent<BoxCollider>().size, Vector3.up, Quaternion.Euler(Vector3.zero), 1, enemiesLayer);
		foreach (var hit in hits)
		{
			Debug.Log(hit.collider.name);
			var damage = (ICanTakeDamage)hit.collider.gameObject.GetComponent(typeof(ICanTakeDamage));
			if (damage != null)
				damage.TakeDamage(10000, Vector2.up * pushEnemyUp, gameObject, hit.point); //kill it right away
		}
	}

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
		BoxHit();
	}
}
