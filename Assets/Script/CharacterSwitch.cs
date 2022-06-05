using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class CharacterSwitch : MonoBehaviour
{
    //	public Transform BlockLevel;
    public RectTransform BlockLevel;
    public int howManyBlocks = 3;
    public float step = 720f;
    public Image[] Dots;
    private float newPosX = 0;

    int currentPos = 0;
    public AudioClip music;

    public Button btnNext, btnPre;

    void OnEnable()
    {
        SoundManager.PlayMusic(music);
        Debug.LogWarning("ON ENALBE");
    }

    void Start()
    {
        SetDots();
    }

    void SetDots()
    {
        foreach (var obj in Dots)
        {
            obj.color = new Color(1, 1, 1, 0.5f);
            obj.rectTransform.sizeDelta = new Vector2(28, 28);
        }
        Dots[currentPos].color = Color.white;
        Dots[currentPos].rectTransform.sizeDelta = new Vector2(38, 38);

        btnNext.interactable = currentPos < howManyBlocks - 1;
        btnPre.interactable = currentPos > 0;
    }

    void OnDisable()
    {
        //if (SoundManager.Instance)
        //    SoundManager.PlayMusic(SoundManager.Instance.musicsGame);
    }

    public void SetCurrentWorld(int world)
    {
        currentPos = (world - 1);
        newPosX = 0;
        newPosX -= step * (world - 1);
        newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        SetMapPosition();
        SetDots();
    }

    public void SetMapPosition()
    {
        BlockLevel.anchoredPosition = new Vector2(newPosX, BlockLevel.anchoredPosition.y);
    }

    bool allowPressButton = true;
    public void Next()
    {
        if (allowPressButton)
        {
            StartCoroutine(NextCo());
            SoundManager.Click();
        }
    }

    IEnumerator NextCo()
    {
        allowPressButton = false;

        if (newPosX != (-step * (howManyBlocks - 1)))
        {
            currentPos++;

            newPosX -= step;
            newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        }
        else
        {
            allowPressButton = true;
            yield break;

            //currentPos = 0;

            //newPosX = 0;
            //newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);


        }


        SetMapPosition();

        SetDots();


        allowPressButton = true;

    }

    public void Pre()
    {
        if (allowPressButton)
        {
            StartCoroutine(PreCo());
            SoundManager.Click();
        }
    }

    IEnumerator PreCo()
    {
        allowPressButton = false;
        if (newPosX != 0)
        {
            currentPos--;

            newPosX += step;
            newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);


        }
        else
        {
            allowPressButton = true;
            yield break;
            //currentPos = howManyBlocks - 1;

            //newPosX = -999999;
            //newPosX = Mathf.Clamp(newPosX, -step * (howManyBlocks - 1), 0);

        }
        SetMapPosition();

        SetDots();


        allowPressButton = true;

    }
}
