using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeController : MonoBehaviour
{
    public enum PipeDirection { None, Top2Down, Left2Right }
    [HideInInspector] public PipeDirection pipeDirection;
    public Transform nextPoint;
    public LayerMask playerLayer;
    public AudioClip soundIn, soundOut;

    private void Start()
    {
        if (pipeDirection == PipeDirection.None)
        {
            enabled = false;
            return;
        }
        else
            ControllerInput.inputEvent += ControllerInput_inputEvent;
    }

    private void ControllerInput_inputEvent(Vector2 direction)
    {
        if ((direction.y == -1 && pipeDirection == PipeDirection.Top2Down) || (direction.x == 1 && pipeDirection == PipeDirection.Left2Right))
        {
            RaycastHit hit;
            if (Physics.SphereCast(transform.position, 0.1f, direction * -1, out hit, 2, playerLayer))
            {
                if (pipeDirection == PipeDirection.Top2Down)
                {
                    if (Mathf.Abs(hit.collider.gameObject.transform.position.x - transform.position.x) <= 0.4f)
                    {
                        GameManager.Instance.Player.AddPosition(new Vector2(transform.position.x- hit.collider.gameObject.transform.position.x , 0));
                        StartCoroutine(ActionCo());
                    }
                }else
                    StartCoroutine(ActionCo());
            }
        }
    }

    private void OnDisable()
    {
        ControllerInput.inputEvent -= ControllerInput_inputEvent;
    }

    IEnumerator ActionCo()
    {
        SoundManager.PlaySfx(soundIn);
        ControllerInput.Instance.ShowController(false);
        GameManager.Instance.SetGameState(GameManager.GameState.Waiting);
        if (pipeDirection == PipeDirection.Top2Down)
        {
            GameManager.Instance.Player.SetInThePipe(true, Vector2.down);
        }
        else if (pipeDirection == PipeDirection.Left2Right)
        {
            GameManager.Instance.Player.SetInThePipe(true, Vector2.right);
        }
        yield return new WaitForSeconds(1f);
        GameManager.Instance.Player.SetInThePipe(false, Vector2.zero);
        GameManager.Instance.Player.SetPosition(nextPoint.position);
        //GameManager.Instance.Player.transform.position = nextPoint.position;
        SoundManager.PlaySfx(soundOut);
        yield return new WaitForSeconds(0.5f);
        GameManager.Instance.SetGameState(GameManager.GameState.Playing);
        ControllerInput.Instance.ShowController(true);
    }

    private void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;

        Gizmos.color = Color.white;
        if (pipeDirection != PipeDirection.None)
        {
            nextPoint.gameObject.SetActive(true);
            Gizmos.DrawLine(transform.position, nextPoint.position);
        }
        else
            nextPoint.gameObject.SetActive(false);
    }
}
