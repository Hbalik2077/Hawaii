using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class BtnPressFX : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Animator btnAnim;

    public void OnPointerEnter(PointerEventData eventData)
    {
        btnAnim.SetBool("holding", true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        btnAnim.SetBool("holding", false);
    }
}
