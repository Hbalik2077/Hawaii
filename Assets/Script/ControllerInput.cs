using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasGroup))]
public class ControllerInput : MonoBehaviour
{
    public static ControllerInput Instance;

    public delegate void InputEvent(Vector2 direction);
    public static event InputEvent inputEvent;

    public GameObject btnJetpack;
    public GameObject btnSlide;

    CanvasGroup canvasGroup;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        //if (GameManager.Instance.Player.IgnoreControllerInput())
        //{
        //    canvasGroup.interactable = false;
        //    canvasGroup.blocksRaycasts = false;
        //}
        //else
        //{
        //    canvasGroup.interactable = true;
        //    canvasGroup.blocksRaycasts = true;
        //}

        btnJetpack.SetActive(GameManager.Instance.Player.isJetpackActived);
        btnSlide.SetActive(GameManager.Instance.Player.isRunning);
    }

    public void ShowController(bool show)
    {
        if (show)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.alpha = 1;
        }
        else
        {
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.alpha = 0;
        }
    }

    [ReadOnly] public bool allowJump = true;
    [ReadOnly] public bool allowSlide = true;

    public void Jump()
    {
        if (allowJump)
            GameManager.Instance.Player.Jump();
    }

    public void JumpOff()
    {
        if (allowJump)
            GameManager.Instance.Player.JumpOff();
    }

    public void SlideOn()
    {
        if (allowSlide)
            GameManager.Instance.Player.SlideOn();
    }

    public void MoveLeft()
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            GameManager.Instance.Player.MoveLeft();

            //isMovingLeft = true;
            if (inputEvent != null)
                inputEvent(Vector2.left);
        }
    }

    public void MoveLeftTap()
    {
        GameManager.Instance.Player.MoveLeftTap();
    }

    public void MoveRight()
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            GameManager.Instance.Player.MoveRight();
            //isMovingRight = true;
            if (inputEvent != null)
                inputEvent(Vector2.right);
        }
    }

    public void MoveRightTap()
    {
        GameManager.Instance.Player.MoveRightTap();
    }

    public void MoveDown()
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            GameManager.Instance.Player.MoveDown();
            if (inputEvent != null)
                inputEvent(Vector2.down);
        }
    }

    public void StopMove(int fromDirection)
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            GameManager.Instance.Player.StopMove(fromDirection);
            //isMovingLeft = false;
            //isMovingRight = false;
            if (inputEvent != null)
                inputEvent(Vector2.zero);
        }
    }

    public void RangeAttack()
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            GameManager.Instance.Player.RangeAttack();
        }
    }

    public void MeleeAttack()
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing) 
            GameManager.Instance.Player.MeleeAttack();
    }

    public void UseJetpack(bool use)
    {
        GameManager.Instance.Player.UseJetpack(use);
    }
}
