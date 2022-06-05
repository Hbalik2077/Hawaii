using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RopePoint : MonoBehaviour
{
    public GameObject idleObj, activeObj;

    [Header("Active Slow motion to guide player")]
    public bool showTutorial = false;
    public GameObject activeHelperObj;
    public GameObject showReleaseTutObj;

    bool isWorking = false;
    bool grabLeftSide = false;

    private void Start()
    {
        if (activeHelperObj)
            activeHelperObj.SetActive(false);
    }

    void Update()
    {
        if (GameManager.Instance.Player.currentAvailableRope != null && GameManager.Instance.Player.currentAvailableRope.gameObject == gameObject)
        {
            if (!isWorking)
            {
                isWorking = true;
                grabLeftSide = GameManager.Instance.Player.transform.position.x < transform.position.x;
            }

            idleObj.SetActive(false);
            activeObj.SetActive(true);

            if (showTutorial)
            {
                if (!showReleaseTutObj.activeInHierarchy)
                {
                    if (activeHelperObj)
                        activeHelperObj.SetActive(true);
                }

                if (showReleaseTutObj && !showReleaseTutObj.activeInHierarchy)
                    if (GameManager.Instance.Player.isGrabingRope
                        && ((grabLeftSide && (GameManager.Instance.Player.transform.position.x > transform.position.x)) || (!grabLeftSide && (GameManager.Instance.Player.transform.position.x < transform.position.x)))
                        && GameManager.Instance.Player.transform.position.y > (transform.position.y - (Vector2.Distance(transform.position, GameManager.Instance.Player.transform.position) * 0.5f)))
                    {
                        activeHelperObj.SetActive(false);
                        showReleaseTutObj.SetActive(true);
                        Time.timeScale = 0.1f;
                    }
            }
        }
        else
        {
            idleObj.SetActive(true);
            activeObj.SetActive(false);
            isWorking = false;

            //if (showTutorial)
            //{
            if (activeHelperObj)
                activeHelperObj.SetActive(false);
            if (showReleaseTutObj)
                showReleaseTutObj.SetActive(false);
            //}
        }
    }

    private void OnDrawGizmos()
    {
        if (!showTutorial)
        {
            if (activeHelperObj)
                activeHelperObj.SetActive(false);
            if (showReleaseTutObj)
                showReleaseTutObj.SetActive(false);
        }
    }
}
