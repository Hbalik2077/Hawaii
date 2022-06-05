using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public bool isTestLevel = false;
    public static GameManager Instance;
    public enum GameState { Waiting, Playing, GameOver, Finish }
    [ReadOnly] public GameState gameState;
    [ReadOnly] public PlayerController playerController;

    //define player reborn event, it will called all registered objects
    public delegate void OnPlayerReborn();
    public static OnPlayerReborn playerRebornEvent;


    public PlayerController Player
    {
        get
        {
            if (playerController != null)
                return playerController;
            else
            {
                playerController = FindObjectOfType<PlayerController>();
                if (playerController)
                    return playerController;
                else
                    return null;
            }
        }
    }

    public void SetGameState(GameState state)
    {
        gameState = state;
    }

    [ReadOnly] public Vector3 checkPoint;
    public void SetCheckPoint(Vector3 pos)
    {
        checkPoint = pos;
    }

    public void Awake()
    {
        Instance = this;
        var _startPoint = GameObject.Find("Startpoint");
        if (_startPoint)
        {
            Player.gameObject.transform.position = _startPoint.transform.position;
            SetCheckPoint(_startPoint.transform.position);
        }
        else
            Debug.Log("Can't  find the Startpoint on the scene! Please check and make sure it visible on the Scene level!");

        if (CharacterHolder.Instance != null)
        {
            if (Player != null)
                Destroy(Player.gameObject);

            Instantiate(CharacterHolder.Instance.GetPickedCharacter(), Player.transform.position, Player.transform.rotation);
        }
        else
        {
            var FindCharacterHolder = FindObjectOfType<CharacterHolder>();
            if (FindCharacterHolder)
            {
                if (Player != null)
                    Destroy(Player.gameObject);

                Instantiate(FindCharacterHolder.GetPickedCharacter(), Player.transform.position, Player.transform.rotation);
            }
        }
    }

    private void Start()
    {
        gameState = GameState.Playing;
        SoundManager.PlayGameMusic();
    }

    public void GameOver(bool forceGameover = false)
    {
        if (gameState == GameState.GameOver)
            return;
        SoundManager.Instance.PauseMusic(true);
        Time.timeScale = 1;
        gameState = GameState.GameOver;
        if (forceGameover)
            SoundManager.PlaySfx(SoundManager.Instance.soundGameover);
        if (AdsManager.Instance)
            AdsManager.Instance.ShowNormalAd(GameState.GameOver);
        if (isTestLevel || ( !forceGameover /* && GlobalValue.SavedLive > 0*/))
            MenuManager.Instance.ShowAskForContinue();
        else
            MenuManager.Instance.GameOver();
    }

    public void FinishGame()
    {
        if (gameState == GameState.Finish)
            return;
        
        gameState = GameState.Finish;

        if(GlobalValue.levelPlaying >= GlobalValue.LevelHighest)
        {
            GlobalValue.LevelHighest++;
        }

        MenuManager.Instance.Finish();
        if (AdsManager.Instance)
            AdsManager.Instance.ShowNormalAd(GameState.Finish);

        SoundManager.PlaySfx(SoundManager.Instance.soundGamefinish);
    }

    public void Continue()
    {
        if (playerRebornEvent != null)
            playerRebornEvent();

        SoundManager.Instance.PauseMusic(false);

        //Destroy(Player.gameObject);
        //playerController = Instantiate(CharacterHolder.Instance.GetPickedCharacter(), checkPoint, Quaternion.identity).GetComponent<PlayerController>();
        //gameState = GameState.Playing;

        Invoke("SpawnPlayer", 0.1f);
    }

    void SpawnPlayer()
    {
        Destroy(playerController.gameObject);
        playerController = Instantiate(CharacterHolder.Instance.GetPickedCharacter(), checkPoint, Quaternion.identity).GetComponent<PlayerController>();
        gameState = GameState.Playing;
    }
}