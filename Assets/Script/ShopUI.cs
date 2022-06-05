using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopUI : MonoBehaviour
{
    public Text txtHearth, txtCoin;

    void Update()
    {
        txtHearth.text = "x" + GlobalValue.SavedLive;
        txtCoin.text = "x" + GlobalValue.SavedCoins;
    }
}