using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalValue : MonoBehaviour
{
    public static int levelPlaying = -1;
    public static bool isSound = true;
    public static bool isMusic = true;


    public static string Character = "Character";
    public static string ChoosenCharacterID = "choosenCharacterID";
    public static string ChoosenCharacterInstanceID = "ChoosenCharacterInstanceID";
    public static GameObject CharacterPrefab;

    public static int SavedCoins
    {
        get { return PlayerPrefs.GetInt("SavedCoins", 100); }
        set { PlayerPrefs.SetInt("SavedCoins", value); }
    }

    public static int SavedLive
    {
        get { return PlayerPrefs.GetInt("SavedLive", 5); }
        set { PlayerPrefs.SetInt("SavedLive", value); }
    }

    public static int Bullets
    {
        get
        {
            int bullets = PlayerPrefs.GetInt("Bullets", 0);
            return bullets;
        }
        set { PlayerPrefs.SetInt("Bullets", value); }
    }

    public static int LevelHighest
    {
        get { return PlayerPrefs.GetInt("LevelHighest", 1); }
        set { PlayerPrefs.SetInt("LevelHighest", value); }
    }

    public static void SetScrollLevelAte(int bigStar, int level)
    {
        PlayerPrefs.SetInt("ATE" + level + bigStar, 1);
    }

    public static bool IsScrollLevelAte(int bigStar, int level)
    {
        //Debug.LogError(scrollID + ":" + (PlayerPrefs.GetInt("AteScroll" + levelPlaying + scrollID, 0) == 1));
        return PlayerPrefs.GetInt("ATE" + level + bigStar, 0) == 1 ? true : false;
    }

    public static int ChooseCharacterID
    {
        get { return PlayerPrefs.GetInt("ChooseCharacterID", 0); }
        set { PlayerPrefs.SetInt("ChooseCharacterID", value); }
    }

    public static void UnlockChar(int ID)
    {
        PlayerPrefs.SetInt("UnlockChar" + ID, 1);
    }

    public static bool IsCharUnlocked(int ID)
    {
        return PlayerPrefs.GetInt("UnlockChar" + ID, 0) == 1 ? true : false;
    }
}