using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentHelper : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == GameManager.Instance.Player.gameObject)
            GameManager.Instance.Player.transform.parent = transform;
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject == GameManager.Instance.Player.gameObject)
            GameManager.Instance.Player.transform.parent = null;
    }
}
