using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlowMotionHeper : MonoBehaviour
{
    //public float slowTime = 0.1f;
    //public GameObject activeObj;
    //public enum ACTIVE_WHEN_PLAYER { Contact, WallSliding }
    //public ACTIVE_WHEN_PLAYER activeWhenPlayer = ACTIVE_WHEN_PLAYER.Contact;
    //public bool activeButton = false;
    //public enum WAIT_FOR { AnyKey, Jump, Slide }
    //public WAIT_FOR waitFor;
    //bool isWorking = false;
    //[ReadOnly] public bool isDone = false;        //player pass this tutorial or not

    //private void Start()
    //{
    //    if (activeObj)
    //        activeObj.SetActive(false);
    //    if (activeButton)
    //    {
    //        switch (waitFor)
    //        {
    //            case WAIT_FOR.Jump:
    //                ControllerInput.Instance.SetJumpButton(false, false);
    //                break;
    //            case WAIT_FOR.Slide:
    //                ControllerInput.Instance.SetSlideButton(false, false);
    //                break;
    //        }
    //    }
    //}

    //private void OnEnable()
    //{
    //    if (activeButton)
    //        GameManager.playerRebornEvent += OnPlayerReborn;
    //}

    //private void OnDisable()
    //{
    //    if (activeButton)
    //        GameManager.playerRebornEvent -= OnPlayerReborn;
    //}

    //private IEnumerator OnTriggerEnter(Collider other)
    //{
    //    if (isWorking)
    //        yield break;

    //    if (other.gameObject == GameManager.Instance.Player.gameObject)
    //    {
    //        isWorking = true;

    //        if (activeWhenPlayer == ACTIVE_WHEN_PLAYER.WallSliding)
    //        {
    //            if (activeObj)
    //                activeObj.SetActive(true);
    //            while (!GameManager.Instance.Player.isWallSliding) { yield return null; }
    //            Time.timeScale = slowTime;
    //            while (GameManager.Instance.Player.isWallSliding) { yield return null; }
    //            Time.timeScale = 1;
    //        }
    //        else
    //        {
    //            if (activeObj)
    //                activeObj.SetActive(true);
    //            Time.timeScale = slowTime;
    //            switch (waitFor)
    //            {
    //                case WAIT_FOR.AnyKey:
    //                    while (true)
    //                    {
    //                        if (Input.anyKeyDown)
    //                        {
    //                            Time.timeScale = 1;
    //                            yield break;
    //                        }
    //                        yield return null;
    //                    }
    //                case WAIT_FOR.Jump:
    //                    if (activeButton)
    //                        ControllerInput.Instance.SetJumpButton(true, true);
    //                    while (true)
    //                    {
    //                        if (!GameManager.Instance.Player.isGrounded)
    //                        {
    //                            ControllerInput.Instance.SetJumpButton(false, true);
    //                            isDone = true;
    //                            Time.timeScale = 1;
    //                            yield break;
    //                        }
    //                        yield return null;
    //                    }
    //                case WAIT_FOR.Slide:
    //                    if (activeButton)
    //                        ControllerInput.Instance.SetSlideButton(true, true);
    //                    while (true)
    //                    {
    //                        if (GameManager.Instance.Player.isSliding)
    //                        {
    //                            ControllerInput.Instance.SetSlideButton(false, true);
    //                            isDone = true;
    //                            Time.timeScale = 1;
    //                            yield break;
    //                        }
    //                        yield return null;
    //                    }
    //            }
    //        }
    //    }
    //}

    //private void OnTriggerExit(Collider other)
    //{
    //    if (other.gameObject == GameManager.Instance.Player.gameObject)
    //    {
    //        if (activeObj)
    //            activeObj.SetActive(false);
    //        Time.timeScale = 1;
    //        StopAllCoroutines();

    //        if (activeButton)
    //        {
    //            if (isWorking && !isDone)
    //            {
    //                switch (waitFor)
    //                {
    //                    case WAIT_FOR.Jump:
    //                        ControllerInput.Instance.SetJumpButton(false, false);
    //                        break;
    //                    case WAIT_FOR.Slide:
    //                        ControllerInput.Instance.SetSlideButton(false, false);
    //                        break;
    //                }
    //            }
    //        }
    //        isWorking = false;
    //    }
    //}

    //void OnPlayerReborn()
    //{
    //    if (activeButton)
    //    {
    //        if (isWorking && !isDone)
    //        {
    //            switch (waitFor)
    //            {
    //                case WAIT_FOR.Jump:
    //                    ControllerInput.Instance.SetJumpButton(false, false);
    //                    isWorking = false;
    //                    break;
    //                case WAIT_FOR.Slide:
    //                    ControllerInput.Instance.SetSlideButton(false, false);
    //                    isWorking = false;
    //                    break;
    //            }
    //        }
    //    }
    //    isWorking = false;
    //}
}
