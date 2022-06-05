using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public static MenuManager Instance;
    public GameObject uI, gameOver, finish, pauseUI, askForContinue, LoadingUI;
    public Text[] txtStars;
    public Text[] txtLives;
    public Text[] txtLevels;
    public Text txtBullet;
    public GameObject[] butNext;

    [Header("Progressing Bar")]
    public Slider progressSlider;
    float startPos, finishPos;

    [Header("Jetpack bar")]
    public Slider jetpackSlider;
    public Text txtJetpackRemainPercent;

    public Image star1, star2, star3;
    public Color collectColor = Color.yellow;
    bool isCollectStar1, isCollectStar2, isCollectStar3;
    #region STARS
    [Header("Sound and Music")]
    public Image soundImage;
    public Image musicImage;
    public Sprite soundImageOn, soundImageOff, musicImageOn, musicImageOff;

    public void CollectStar(int ID)
    {
        switch (ID)
        {
            case 1:
                star1.color = collectColor;
                isCollectStar1 = true;
                break;
            case 2:
                star2.color = collectColor;
                isCollectStar2 = true;
                break;
            case 3:
                star3.color = collectColor;
                isCollectStar3 = true;
                break;
            default:
                break;
        }
    }

    #endregion

    private void OnEnable()
    {
        if (isCollectStar1)
        {
            star1.color = collectColor;
        }
        if (isCollectStar2)
        {
            star2.color = collectColor;
        }
        if (isCollectStar3)
        {
            star3.color = collectColor;
        }
    }

    private void Awake()
    {
        Instance = this;

       
    }

    void Start()
    {
        uI.SetActive(true);
        gameOver.SetActive(false);
        pauseUI.SetActive(false);
        askForContinue.SetActive(false);
        LoadingUI.SetActive(false);

        if (Time.timeScale == 0)
            Time.timeScale = 1;

        if (GameManager.Instance)
            startPos = GameManager.Instance.Player.transform.position.x;

        var hasFinishPoint = GameObject.FindGameObjectWithTag("Finish");
        if (hasFinishPoint)
            finishPos = hasFinishPoint.transform.position.x;
        else
            Debug.LogError("NO FINISH POINT ON SCENE!");

        foreach (var txt in txtLevels)
        {
            if (GlobalValue.levelPlaying == -1)
                txt.text = "TEST ALL FEATURES";
            else
                txt.text = "Level " + GlobalValue.levelPlaying;
        }

        if (soundImage)
            soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;
        if (musicImage)
            musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;
        if (!GlobalValue.isSound)
            SoundManager.SoundVolume = 0;
        if (!GlobalValue.isMusic)
            SoundManager.MusicVolume = 0;
    }

    #region Music and Sound
    public void TurnSound()
    {
        GlobalValue.isSound = !GlobalValue.isSound;
        soundImage.sprite = GlobalValue.isSound ? soundImageOn : soundImageOff;

        SoundManager.SoundVolume = GlobalValue.isSound ? 1 : 0;
        SoundManager.Click();
    }

    public void TurnMusic()
    {
        GlobalValue.isMusic = !GlobalValue.isMusic;
        musicImage.sprite = GlobalValue.isMusic ? musicImageOn : musicImageOff;

        SoundManager.MusicVolume = GlobalValue.isMusic ? SoundManager.Instance.musicsGameVolume : 0;
        SoundManager.Click();
    }
    #endregion

    void Update()
    {
        foreach(var txt in txtStars)
        {
            txt.text = "x" + GlobalValue.SavedCoins;
        }

        foreach (var txt in txtLives)
        {
            txt.text = GameManager.Instance.isTestLevel? "Test level" :  "x" + GlobalValue.SavedLive;
        }

        txtBullet.text = GameManager.Instance.isTestLevel ? "Test Level" : (GlobalValue.Bullets + "");

        progressSlider.value = Mathf.InverseLerp(startPos, finishPos, GameManager.Instance.Player.transform.position.x);

        //update jetpack
        jetpackSlider.gameObject.SetActive(GameManager.Instance.Player.isJetpackActived);
        jetpackSlider.value = GameManager.Instance.Player.jetpackRemainTime / GameManager.Instance.Player.jetpackDrainTimeOut;
        txtJetpackRemainPercent.text = ((GameManager.Instance.Player.jetpackRemainTime / GameManager.Instance.Player.jetpackDrainTimeOut) * 100).ToString("0") + "%";
    }

    public void Finish()
    {
        Invoke("FinishCo", 2);
    }

    void FinishCo()
    {
        foreach (var but in butNext)
        {
            if (!GameManager.Instance.isTestLevel)
                but.SetActive(GlobalValue.levelPlaying < GlobalValue.LevelHighest);
            else
            {
                but.SetActive(false);
            }
        }

        uI.SetActive(false);
        finish.SetActive(true);
    }

    public void ShowAskForContinue()
    {
        Invoke("ShowAskForContinueCo", 1);
    }

    void ShowAskForContinueCo()
    {
        uI.SetActive(false);
        askForContinue.SetActive(true);
    }

    public void Continue()
    {
        uI.SetActive(true);
        askForContinue.SetActive(false);
        GameManager.Instance.Continue();
    }

    public void GameOver()
    {
        Invoke("GameOverCo", 1);
    }

    void GameOverCo()
    {
        foreach (var but in butNext)
        {
            if (!GameManager.Instance.isTestLevel)
                but.SetActive(GlobalValue.levelPlaying < GlobalValue.LevelHighest);
            else
                but.SetActive(false);
        }

        uI.SetActive(false);
        gameOver.SetActive(true);
    }

    public void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void Pause(bool pause)
    {
        pauseUI.SetActive(pause);
        Time.timeScale = pause ? 0 : 1;

        SoundManager.Instance.PauseMusic(pause);
    }

    public void NextLevel()
    {
        GlobalValue.levelPlaying++;
        LoadingUI.SetActive(true);
        SceneManager.LoadSceneAsync("Level " + GlobalValue.levelPlaying);
    }

    public void Home()
    {
        GlobalValue.levelPlaying = -1;
        LoadingUI.SetActive(true);
        SceneManager.LoadSceneAsync("HomeScene");
    }
}
