using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEnemy : MonoBehaviour, ICanTakeDamage
{
    public bool canBeKillWhenPlayerJumpOn = false;
    public float moveSpeed = 2;
    public float gravity = -9.8f;
    public int horizontalInput = -1;
    public LayerMask layerAsGround;
    public LayerMask layerAsWall;
    public AudioClip soundDie;
    [ReadOnly] public bool isGrounded = false;
    CharacterController characterController;
    [ReadOnly] public Vector2 velocity;
    bool isDead = false;
    Animator anim;

    public bool allowCheckGroundAhead = false;

    [Header("*** PATROL ***")]
    public bool usePatrol = false;
    [Range(-10,-1)]
    public float limitLocalLeft = -2;
    [Range(1, 10)]
    public float limitLocalRight = 2;
    [ReadOnly] public float limitLeft, limitRight;

    [Header("+++ FIRE BULLET +++")]
    public bool allowFireBullet = false;
    public float fireRate = 3;
    public float checkDistance = 8;
    public Projectile bullet;
    public float bulletSpeed = 6;
    public Transform firePosition;
    public LayerMask layerAsTarget;
    public AudioClip soundShoot;
    float lastShoot = -999;

    bool isDetectPlayer = false;
    private void Start()
    {
        anim = GetComponent<Animator>();
        characterController = GetComponent<CharacterController>();

        if (allowFireBullet)
            StartCoroutine(CheckAndShootTarget());

        limitLeft = transform.position.x + limitLocalLeft;
        limitRight = transform.position.x + limitLocalRight;
    }

    IEnumerator CheckAndShootTarget()
    {
        while (true)
        {
            RaycastHit hit;
            if (Physics.Raycast(firePosition.position, (transform.rotation.eulerAngles.y > 45 && transform.rotation.eulerAngles.y < 145) ? Vector3.right : Vector3.left, out hit, checkDistance, layerAsTarget))
            {
                isDetectPlayer = true;
                if (Time.time > (lastShoot + fireRate))
                {
                    //check if nothing block the bullet line
                    if (!Physics.Linecast(new Vector3(transform.position.x, firePosition.position.y, 0), hit.point))
                    {
                        Vector2 firePoint = firePosition.position;

                        var projectile = Instantiate(bullet.gameObject, firePoint, bullet.gameObject.transform.rotation).GetComponent<Projectile>();
                        projectile.Initialize(gameObject, horizontalInput > 0 ? Vector2.right : Vector2.left, Vector2.zero, false, false, 1, bulletSpeed);
                        projectile.gameObject.SetActive(true);

                        SoundManager.PlaySfx(soundShoot);
                        lastShoot = Time.time;

                        anim.SetTrigger("shoot");
                    }else
                        isDetectPlayer = false;
                }
            }
            else
                isDetectPlayer = false;

            yield return null;
        }
    }

    private void OnDrawGizmos()
    {
        if (allowFireBullet)
        {
            Gizmos.DrawLine(firePosition.position, firePosition.position + Vector3.left * checkDistance);
        }

        if (usePatrol)
        {
            if (Application.isPlaying)
            {
                var lPos = transform.position;
                lPos.x = limitLeft;
                var rPos = transform.position;
                rPos.x = limitRight;
                Gizmos.DrawWireCube(lPos, Vector3.one*0.2f);
                Gizmos.DrawWireCube(rPos, Vector3.one * 0.2f);
                Gizmos.DrawLine(lPos, rPos);
            }
            else
            {
                Gizmos.DrawWireCube(transform.position + Vector3.right * limitLocalLeft, Vector3.one * 0.2f);
                Gizmos.DrawWireCube(transform.position + Vector3.right * limitLocalRight, Vector3.one * 0.2f);
                Gizmos.DrawLine(transform.position + Vector3.right * limitLocalLeft, transform.position + Vector3.right * limitLocalRight);
            }
        }
    }

    private void Update()
    {
        transform.forward = new Vector3(horizontalInput, 0, 0);
        if (isDetectPlayer || GameManager.Instance.gameState != GameManager.GameState.Playing)
            velocity.x = 0;
        else
            velocity.x = moveSpeed * horizontalInput;

        //velocity.x = moveSpeed * horizontalInput;      //calucating the x speed

        CheckGround();

        if (isGrounded && velocity.y < 0)
            velocity.y = 0;
        else
            velocity.y += gravity * Time.deltaTime;     //add gravity

        if (isDead)
            velocity.x = 0;

        Vector2 finalVelocity = velocity;
        if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
            GetSlopeVelocity(ref finalVelocity);

        characterController.Move(finalVelocity * Time.deltaTime);
        HandleAnimation();

        if (!isDead)
        {
            if (isWallAHead())
                Flip();
            else if (usePatrol)
            {
                if ((velocity.x < 0 && transform.position.x < limitLeft)
                    || (velocity.x > 0 && transform.position.x > limitRight))
                    Flip();
            }

            if (allowCheckGroundAhead && !isGroundedAhead())
                Flip();
        }
    }

    RaycastHit groundHit;
    void CheckGround()
    {
        isGrounded = false;
        if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.9f, Vector3.down, out groundHit, 1f, layerAsGround))
        {
            float distance = transform.position.y - groundHit.point.y;
            if (distance <= (characterController.skinWidth + 0.01f))
                isGrounded = true;
        }
    }

    bool isGroundedAhead()
    {
        var _isGroundAHead = Physics.Raycast(transform.position + Vector3.up * 0.5f + (velocity.x > 0 ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down, 1);
        Debug.DrawRay(transform.position + Vector3.up * 0.5f + (velocity.x > 0 ? Vector3.right : Vector3.left) * characterController.radius * 1.1f, Vector3.down * 1);
        return _isGroundAHead;
    }

    void GetSlopeVelocity(ref Vector2 vel)
    {
        var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
        vel = vel.x * crossSlope;

        Debug.DrawRay(transform.position, crossSlope * 10);
    }

    void Flip()
    {
        horizontalInput *= -1;
    }

    bool isWallAHead()
    {
        if (Physics.CapsuleCast(transform.position + Vector3.up * characterController.height * 0.5f, transform.position + Vector3.up * (characterController.height - characterController.radius),
            characterController.radius, horizontalInput > 0 ? Vector3.right : Vector3.left, 0.1f, layerAsWall))
        {
            return true;
        }
        else
            return false;
    }

    void HandleAnimation()
    {
        anim.SetFloat("speed", Mathf.Abs(velocity.x));
        anim.SetBool("isDead", isDead);
    }

    public void Kill()
    {
        StopAllCoroutines();
        isDead = true;
        SoundManager.PlaySfx(soundDie);
        characterController.enabled = false;
        GetComponent<BoxCollider>().enabled = false;
        gameObject.layer = LayerMask.NameToLayer("TriggerPlayer");
        //if (gravity == 0)
        //    gravity = -25;
        gameObject.AddComponent<Rigidbody>();


        Destroy(gameObject, 2);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Deadzone")
        {
            Kill();
        }
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        Kill();
    }
}
