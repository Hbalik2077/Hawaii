using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmartRotationObject : MonoBehaviour, ICanTakeDamage
{
    public float delay = 0;
    public enum StartRotate { AutoRotate, HitToRotate}
    public enum StartDirection { Left, Right }
    public AudioClip soundHit;
    [Header("ObstacleRotation")]
    public StartRotate startAction;
    public StartDirection startDirection;
    [Tooltip("Disable all owner collider2D when rotating")]
    public bool disableColliderWhenWork = false;

    public float angle = 45;
    public float speed = 1;
    public float smooth = 0.1f;
    [Range(0, 2)]
    public float easeAmount;

    float from = 0;
    float to = 0;

    [Header("Draw line")]
    public bool drawLine = false;
    public List<Transform> objectLocalWaypoints;
    public float lineWidth = 0.2f;
    public Material lineMat;
    public float offsetLineZ = -1;
    LineRenderer lineRen;

    float percentBetweenWaypoints;
    bool isWorking = false;
    
    IEnumerator Start()
    {
        yield return new WaitForSeconds(delay);
        lineRen = GetComponent<LineRenderer>();
        if (!lineRen)
            lineRen = gameObject.AddComponent<LineRenderer>();

        lineRen.positionCount = objectLocalWaypoints.Count;
        lineRen.useWorldSpace = true;
        lineRen.startWidth = lineWidth;
        lineRen.material = lineMat;
        lineRen.textureMode = LineTextureMode.Tile;
        

        if (startAction == StartRotate.AutoRotate)
        {
            Work(startDirection);
        }

        Quaternion.Euler(0, 0, 0);
    }

    void Work(StartDirection moveDirection)
    {
        if (moveDirection == StartDirection.Left)
        {
            from = angle * 2;
            to = 0;
        }
        else
        {
            from = 0;
            to = angle * 2;
        }

        //GetComponent<Collider2D>().enabled = false;     //disable self collider
        percentBetweenWaypoints = 0.5f;
        isWorking = true;

        if (disableColliderWhenWork)
        {
            var cols = GetComponents<Collider2D>();
            if(cols.Length > 0)
            {
                foreach(var col in cols)
                {
                    col.enabled = false;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!isWorking)
            return;

        if (drawLine && objectLocalWaypoints.Count >= 2)
        {
            for (int i = 0; i < objectLocalWaypoints.Count; i++)
                lineRen.SetPosition(i, objectLocalWaypoints[i].position + Vector3.forward * offsetLineZ);
        }

        if (isWorking)
        {

            float distanceBetweenWaypoints = Mathf.Abs(from - to);
            percentBetweenWaypoints += Time.deltaTime * speed / distanceBetweenWaypoints;
            percentBetweenWaypoints = Mathf.Clamp01(percentBetweenWaypoints);
            float easedPercentBetweenWaypoints = Ease(percentBetweenWaypoints);

            float newAngle = Mathf.Lerp(from, to, easedPercentBetweenWaypoints);

            if (percentBetweenWaypoints >= 1)
            {
                percentBetweenWaypoints = 0;
                if (from == 0)
                {
                    from = to;
                    to = 0;
                }
                else
                {
                    to = from;
                    from = 0;
                }
            }
            transform.rotation = Quaternion.Euler(0, 0, newAngle - angle);
        }
    }

    float Ease(float x)
    {
        float a = easeAmount + 1;
        return Mathf.Pow(x, a) / (Mathf.Pow(x, a) + Mathf.Pow(1 - x, a));
    }

    void OnDrawGizmos()
    {
        if (Application.isPlaying)
            return;
        lineRen = GetComponent<LineRenderer>();

        if (drawLine && objectLocalWaypoints.Count >= 2)
        {
            if (!lineRen)
                lineRen = gameObject.AddComponent<LineRenderer>();

            lineRen.positionCount = objectLocalWaypoints.Count;
            lineRen.useWorldSpace = true;
            lineRen.startWidth = lineWidth;
            lineRen.material = lineMat;
            lineRen.textureMode = LineTextureMode.Tile;

            for (int i = 0; i < objectLocalWaypoints.Count; i++)
                lineRen.SetPosition(i, objectLocalWaypoints[i].position + Vector3.forward * offsetLineZ);

        }
        else if (lineRen)
            DestroyImmediate(lineRen);

    }

    void ICanTakeDamage.TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        SoundManager.PlaySfx(soundHit);
        if (isWorking)
            return;

        Work(transform.position.x > instigator.transform.position.x ? StartDirection.Right : StartDirection.Left);
        
    }
}
