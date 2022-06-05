using UnityEngine;
using System.Collections;

public class AutoDestroy : MonoBehaviour {
    public bool onlyDisactive = false;
	public float destroyAfterTime = 3f;

    private void OnEnable()
    {
        StartCoroutine(DisableCo());
    }

    IEnumerator DisableCo()
    {
        yield return new WaitForSeconds(destroyAfterTime);
        if (onlyDisactive)
            gameObject.SetActive(false);
        else Destroy(gameObject);
    }
}
