using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu_AskSaveMe : MonoBehaviour
{
    public Text timerTxt;

    public int timer = 3;
    public int watchVideoFreeLives = 3;
    public Button btnSaveByHeart;
    public Button btnWatchVideoAd;
    
    void OnEnable()
    {
        btnSaveByHeart.interactable = GlobalValue.SavedLive >= 1;
#if UNITY_ANDROID || UNITY_IOS
        btnWatchVideoAd.interactable = AdsManager.Instance && AdsManager.Instance.isRewardedAdReady();
#else
            btnWatchVideoAd.interactable = false;
            btnWatchVideoAd.gameObject.SetActive(false);
#endif

        if (GameManager.Instance)
        {
            if (GameManager.Instance.isTestLevel)
                Continue();
            else if (!btnSaveByHeart.interactable && !btnWatchVideoAd.interactable)
                Close();
            else
            {
                StartCoroutine(StartCountingDown());
            }
        }
    }

    private void OnDisable()
    {
        StopAllCoroutines();
    }

    IEnumerator StartCountingDown()
    {
        var currentTime = Time.realtimeSinceStartup;
        while (Time.realtimeSinceStartup - currentTime < timer)
        {
            timerTxt.text = (timer - (int)(Time.realtimeSinceStartup - currentTime)) + "";
            yield return null;
        }

        Close();
    }

    public void Close()
    {
        StopAllCoroutines();
        //if (AdsManager.Instance)
        //    AdsManager.Instance.ShowAdmobBanner(true);
        GameManager.Instance.gameState = GameManager.GameState.Waiting;
        GameManager.Instance.GameOver(true);
        Time.timeScale = 1;
        gameObject.SetActive(false);
        Destroy(this);      //destroy this script
    }

    public void SaveByHeart()
    {
        StopAllCoroutines();
        SoundManager.Click();
        GlobalValue.SavedLive--;
        Continue();
    }

    public void WatchVideoAd()
    {
        StopAllCoroutines();
        SoundManager.Click();
        AdsManager.AdResult += AdsManager_AdResult;
        AdsManager.Instance.ShowRewardedAds();
    }

    private void AdsManager_AdResult(bool isSuccess)
    {
        AdsManager.AdResult -= AdsManager_AdResult;
        if (isSuccess)
        {
            //reset to avoid play Unity video ad when finish game
            AdsManager.Instance.ResetCounter();
            GlobalValue.SavedLive += watchVideoFreeLives;
            Continue();
        }
    }

    void Continue()
    {
        MenuManager.Instance.Continue();
    }
}
