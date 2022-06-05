using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopItemUI : MonoBehaviour
{
    public enum ITEM_TYPE { none, watchVideo, buyLive}

    public ITEM_TYPE itemType;
    public int rewarded = 100;
    public float price = 100;
    public GameObject watchVideocontainer;

    public AudioClip soundRewarded;

    public Text priceTxt, rewardedTxt, rewardTimeCountDownTxt;

    private void Update()
    {
        UpdateStatus();
    }

    void UpdateStatus()
    {
        if (itemType == ITEM_TYPE.buyLive)
        {
            priceTxt.text = price + "";
            rewardedTxt.text = "+" + rewarded;
        }
        else if (itemType == ITEM_TYPE.watchVideo)
        {
            priceTxt.text = "FREE";
            rewardedTxt.text = "+" + rewarded;

         
            if (watchVideocontainer != null)
            {
                watchVideocontainer.SetActive(AdsManager.Instance && AdsManager.Instance.isRewardedAdReady());

                if (AdsManager.Instance && AdsManager.Instance.TimeWaitingNextWatch() > 0)
                {
                    watchVideocontainer.SetActive(false);
                    rewardTimeCountDownTxt.text = 
                    ((int)(AdsManager.Instance.TimeWaitingNextWatch()) / 60).ToString("0") + ":" + ((int)AdsManager.Instance.TimeWaitingNextWatch() % 60).ToString("00");
                }
                else
                {
                    if (rewardTimeCountDownTxt)
                    {
                        rewardTimeCountDownTxt.text = "";

                        if (!AdsManager.Instance || (AdsManager.Instance && !AdsManager.Instance.isRewardedAdReady()))
                            rewardTimeCountDownTxt.text = "No Ads";
                    }
                }
            }
        }
    }

    public void Buy()
    {
        switch (itemType)
        {
            case ITEM_TYPE.buyLive:
                if (GlobalValue.SavedCoins >= price)
                {
                    GlobalValue.SavedCoins -= (int)price;
                    GlobalValue.SavedLive += rewarded;
                    SoundManager.PlaySfx(soundRewarded);
                }
                break;
            case ITEM_TYPE.watchVideo:
                if (AdsManager.Instance && AdsManager.Instance.isRewardedAdReady())
                {
                    AdsManager.AdResult += AdsManager_AdResult;
                    AdsManager.Instance.ShowRewardedAds();
                }
                break;
        }
    }

    private void AdsManager_AdResult(bool isSuccess)
    {
        AdsManager.AdResult -= AdsManager_AdResult;
        if (isSuccess)
        {
            GlobalValue.SavedCoins += rewarded;
            SoundManager.PlaySfx(soundRewarded);
            UpdateStatus();
        }
    }
}
