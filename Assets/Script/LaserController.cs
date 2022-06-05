using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaserController : MonoBehaviour {
    public LightningGroup[] lightningGroup;
    public AudioClip warningSound;
    public AudioClip laserSound;
    bool isWorking = false;
    int currentGroup = 0;

    public GameObject[] Traps;
    // Use this for initialization
    void Start() {
        foreach (var obj in Traps)
        {
            obj.SetActive(false);
        }
    }

    // Update is called once per frame
    void LateUpdate() {
        if (isWorking)
            transform.position = new Vector2(Camera.main.transform.position.x, transform.position.y);
    }

    public void Working() {
        if (isWorking)
            return;

        foreach (var obj in Traps)
        {
            obj.SetActive(true);
        }

        //		transform.position = new Vector2 (transform.position.x, GameManager.instance.player.transform.position.y);
        isWorking = true;
        currentGroup = 0;
        StartCoroutine(WorkingCo());

        foreach (var obj in lightningGroup) {
            foreach (var obj2 in obj.trapGroup) {
                obj2.MoveIn();
            }
        }
    }

    IEnumerator WorkingCo() {
        while (currentGroup < lightningGroup.Length) {
            currentGroup++;
            //		if (currentGroup > lightningGroup.Length)
            //			yield break;
            yield return new WaitForSeconds(lightningGroup[currentGroup - 1].delay);

            foreach (var obj in lightningGroup[currentGroup - 1].trapGroup) {
                obj.Warning();
                SoundManager.PlaySfx(warningSound);
            }

          
            //yield return new WaitForSeconds(lightningGroup[currentGroup - 1].delay);
            yield return new WaitForSeconds(1f);

            foreach (var obj in lightningGroup[currentGroup - 1].trapGroup) {
                obj.DoLightning(lightningGroup[currentGroup - 1].timeLightning);
                SoundManager.PlaySfx(laserSound);
            }

            yield return new WaitForSeconds(lightningGroup[currentGroup - 1].timeLightning);
            foreach (var obj in lightningGroup[currentGroup - 1].trapGroup) {
                obj.StopLightning();
            }

            if (GameManager.Instance.gameState == GameManager.GameState.GameOver)
                yield break;
        }

        Stop();
    }

    public void Stop() {
        StopAllCoroutines();

        foreach (var obj in lightningGroup) {
            foreach (var obj2 in obj.trapGroup) {
                obj2.MoveOut();
            }
        }

        Invoke("Disable", 2);
    }

    void Disable()
    {
        Destroy(gameObject);
    }



    [System.Serializable]
	public class LightningGroup{
		public LaserTrap[] trapGroup;
		public float delay = 1;
		public float timeLightning = 0.2f;
	}
}
