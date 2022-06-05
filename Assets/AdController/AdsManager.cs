using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdsManager : MonoBehaviour
{
    public static AdsManager Instance;
    //delegate   ()
    public delegate void RewardedAdResult(bool isSuccess);

    //event  
    public static event RewardedAdResult AdResult;
    public float timePerWatch = 90;
    float lastTimeWatch = -999;

    [Header("SHOW AD VICTORY/GAMEOVER")]
    public int showAdGameOverCounter = 2;
    int counter_gameOver = 0;
    public int showAdVictoryCounter = 1;
    int counter_victory = 0;
  
    private void Awake()
    {
        if (AdsManager.Instance != null)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void ShowAdmobBanner(bool show)
    {
        AdmobController.Instance.ShowBanner(show);
    }

    #region NORMAL AD

    public void ShowNormalAd(GameManager.GameState state)
    {
        Debug.Log("SHOW NORMAL AD " + state);

        if (state == GameManager.GameState.GameOver)
            StartCoroutine(ShowNormalAdCo(state, 0.8f));
        else
            StartCoroutine(ShowNormalAdCo(state, 0));
    }

    IEnumerator ShowNormalAdCo(GameManager.GameState state, float delay)
    {
        yield return new WaitForSeconds(delay);
        if (state == GameManager.GameState.GameOver)
        {
            counter_gameOver++;
            if (counter_gameOver >= showAdGameOverCounter)
            {
                    if (AdmobController.Instance.ForceShowInterstitialAd())
                    {
                        counter_gameOver = 0;
                    }
                
            }
        }
        else if (state == GameManager.GameState.Finish)
        {
            counter_victory++;
            if (counter_victory >= showAdVictoryCounter)
            {
                    if (AdmobController.Instance.ForceShowInterstitialAd())
                    {
                        counter_victory = 0;
                    }
                
            }
        }
    }

    public void ResetCounter()
    {
        counter_gameOver = 0;
        counter_victory = 0;
    }

    #endregion

    #region REWARDED VIDEO AD
    bool _isRewadedAdReady = false;

    public bool isRewardedAdReady()
    {
        if (_isRewadedAdReady)
            return true;

        if (AdmobController.Instance.isRewardedVideoAdReady())
        {
            _isRewadedAdReady = true;
            return true;
        }

        return false;
    }

    public float TimeWaitingNextWatch()
    {
        return timePerWatch - (Time.realtimeSinceStartup - lastTimeWatch);
    }

    public void ShowRewardedAds()
    {
        _isRewadedAdReady = false;
        lastTimeWatch = Time.realtimeSinceStartup;

        AdmobController.AdResult += AdmobController_AdResult;
        AdmobController.Instance.WatchRewardedVideoAd();
    }

    private void AdmobController_AdResult(bool isWatched)
    {
        AdmobController.AdResult -= AdmobController_AdResult;
        AdResult(true);
    }


    #endregion
}
