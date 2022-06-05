using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetObjectOnPlayerDead : MonoBehaviour
{
    [Header("RESET OBJECT WHEN PLAYER REBORN")]
    [ReadOnly] public GameObject cloneObj;

    private void OnEnable()
    {
        GameManager.playerRebornEvent += OnPlayerReborn;
    }
    private void OnDisable()
    {
        GameManager.playerRebornEvent -= OnPlayerReborn;
    }

    private void Start()
    {
        cloneObj = Instantiate(gameObject, transform.position, transform.rotation);
        cloneObj.SetActive(false);
    }

    void OnPlayerReborn()
    {
        cloneObj.SetActive(true);
        Destroy(gameObject);
    }
}