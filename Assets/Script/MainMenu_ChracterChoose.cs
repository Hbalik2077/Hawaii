using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class MainMenu_ChracterChoose : MonoBehaviour
{
	[Tooltip("The unique character ID")]
	public int characterID;
	public int price;
	public PlayerController character;

	public bool unlockDefault = false;

	//	public GameObject Locked;
	public GameObject UnlockButton;

	public Text pricetxt;
	public Text state;

	bool isUnlock;
	SoundManager soundManager;

	void Start()
	{
		soundManager = FindObjectOfType<SoundManager>();

		if (unlockDefault)
			isUnlock = true;
		else
			isUnlock = GlobalValue.IsCharUnlocked(characterID) ? true : false;

		UnlockButton.SetActive(!isUnlock);

		pricetxt.text = price.ToString();
	}

	void Update()
	{
		if (!isUnlock)
			return;

		if (PlayerPrefs.GetInt(GlobalValue.ChoosenCharacterID, 1) == characterID)
		{
			state.color = Color.blue;
			state.text = "Equipped";
		}
		else
		{
			state.color = Color.black;
			state.text = "Equip";
		}
	}

	public void Unlock()
	{
		if (GlobalValue.SavedCoins >= price)
		{
			GlobalValue.SavedCoins -= price;
			DoUnlock();
		}
	}

	void DoUnlock()
    {
		//PlayerPrefs.SetInt(GlobalValue.Character + characterID, 1);
		GlobalValue.UnlockChar(characterID);
		isUnlock = true;
		//Locked.SetActive (false);
		UnlockButton.SetActive(false);
		SoundManager.PlaySfx(SoundManager.Instance.soundPurchased);
	}

	public void Pick()
	{
		SoundManager.Click();
		if (!isUnlock)
		{
			Unlock();
			return;
		}

		PlayerPrefs.SetInt(GlobalValue.ChoosenCharacterID, characterID);
		//PlayerPrefs.SetInt(GlobalValue.ChoosenCharacterInstanceID, CharacterPrefab.GetInstanceID());
		GlobalValue.ChooseCharacterID = character.playerID;
		//CharacterHolder.Instance.CharacterPicked = CharacterPrefab;
	}
}
