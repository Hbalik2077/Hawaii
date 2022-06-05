using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class ResetData : MonoBehaviour
{
    public void Reset()
    {
        PlayerPrefs.DeleteAll();
        SoundManager.Click();
        SceneManager.LoadSceneAsync(SceneManager.GetActiveScene().buildIndex);
        Debug.Log("RESETED ALL DATA!");
    }
}
