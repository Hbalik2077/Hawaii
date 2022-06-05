using UnityEngine;
using System.Collections;
using UnityEngine.UI;
public class MainMenuShopItems : MonoBehaviour {
	[Header("---PRICE---")]
	public int livePrice;

    public AudioClip boughtSound;
	[Range(0,1)]
	public float boughtSoundVolume = 0.5f;

	public Text livePriceTxt;
    public Text livesTxt;
	
    // Use this for initialization
    void Start () {
		livePriceTxt.text = livePrice.ToString ();
    }
	
	public void BuyLive(){
		if (GlobalValue.SavedCoins >= livePrice) {
			GlobalValue.SavedCoins -= livePrice;
			SoundManager.PlaySfx (boughtSound, boughtSoundVolume);
		}
	}
}
