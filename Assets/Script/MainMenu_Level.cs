using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu_Level : MonoBehaviour
{
	int levelNumber = 1;
	public GameObject starGroup;
	public GameObject star1;
	public GameObject star2;
	public GameObject star3;

	public Text TextLevel;
	public GameObject Locked;

	public GameObject backgroundNormal, backgroundInActive;

	void Start()
	{
		levelNumber = int.Parse(gameObject.name);
		backgroundNormal.SetActive(true);
		backgroundInActive.SetActive(false);

		var levelReached = GlobalValue.LevelHighest;
		
		if ((levelNumber <= levelReached))
		{
			TextLevel.text = levelNumber.ToString();
			Locked.SetActive(false);

			var openLevel = levelReached + 1 >= levelNumber /*int.Parse(gameObject.name)*/;

			star1.SetActive(openLevel && GlobalValue.IsScrollLevelAte(1, levelNumber));
			star2.SetActive(openLevel && GlobalValue.IsScrollLevelAte(2, levelNumber));
			star3.SetActive(openLevel && GlobalValue.IsScrollLevelAte(3, levelNumber));

			Locked.SetActive(!openLevel);
			starGroup.SetActive(openLevel);

			bool isInActive = levelNumber == levelReached;
			
			backgroundNormal.SetActive(!isInActive);
			backgroundInActive.SetActive(isInActive);

			GetComponent<Button>().interactable = openLevel;
		}
		else
		{
			TextLevel.gameObject.SetActive(false);
			starGroup.SetActive(false);
			Locked.SetActive(true);
			GetComponent<Button>().interactable = false;
		}
	}

	public void LoadScene()
	{
		GlobalValue.levelPlaying = levelNumber;
		HomeMenu.Instance.LoadLevel();
	}
}

