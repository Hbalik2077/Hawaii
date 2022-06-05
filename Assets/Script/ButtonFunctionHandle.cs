using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ButtonFunctionHandle : MonoBehaviour
{
    public GameObject activeGroup, disActiveGroup;
    public void SetActive(bool active)
    {
        activeGroup.SetActive(active);
        disActiveGroup.SetActive(!active);
        GetComponent<CanvasGroup>().blocksRaycasts = active;
    }
}
