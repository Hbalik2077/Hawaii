using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SimplePathedMoving : MonoBehaviour
{
    public virtual void ReachPointEvent(int currentPoint) { }

    public bool use = true;
    [Header("Manual Control: Call Play()")]
    public bool manualControl = false;
    bool _manualControl = false;
    [Space]
    public float delayOnStart = 0;
    public AudioClip soundWhenMoveNextTarget;
    public float playSoundWhenPlayerInRange = 8;
    public Vector3[] localWaypoints;
    Vector3[] globalWaypoints;

    public float speed = 1;
    public bool cyclic;
    public bool loop = false;
    public float waitTime = 0.5f;
    [Range(0, 2)]
    public float easeAmount;

    protected int fromWaypointIndex;
    protected float percentBetweenWaypoints;
    float nextMoveTime;

    bool isPlaying = false;

    public void Init(float _delay, float _speed, Vector3[] _localWaypoints, bool _loop = false)
    {
        delayOnStart = _delay;
        speed = _speed;
        localWaypoints = _localWaypoints;
        loop = _loop;
    }

    public void Play()
    {
        _manualControl = false;
    }

    public virtual IEnumerator Start()
    {
        if (!use)
            Destroy(this);

        _manualControl = manualControl;
        while (_manualControl) { yield return new WaitForSeconds(0.1f); }

        if (delayOnStart > 0)
            yield return new WaitForSeconds(delayOnStart);
        if (localWaypoints.Length >= 2)
        {
            isPlaying = true;

            globalWaypoints = new Vector3[localWaypoints.Length];
            for (int i = 0; i < localWaypoints.Length; i++)
            {
                globalWaypoints[i] = localWaypoints[i] + transform.position;
            }
        }
    }

    void LateUpdate()
    {
        if (!isPlaying)
            return;

        Vector3 velocity = CalculatePlatformMovement();
        transform.Translate(velocity);
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    Vector3 CalculatePlatformMovement()
    {

        if (Time.time < nextMoveTime)
        {
            return Vector3.zero;
        }

        fromWaypointIndex %= globalWaypoints.Length;
        int toWaypointIndex = (fromWaypointIndex + 1) % globalWaypoints.Length;
        float distanceBetweenWaypoints = Vector3.Distance(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex]);
        percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
        percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
        float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

        Vector3 newPos = Vector3.Lerp(globalWaypoints[fromWaypointIndex], globalWaypoints[toWaypointIndex], easedPercentBetweenWaypoints);

        if (percentBetweenWaypoints >= 1)
        {
            percentBetweenWaypoints = 0;
            fromWaypointIndex++;

            if (Vector2.Distance(GameManager.Instance.Player.transform.position, transform.position) < playSoundWhenPlayerInRange)
                SoundManager.PlaySfx(soundWhenMoveNextTarget);

            if (!cyclic)
            {
                if (fromWaypointIndex >= globalWaypoints.Length - 1)
                {
                    if (!loop)
                    {
                        Destroy(this);
                    }
                    fromWaypointIndex = 0;
                    System.Array.Reverse(globalWaypoints);
                }
            }
            nextMoveTime = Time.time + waitTime;
            ReachPointEvent(fromWaypointIndex);
        }

        return newPos - transform.position;
    }

    protected virtual void OnDrawGizmos()
    {
        if (!use || Application.isPlaying || !enabled)
            return;

        if (localWaypoints != null)
        {
            float size = .3f;

            for (int i = 0; i < localWaypoints.Length; i++)
            {

                Gizmos.color = Color.red;
                Vector3 globalWaypointPos = (Application.isPlaying) ? globalWaypoints[i] : localWaypoints[i] + transform.position;

                Gizmos.DrawWireSphere(globalWaypointPos, size);

                if (i + 1 >= localWaypoints.Length)
                {
                    if (cyclic)
                    {
                        Gizmos.color = Color.yellow;
                        if (Application.isPlaying)
                            Gizmos.DrawLine(globalWaypoints[i], globalWaypoints[0]);
                        else
                            Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[0] + transform.position);
                    }
                    return;
                }

                Gizmos.color = Color.green;
                Gizmos.DrawLine(localWaypoints[i] + transform.position, localWaypoints[i + 1] + transform.position);
            }
        }
    }
}
