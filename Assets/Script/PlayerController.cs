using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public enum PlayerState { Ground, Water, Jetpack, Windy }

[RequireComponent(typeof(WaterChecker))]

[System.Serializable]
public class PlayerParameter
{
    public float moveSpeed = 5;
    public float runSpeed = 6;
    public float maxJumpHeight = 3;
    public float minJumpHeight = 1;
    public float gravity = -35f; 
}

public class PlayerController : MonoBehaviour, ICanTakeDamage
{
    public int playerID = 1;
    [ReadOnly] public float gravity = -35f;
    [ReadOnly] public PlayerState PlayerState = PlayerState.Ground;        //set what state the player in
    public GameObject trailFX;
    [Header("---SLIDING---")]
    public float slidingTime = 1;
    public float slidingCapsultHeight = 0.8f;
    float originalCharHeight, originalCharCenterY;

    [Header("---SETUP LAYERMASK---")]
    public LayerMask layerAsGround;
    public LayerMask layerAsWall;
    public LayerMask layerCheckHitHead;

    [Header("---WALL SLIDE---")]
    public float wallSlidingSpeed = 0.5f;
    [Tooltip("Player only can stick on the wall a little bit, then fall")]
    public float wallStickTime = 0.25f;
    [ReadOnly] public float wallStickTimeCounter;
    public Vector2 wallSlidingJumpForce = new Vector2(6, 3);
    [ReadOnly] public bool isWallSliding = false;

    [Header("WATER")]
    public GameObject breathBubbleFX;
    public GameObject bubbleTrailFX;
    public float breathBubbleMin = 1;
    public float breathBubbleMax = 3;
    public AudioClip[] waterSwimSounds;
    public float waterSwimSoundVolume = 0.35f;

    public CharacterController characterController { get; set; }
    [ReadOnly] public Vector2 velocity;
    [ReadOnly] public Vector2 input;
    [ReadOnly] public Vector2 inputLastTime = Vector2.right;
    [ReadOnly] public bool isGrounded = false;
    Animator anim;
    bool isPlaying = true;
    [ReadOnly] public bool isSliding = false;
    [ReadOnly] public bool isDead = false;

    float velocityXSmoothing;
    public float accelerationTimeAirborne = .2f;
    public float accelerationTimeGroundedRun = .3f;
    public float accelerationTimeGroundedSliding = 1f;

    [Header("---AUDIO---")]
    public AudioClip soundFootStep;
    [Range(0f, 1f)]
    public float soundFootStepVolume = 0.5f;
    public AudioClip soundJump, soundHit, soundDie, soundSlide;

    public bool isInJumpZone { get; set; }

    public float accGrounedOverride { get; set; }

    [Header("Setup parameter on ground")]
    public PlayerParameter GroundParameter;     //Ground parameters
    [Header("Setup parameter in water zone")]
    public PlayerParameter WaterZoneParameter;      //Water parameters

    private float moveSpeed;        //the moving speed, changed evey time the player on ground or in water
    private float runSpeed = 6;
    private float maxJumpHeight;
    private float minJumpHeight;

    public bool isFacingRight { get { return inputLastTime.x > 0; } }
    [ReadOnly] public bool isRunning = false;

    WaterChecker waterChecker;

    protected PlayerParameter overrideZoneParameter = null; //null mean no override
    protected bool useOverrideParameter = false;
    PlayerOverrideParametersChecker playerOverrideParametersChecker;
    public void SetOverrideParameter(PlayerParameter _override, bool isUsed, PlayerState _zone = PlayerState.Ground)
    {
        overrideZoneParameter = _override;
        useOverrideParameter = isUsed;
        PlayerState = _zone;
    }

    bool isUsingPatachute = false;

    public void SetParachute(bool useParachute)
    {
        isUsingPatachute = useParachute;
    }

    public void ExitZoneEvent()
    {
        PlayerState = PlayerState.Ground;
        if (isUsingPatachute)
            SetParachute(false);
    }

    void Start()
    {
        transform.position = new Vector3(transform.position.x, transform.position.y, 0);
        characterController = GetComponent<CharacterController>();
        anim = GetComponent<Animator>();
        transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);
        originalCharHeight = characterController.height;
        originalCharCenterY = characterController.center.y;
        jetpackObj.SetActive(false);

        jetpackAScr = jetpackObj.AddComponent<AudioSource>();
        jetpackAScr.clip = jetpackSound;
        jetpackAScr.volume = 0;
        jetpackAScr.loop = true;

        rangeAttack = GetComponent<RangeAttack>();
        meleeAttack = GetComponent<MeleeAttack>();
        waterChecker = GetComponent<WaterChecker>();

        ropeRenderer = GetComponent<LineRenderer>();
        playerOverrideParametersChecker = GetComponent<PlayerOverrideParametersChecker>();
        SetupParameter();

        StartCoroutine(BreathBubbleCo());
    }

    IEnumerator BreathBubbleCo()
    {
        while (true)
        {
            while (PlayerState != PlayerState.Water)
                yield return null;

            yield return new WaitForSeconds(Random.Range(breathBubbleMin, breathBubbleMax));

            if (PlayerState == PlayerState.Water)
                Instantiate(breathBubbleFX, transform.position + Vector3.up * characterController.height, Quaternion.identity);
        }
    }

    public void SetupParameter()
    {
        PlayerParameter _tempParameter;

        switch (PlayerState)
        {
            case PlayerState.Ground:
                _tempParameter = GroundParameter;
                //animSetSpeed(1);
                break;
            case PlayerState.Water:
                _tempParameter = WaterZoneParameter;
                //animSetSpeed(.75f);
                velocity.y = Mathf.Max(velocity.y, -3);
                break;
            default:
                _tempParameter = GroundParameter;
                break;
        }

        if (useOverrideParameter)
            _tempParameter = overrideZoneParameter;

        isRunning = false;
        moveSpeed = _tempParameter.moveSpeed;
        runSpeed = _tempParameter.runSpeed;
        maxJumpHeight = _tempParameter.maxJumpHeight;
        minJumpHeight = _tempParameter.minJumpHeight;
        gravity = _tempParameter.gravity;
    }

    public void animSetSpeed(float value)
    {
        if (anim)
            anim.speed = value;
    }

    void SetCheckPoint(Vector3 pos)
    {
        RaycastHit hit;
        if (Physics.Raycast(pos + Vector3.up, Vector3.down, out hit, 100, layerAsGround))
        {
            GameManager.Instance.SetCheckPoint(hit.point);
        }
    }

    void Update()
    {
        if (isMovingInPipe)
            return;

        if (isGrabingRope)
        {
            transform.RotateAround(currentAvailableRope.transform.position, rotateAxis, (isFacingRight ? 1 : -1) * speed * Time.deltaTime);


            transform.up = currentAvailableRope.transform.position - transform.position;
            transform.Rotate(0, isFacingRight ? 90 : -90, 0);

            ropeRenderer.SetPosition(0, transform.position + transform.forward * grabOffset.x + transform.up * grabOffset.y);
            ropeRenderer.SetPosition(1, currentAvailableRope.transform.position);

            if (transform.position.y >= releasePointY)
            {
                if ((isFacingRight && transform.position.x > currentAvailableRope.transform.position.x) || (!isFacingRight && transform.position.x < currentAvailableRope.transform.position.x))
                    //GrabRelease();      //disconnect grab if player reach to the limit position
                    Flip();
            }
        }
        else if (climbingState != ClimbingState.ClimbingLedge)      //stop move when climbing
        {
            transform.forward = new Vector3(isFacingRight ? 1 : -1, 0, 0);
            //float targetVelocityX = moveSpeed * input;
            float targetVelocityX = input.x * (isHangingTopPipe ? hangingMoveSpeed : (isRunning ? runSpeed : moveSpeed));

            if (isJumpingOutFromRope)
                targetVelocityX = (isFacingRight ? 1 : -1) * (isRunning ? runSpeed : moveSpeed);

            if (isSliding || isWallSliding)
                targetVelocityX = 0;


            if (forceStandingRemain > 0)
            {
                targetVelocityX = 0;
                forceStandingRemain -= Time.deltaTime;
            }

            velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, isGrounded ?
                (isSliding == false ? accelerationTimeGroundedRun : accelerationTimeGroundedSliding) : (isHangingTopPipe ? 0 : accelerationTimeAirborne));

            CheckGround();

            if (isGrounded && groundHit.collider.gameObject.tag == "Deadzone")
                HitAndDie();

            if (!isDead && isUsingJetpack)
            {
                jetpackRemainTime -= Time.deltaTime;
                jetpackRemainTime = Mathf.Max(0, jetpackRemainTime);
                if (jetpackRemainTime > 0)
                    velocity.y += jetForce * Time.deltaTime;
                else {
                    ActiveJetpack(false);
                }
            }

            if (isGrounded && velocity.y < 0)
            {
                velocity.y = 0;
                lastJumpZoneObj = null;
                CheckEnemy();
                CheckSpring();
                lastRopePointObj = null;
                isJumpingOutFromRope = false;
                isFallingFromWall = false;
                allowGrabWall = true;
            }
            else if (isWallSliding)
                velocity.y = -wallSlidingSpeed;
            else if (isHangingTopPipe)
                velocity.y = 0;
            else
                velocity.y += gravity * Time.deltaTime;     //add gravity

            if (!isPlaying || isDead || isFallingFromWall)
                velocity.x = 0;


            if (!isUsingJetpack)
                CheckWallSliding();


            CheckEnemyAHead();

            if (PlayerState == PlayerState.Windy)
            {

                if (isUsingPatachute)
                {
                    if (velocity.y < playerOverrideParametersChecker.currentZone.forceVeritcalWithParachute)
                    {
                        velocity.y = playerOverrideParametersChecker.currentZone.forceVeritcalWithParachute;
                    }
                }
                else
                {
                    if (velocity.y < playerOverrideParametersChecker.currentZone.forceVertical)
                    {
                        velocity.y = playerOverrideParametersChecker.currentZone.forceVertical;
                    }
                }
            }

            Vector2 finalVelocity = velocity + new Vector2(extraForceSpeed, 0);
            if (isGrounded && groundHit.normal != Vector3.up)        //calulating new speed on slope
                GetSlopeVelocity(ref finalVelocity);

            CheckLimitPos(ref finalVelocity);
            characterController.Move(finalVelocity * Time.deltaTime);

            if (isJetpackActived)
                UpdateJetPackStatus();

        }

        HandleInput();
        HandleAnimation();
        CheckDoubleTap();
        bubbleTrailFX.SetActive(PlayerState == PlayerState.Water);
        trailFX.SetActive(PlayerState != PlayerState.Water && isRunning);

        if (isGrounded)
        {
            isInJumpZone = false;
            wallStickTimeCounter = wallStickTime;       //set reset wall stick timer when on ground
            CheckStandOnEvent();
        }

        ropeRenderer.enabled = isGrabingRope;
        CheckRopeInZone();      //only checking rope when in jump status

        if (!isUsingJetpack && PlayerState == PlayerState.Ground && forceStandingRemain <= 0)
        {
            if (climbingState == ClimbingState.None && isGrounded)
                CheckLowerLedge();

            if (climbingState == ClimbingState.None && !isGrounded && velocity.y < 0)
                CheckLedge();
        }

        if (isHangingTopPipe)
            CheckIfInTopPipe();
        else
            CheckGrabHangingTopPipe();

        if (climbingState == ClimbingState.ClimbingLedge)
            UpdatePositionOnMovableObject(ledgeTarget);
        else if (isGrounded || input.x == 0)
            UpdatePositionOnMovableObject(groundHit.transform);

        CheckWaterArea();       //
    }

    void CheckWaterArea()
    {
        if (PlayerState != PlayerState.Water)
        {
            if (waterChecker.isInWater)
            {
                PlayerState = PlayerState.Water;
                SetupParameter();
                SoundManager.PlaySfx(waterSwimSounds[Random.Range(0, waterSwimSounds.Length)], waterSwimSoundVolume);
            }
        }
        else
        {
            if (!waterChecker.isInWater)
            {
                PlayerState = PlayerState.Ground;
                SetupParameter();
            }
        }
    }

    float extraForceSpeed = 0;
    public void AddHorizontalForce(float _speed)
    {
        extraForceSpeed = _speed;
    }

    [ReadOnly] public Vector3 m_LastGroundPos = Vector3.zero;
    private float m_LastAngle = 0;
    [ReadOnly] public Transform m_CurrentTarget;
    public Vector3 DeltaPos { get; private set; }
    public float DeltaYAngle { get; private set; }
    public void UpdatePositionOnMovableObject(Transform target)
    {
        if (target == null)
        {
            m_CurrentTarget = null;
            return;
        }

        if (m_CurrentTarget != target)
        {
            m_CurrentTarget = target;
            DeltaPos = Vector3.zero;
            DeltaYAngle = 0;
        }
        else
        {
            DeltaPos = target.transform.position - m_LastGroundPos;
            DeltaYAngle = target.transform.rotation.eulerAngles.y - m_LastAngle;

            Vector3 direction = transform.position - target.transform.position;
            direction.y = 0;

            float FinalAngle = Vector3.SignedAngle(Vector3.forward, direction.normalized, Vector3.up) + DeltaYAngle;

            float xMult = Vector3.Dot(Vector3.forward, direction.normalized) > 0 ? 1 : -1;
            float zMult = Vector3.Dot(Vector3.right, direction.normalized) > 0 ? -1 : 1;

            float cosine = Mathf.Abs(Mathf.Cos(FinalAngle * Mathf.Deg2Rad));
            Vector3 deltaRotPos = new Vector3(cosine * xMult, 0,
                 Mathf.Abs(Mathf.Sin(FinalAngle * Mathf.Deg2Rad)) * zMult) * Mathf.Abs(direction.magnitude);

            DeltaPos += deltaRotPos * (DeltaYAngle * Mathf.Deg2Rad);
        }

        if (DeltaPos.magnitude > 3f)
            DeltaPos = Vector3.zero;

        characterController.enabled = false;
        transform.position += DeltaPos;
        transform.Rotate(0, DeltaYAngle, 0);
        characterController.enabled = true;

        m_LastGroundPos = target.transform.position;
        m_LastAngle = target.transform.rotation.eulerAngles.y;
    }

    void CheckDoubleTap()
    {
        if (leftButtonCoolerTime > 0)
        {

            leftButtonCoolerTime -= 1 * Time.deltaTime;

        }
        else
        {
            leftButtonCount = 0;
        }

        if (rightButtonCoolerTime > 0)
        {

            rightButtonCoolerTime -= 1 * Time.deltaTime;

        }
        else
        {
            rightButtonCount = 0;
        }
    }

    void CheckLimitPos(ref Vector2 vel)
    {
        if (GameManager.Instance.gameState == GameManager.GameState.Playing)
        {
            if (transform.position.y < CameraFollow.Instance.limitCameraBelow)     //if player fall below the camera, player die
                Die();
        }

        if (vel.y > 0 && (transform.position.y + characterController.height)
            >= Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, Camera.main.transform.position.z * -1)).y)
        {
            vel.y = 0;     //if player go higher than the camera, stop player
            velocity.y = 0;
        }
    }

    private void LateUpdate()
    {
        if (isMovingInPipe)
            return;
        if (isGrabingRope)
            return;

        var finalPos = new Vector3(transform.position.x, transform.position.y, 0);
        transform.position = finalPos;    //keep the player stay 0 on Z axis
        if (!isHangingTopPipe && input.x != 0)
            inputLastTime = input;

    }

    public void AddPosition(Vector2 pos)
    {
        characterController.enabled = false;
        transform.position += (Vector3)pos;
        characterController.enabled = true;
    }

    public void SetPosition(Vector2 pos)
    {
        characterController.enabled = false;
        transform.position = (Vector3)pos;
        characterController.enabled = true;
    }

    public void TeleportTo(Vector3 pos)
    {
        if (!isPlaying)
            return;

        StartCoroutine(TeleportCo(pos));
    }

    IEnumerator TeleportCo(Vector3 pos)
    {
        isPlaying = false;

        ControllerInput.Instance.ShowController(false);
        yield return new WaitForSeconds(0.5f);
        characterController.enabled = false;
        transform.position = pos;
        characterController.enabled = true;
        isPlaying = true;
        ControllerInput.Instance.ShowController(true);
    }

    private void CheckStandOnEvent()
    {
        var hasEvent = (IPlayerStandOn)groundHit.collider.gameObject.GetComponent(typeof(IPlayerStandOn));
        if (hasEvent != null)
            hasEvent.OnPlayerStandOn();
    }

    void CheckEnemy()
    {
        var isEnemy = groundHit.collider.GetComponent<SimpleEnemy>();
        if (isEnemy)
        {
            if (isEnemy.canBeKillWhenPlayerJumpOn)
            {
                isEnemy.Kill();
                Jump(0.5f);
            }
            else
                HitAndDie();
        }
    }

    void CheckEnemyAHead()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * characterController.height * 0.5f,
            characterController.radius,
            isFacingRight ? Vector3.right : Vector3.left,
            out hit, 0.1f, 1 << LayerMask.NameToLayer("Enemy")))
        {
            HitAndDie();
        }
    }

    void CheckSpring()
    {
        var isSpring = groundHit.collider.GetComponent<TheSpring>();
        if (isSpring)
        {
            isSpring.Action();
            Jump(isSpring.pushHeight);
        }
    }

    void Flip()
    {
        if (isSliding || isJumpingOutFromRope || isHangingTopPipe)
            return;
        //input *= -1;
        inputLastTime *= -1;
    }

    void GetSlopeVelocity(ref Vector2 vel)
    {
        var crossSlope = Vector3.Cross(groundHit.normal, Vector3.forward);
        vel = vel.x * crossSlope;

        Debug.DrawRay(transform.position, crossSlope * 10);
    }

    bool allowGrabWall = true;

    void CheckWallSliding()
    {
        isWallSliding = false;

        if (allowGrabWall)
        {
            if (PlayerState != PlayerState.Ground || isUsingJetpack || input.x != (isFacingRight ? 1 : -1) || isGrounded || velocity.y >= 0)
                return;

            if (isWallAHead())
            {
                velocity.x = 0;
                if (isFallingFromWall)
                    return;     //stop checking if player in falling state

                if (!isGrounded && !isDead)
                {
                    isWallSliding = true;
                    wallStickTimeCounter -= Time.deltaTime;
                    if (wallStickTimeCounter < 0)
                    {
                        isWallSliding = false;
                        isFallingFromWall = true;
                    }
                }
            }
        }
    }

    bool isFallingFromWall = false;

    bool isWallAHead()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position + Vector3.up * characterController.height * 0.5f,
            characterController.radius,
            isFacingRight ? Vector3.right : Vector3.left,
            out hit, 0.1f, layerAsWall))
        {
            return true;
        }
        else
            return false;
    }

    public void Victory()
    {
        isPlaying = false;
        isRunning = false;
        GameManager.Instance.FinishGame();
        anim.SetBool("victory", true);
    }

    void HitAndDie()
    {
        if (isDead)
            return;

        SoundManager.PlaySfx(soundHit);
        Die();
    }

    public void Die(bool disappear = false)
    {
        if (isDead)
            return;
        isRunning = false;
        SoundManager.PlaySfx(soundDie);
        isDead = true;
        velocity.x = 0;
        forceStandingRemain = 0;
        anim.applyRootMotion = true;
        if (isJetpackActived)
            ActiveJetpack(false);
        if (isHangingTopPipe)
            DropHangingTopPipe();

        GameManager.Instance.GameOver();

        if (disappear)
            gameObject.SetActive(false);
    }

    void HandleAnimation()
    {
        if (PlayerState == PlayerState.Ground)
            anim.speed = isRunning ? 1.8f : 1;
        else if (PlayerState == PlayerState.Water)
            anim.speed = 0.75f;

        anim.SetInteger("inputX", (int)input.x);
        anim.SetFloat("speed", Mathf.Abs(velocity.x));
        anim.SetBool("isGrounded", isGrounded);
        anim.SetFloat("height speed", velocity.y);
        anim.SetBool("isSliding", isSliding);
        anim.SetBool("isWallSliding", isWallSliding);
        anim.SetBool("isDead", isDead);
        anim.SetBool("isFallingFromWall", isFallingFromWall);
        anim.SetBool("isUsingJetpack", isUsingJetpack);
        anim.SetBool("isGrabingRope", isGrabingRope);
        anim.SetBool("isInWindZone", PlayerState == PlayerState.Windy);
        anim.SetBool("isHangingTopPipe", isHangingTopPipe);
        anim.SetBool("isInWaterZone", PlayerState == PlayerState.Water);
    }

    void HandleInput()
    {
        //if (currentAvailableRope != null)
        //{
        //    if (Input.GetMouseButtonDown(0))
        //        GrabRope();

        //    if (Input.GetMouseButtonUp(0))
        //        GrabRelease();
        //}
        //else
        //{
        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
            ControllerInput.Instance.MoveLeftTap();
        else if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
            ControllerInput.Instance.MoveRightTap();
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
            ControllerInput.Instance.MoveLeft();
        else if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
            ControllerInput.Instance.MoveRight();

        else if (Input.GetKeyUp(KeyCode.A) || Input.GetKeyUp(KeyCode.LeftArrow))
            ControllerInput.Instance.StopMove(-1);
        else if (Input.GetKeyUp(KeyCode.D) || Input.GetKeyUp(KeyCode.RightArrow))
            ControllerInput.Instance.StopMove(1);

        if (Input.GetKeyDown(KeyCode.Space))
            ControllerInput.Instance.Jump();
        else if (Input.GetKeyUp(KeyCode.Space))
            ControllerInput.Instance.JumpOff();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            ControllerInput.Instance.UseJetpack(true);
        else if (Input.GetKeyUp(KeyCode.LeftShift))
            ControllerInput.Instance.UseJetpack(false);

        if (Input.GetKeyDown(KeyCode.S))
        {
            ControllerInput.Instance.MoveDown();
        }

        if (isRunning && Input.GetKeyDown(KeyCode.LeftShift))
        {
            ControllerInput.Instance.SlideOn();
        }
        //}
    }

    #region ATTACK
    protected RangeAttack rangeAttack;

    public void RangeAttack()
    {
        if (isHangingTopPipe || isGrabingRope || isSliding || climbingState == ClimbingState.ClimbingLedge)
            return;

        if (!isPlaying)
            return;

        if (rangeAttack != null)
        {
            if (rangeAttack.Fire(isFacingRight ? Vector2.right : Vector2.left))
            {
                anim.SetTrigger("throw");
            }
        }
    }

    protected MeleeAttack meleeAttack;
    public void MeleeAttack(){
		if (!isPlaying)
			return;

        if (isHangingTopPipe || isGrabingRope || isSliding || climbingState == ClimbingState.ClimbingLedge || isUsingJetpack || PlayerState == PlayerState.Windy)
            return;

        if (meleeAttack!=null) {
			if (meleeAttack.Attack ()) {
				anim.SetTrigger ("meleeAttack");
                forceStandingRemain = meleeAttack.standingTime;
            }
		}
	}

    float forceStandingRemain = 0;

    #endregion

    public void SlideOn()
    {
        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        if (GameManager.Instance.gameState != GameManager.GameState.Playing)
            return;

        if (!isGrounded)
            return;

        if (isSliding)
            return;

        if (isUsingJetpack)
            return;

        SoundManager.PlaySfx(soundSlide);
        isSliding = true;
        characterController.height = slidingCapsultHeight;
        var _center = characterController.center;
        _center.y = slidingCapsultHeight * 0.5f;
        characterController.center = _center;

        Invoke("SlideOff", slidingTime);
    }

    void SlideOff()
    {
        if (!isSliding)
            return;

        if (isUsingJetpack)
            return;

        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        characterController.height = originalCharHeight;
        var _center = characterController.center;
        _center.y = originalCharCenterY;
        characterController.center = _center;

        isSliding = false;
    }

    [HideInInspector] public RaycastHit groundHit;
    void CheckGround()
    {
        isGrounded = false;
        if (velocity.y > 0.1f)
            return;

        if (Physics.SphereCast(transform.position + Vector3.up * 1, characterController.radius * 0.99f, Vector3.down, out groundHit, 2, layerAsGround))
        {
            float distance = transform.position.y - groundHit.point.y;
           
            if (distance <= (characterController.skinWidth + 0.01f + (DeltaPos.y!=0 ? 0.1:0)))      //if standing on the moving platform (deltaPos.y != 0), increase the delect ground to avoid problem
            {
                isGrounded = true;

                //check if standing on small ledge then force play move
                if (!Physics.Raycast(transform.position, Vector3.down, 1, layerAsGround))
                {
                    if (input.x == 0 && groundHit.point.y > (transform.position.y - 0.1f))
                    {
                        var forceMoveOnLedge = Vector3.zero;
                        if (groundHit.point.x < transform.position.x)
                            forceMoveOnLedge = (transform.position - groundHit.point) * Time.deltaTime * 10 * (isFacingRight ? 1 : -1);
                        else
                            forceMoveOnLedge = (transform.position - groundHit.point) * Time.deltaTime * 10 * (isFacingRight ? -1 : 1);
                        characterController.Move(forceMoveOnLedge);
                    }
                }
            }
        }
    }

    // this script pushes all rigidbodies that the character touches
    [Header("---PUSH OBJECT---")]
    public float pushPower = 2.0f;

    [ReadOnly] public Vector3 moveDirection;

    void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (isDead)
            return;

        moveDirection = hit.moveDirection;
        var isTrigger = hit.gameObject.GetComponent<TriggerEvent>();
        if (isTrigger)
        {
            isTrigger.OnContactPlayer();
        }

        if (velocity.y > 1 && hit.moveDirection.y == 1)     //check hit object from below
        {
            velocity.y = 0;
            CheckBlockBrick(hit.gameObject);  //check if hit block with head
           
        }
        Rigidbody body = hit.collider.attachedRigidbody;

        // no rigidbody
        if (body == null || body.isKinematic)
        {
            return;
        }

        // We dont want to push objects below us
        if (hit.moveDirection.y < -0.3)
        {
            return;
        }

        // Calculate push direction from move direction,
        // we only push objects to the sides never up and down
        Vector3 pushDir = new Vector3(hit.moveDirection.x, 0, hit.moveDirection.z);

        // If you know how fast your character is trying to move,
        // then you can also multiply the push velocity by that.

        // Apply the push
        body.velocity = pushDir * pushPower;
    }

    void CheckBlockBrick(GameObject obj)
    {
        Block isBlock;
        isBlock = obj.GetComponent<Block>();
        if (isBlock)
        {
            isBlock.BoxHit();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        if (characterController == null)
            characterController = GetComponent<CharacterController>();

        Gizmos.DrawWireSphere(transform.position + Vector3.up * characterController.height * 0.5f, ropeCheckRadius);
    }

    private void OnDrawGizmosSelected()
    {
        if (isGrounded)
        {
            Gizmos.DrawWireSphere(groundHit.point, characterController.radius * 0.9f);
        }
    }

    public void Jump(float newForce = -1)
    {
        if (currentAvailableRope != null)
        {
            GrabRope();
            return;
        }

        if (isUsingJetpack)
            return;

        if (climbingState == ClimbingState.ClimbingLedge)      //stop move when climbing
            return;

        if (GameManager.Instance.gameState != GameManager.GameState.Playing)
            return;

        UpdatePositionOnMovableObject(null);

        wallStickTimeCounter = wallStickTime;

        if (isWallSliding)
        {
            if (newForce == -1)
                SoundManager.PlaySfx(soundJump);

            isWallSliding = false;
            allowGrabWall = false;
            velocity.y += Mathf.Sqrt(wallSlidingJumpForce.y * -2 * gravity);
        }
        else if (isGrounded && (PlayerState == PlayerState.Ground || PlayerState == PlayerState.Windy))
        {
            if (newForce == -1)
                SoundManager.PlaySfx(soundJump);

            if (isSliding)
                SlideOff();

            isGrounded = false;
            var _height = newForce != -1 ? newForce : maxJumpHeight;
            velocity.y += Mathf.Sqrt(_height * -2 * gravity);
            velocity.x = characterController.velocity.x;

            characterController.Move(velocity * Time.deltaTime);

        }
        else if (PlayerState == PlayerState.Water)
        {
            var _height = newForce != -1 ? newForce : minJumpHeight;
            velocity.y = Mathf.Sqrt(_height * -2 * gravity);
            SoundManager.PlaySfx(waterSwimSounds[Random.Range(0, waterSwimSounds.Length)], waterSwimSoundVolume);
        }
        else if (isInJumpZone)
        {
            if (newForce == -1)
                SoundManager.PlaySfx(soundJump);

            var _height = maxJumpHeight;
            velocity.y = Mathf.Sqrt(_height * -2 * gravity);
            //velocity.x = characterController.velocity.x;

            characterController.Move(velocity * Time.deltaTime);
            isInJumpZone = false;
            Time.timeScale = 1;
        }
    }

    public void JumpOff()
    {
        if (currentAvailableRope != null)
        {
            GrabRelease();
            return;
        }

        if (!isPlaying)
            return;

        var _minJump = Mathf.Sqrt(minJumpHeight * -2 * gravity);
        if (velocity.y > _minJump)
        {
            velocity.y = _minJump;
        }
    }

    JumpZoneObj lastJumpZoneObj;
    private void OnTriggerEnter(Collider other)
    {
        if (!isPlaying)
            return;

        if (isDead)
            return;

        var isTrigger = other.GetComponent<TriggerEvent>();
        if (isTrigger)
        {
            isTrigger.OnContactPlayer();
        }

        if (other.gameObject.tag == "Finish")
            Victory();
        else if (other.gameObject.tag == "Deadzone")
            Die();

        if (other.gameObject.tag == "Checkpoint")
            SetCheckPoint(other.transform.position);

        if (other.gameObject.tag == "TurnAround")
            Flip();

        if (other.gameObject.tag == "StopJetpack")
            ActiveJetpack(false);

        var isJumpZone = other.GetComponent<JumpZoneObj>();
        if (lastJumpZoneObj != isJumpZone && isJumpZone != null)
        {
            isInJumpZone = true;
            isJumpZone.SetState(true);

            if (isJumpZone.slowMotion)
            {
                Time.timeScale = 0.1f;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        //Check Jump Zone
        var isJumpZone = other.GetComponent<JumpZoneObj>();
        if (isJumpZone != null)
        {
            isInJumpZone = false;
            isJumpZone.SetState(false);
            if (velocity.y > 0)
                isJumpZone.SetStateJump();
            lastJumpZoneObj = isJumpZone;
            Time.timeScale = 1f;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Deadzone")
        {
            HitAndDie();
        }
    }

    //Call by walk animation
    public void FootStep()
    {
        SoundManager.PlaySfx(soundFootStep, soundFootStepVolume);
    }

    #region MOVE
    //DOUBLE TAP SYSTEM
    float leftButtonCoolerTime = 0.5f;
    float rightButtonCoolerTime = 0.5f;
    int leftButtonCount, rightButtonCount;

    bool IgnoreControllerInput()
    {
        return isGrabingRope || isJumpingOutFromRope || climbingState == ClimbingState.ClimbingLedge;
    }

    public void MoveLeft()
    {
       if(isGrabingRope || isJumpingOutFromRope)
            return;

        if (isPlaying)
        {
            input = new Vector2(-1, 0);
        }
    }
    public void MoveLeftTap()
    {
        if (IgnoreControllerInput())
            return;
        if (!isGrounded)
            return;

        if (leftButtonCoolerTime > 0 && leftButtonCount == 1/*Number of Taps you want Minus One*/)
        {
            isRunning = true;
        }
        else
        {
            leftButtonCoolerTime = 0.5f;
            leftButtonCount += 1;
        }

    }

    //This action is called by the Input/ControllerInput
    public void MoveRight()
    {
        if (isGrabingRope || isJumpingOutFromRope)
            return;

        if (isPlaying)
        {
            input = new Vector2(1, 0);
        }
    }
    public void MoveRightTap()
    {
        if (IgnoreControllerInput())
            return;
        if (!isGrounded)
            return;

        if (rightButtonCoolerTime > 0 && rightButtonCount == 1/*Number of Taps you want Minus One*/)
        {
            isRunning = true;
        }
        else
        {
            rightButtonCoolerTime = 0.5f;
            rightButtonCount += 1;
        }

    }

    //This action is called by the Input/ControllerInput
    public void MoveUp()
    {
        if (IgnoreControllerInput())
            return;
        input = new Vector2(0, 1);
    }


    //This action is called by the Input/ControllerInput
    public void MoveDown()
    {
        if (IgnoreControllerInput())
            return;

        if (isHangingTopPipe)
            DropHangingTopPipe();
        input = new Vector2(0, -1);
    }

    public void StopMove(int fromDirection = 0)
    {
        if (input.x != 0 && input.x != fromDirection)
            return;

        input = Vector2.zero;
        isRunning = false;
    }
    #endregion

    #region LEDGE
    public enum ClimbingState { None, ClimbingLedge }
    public LayerMask layersCanGrab;
    [Header("------CLIBMING LEDGE------")]
    [ReadOnly] public ClimbingState climbingState;
    [Tooltip("Ofsset from ledge to set character position")]
    public Vector3 climbOffsetPos = new Vector3(0, 1.3f, 0);
    [Tooltip("Adjust to fit with the climbing animation length")]
    public float climbingLedgeTime = 1;
    public Transform verticalChecker;
    public float verticalCheckDistance = 0.5f;

    [Header("---CHECK LOW CLIMB 1m---")]
    [Tooltip("Ofsset from ledge to set character position")]
    public Vector3 climbLCOffsetPos = new Vector3(0, 1f, 0);
    public float climbingLBObjTime = 1;

   
    Transform ledgeTarget;      //use to update ledge moving/rotating
    Vector3 ledgePoint;

    bool CheckLowerLedge()      //check lower ledge
    {
        RaycastHit hitVertical;
        RaycastHit hitGround;
        RaycastHit hitHorizontal;

        if (Physics.Linecast(verticalChecker.position, new Vector3(verticalChecker.position.x,transform.position.y + characterController.stepOffset,transform.position.z), out hitVertical, layersCanGrab, QueryTriggerInteraction.Ignore))
        {
            if(hitVertical.normal == Vector3.up) {
                //Debug.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), (input > 0 ? Vector3.right : Vector3.left) * 2);
                if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y, verticalChecker.position.z), Vector3.down, out hitGround, 3, layersCanGrab, QueryTriggerInteraction.Ignore))
                {
                    if ((int)hitGround.distance <= 1)
                    {
                        if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), isFacingRight ? Vector3.right : Vector3.left, out hitHorizontal, 2, layersCanGrab, QueryTriggerInteraction.Ignore))
                        {
                            ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                            //check if the top of ledge is clear or not
                            var hits = Physics.OverlapSphere(ledgePoint + (isFacingRight ? Vector3.right : Vector3.left) * 0.5f + Vector3.up * 0.5f,
                                0.1f, layersCanGrab);
                            if (hits.Length == 0)
                            {
                                ledgeTarget = hitVertical.transform;
                                velocity = Vector2.zero;
                                characterController.Move(velocity);
                                transform.position = CalculatePositionOnLedge(climbOffsetPos);
                                //reset other value
                                isWallSliding = false;
                                if (isSliding)
                                    SlideOff();

                                StartCoroutine(ClimbingLedgeCo(true));
                                return true;
                            }
                        }
                    }
                }
            }
        }
        return false;
    }

    bool CheckLedge()       //check higher ledge
    {
        RaycastHit hitVertical;
        RaycastHit hitHorizontal;

        Debug.DrawRay(verticalChecker.position, Vector3.down * verticalCheckDistance);
        if (Physics.Raycast(verticalChecker.position, Vector2.down, out hitVertical, verticalCheckDistance, layersCanGrab, QueryTriggerInteraction.Ignore))
        {
            Debug.DrawRay(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), (isFacingRight ? Vector3.right : Vector3.left) * 2);
            if (Physics.Raycast(new Vector3(transform.position.x, hitVertical.point.y - 0.1f, verticalChecker.position.z), isFacingRight ? Vector3.right : Vector3.left, out hitHorizontal, 2, layersCanGrab, QueryTriggerInteraction.Ignore))
            {
                ledgePoint = new Vector3(hitHorizontal.point.x, hitVertical.point.y, transform.position.z);
                //check if the top of ledge is clear or not
                var hits = Physics.OverlapSphere(ledgePoint + (isFacingRight ? Vector3.right : Vector3.left) * 0.5f + Vector3.up * 0.5f,
                    0.1f, layersCanGrab);
                if (hits.Length == 0)
                {
                    ledgeTarget = hitVertical.transform;

                    velocity = Vector2.zero;
                    characterController.Move(velocity);
                    transform.position = CalculatePositionOnLedge(climbOffsetPos);
                    //reset other value
                    isWallSliding = false;
                    if (isSliding)
                        SlideOff();

                    StartCoroutine(ClimbingLedgeCo(false));
                    return true;
                }
            }
        }
        return false;
    }

    private Vector3 CalculatePositionOnLedge(Vector3 offset)
    {
        Vector3 newPos = new Vector3(ledgePoint.x - (characterController.radius * (isFacingRight ? 1 : -1)) - offset.x, ledgePoint.y - offset.y, transform.position.z);
     
        return newPos;
    }

    IEnumerator ClimbingLedgeCo(bool lowClimb)
    {
        isRunning = false;
        climbingState = ClimbingState.ClimbingLedge;

        if (lowClimb)
            anim.SetBool("lowLedgeClimbing", true);
        else
            anim.SetBool("ledgeClimbing", true);

        HandleAnimation();
        yield return new WaitForSeconds(Time.deltaTime);
        characterController.enabled = false;
        anim.applyRootMotion = true;
        transform.position = CalculatePositionOnLedge(lowClimb? climbLCOffsetPos : climbOffsetPos);
        yield return new WaitForSeconds(Time.deltaTime);
        transform.position = CalculatePositionOnLedge(lowClimb ? climbLCOffsetPos : climbOffsetPos);

        yield return new WaitForSeconds(lowClimb ? climbingLBObjTime : climbingLedgeTime);
        LedgeReset();
    }

    void LedgeReset()
    {
        characterController.enabled = true;
        anim.applyRootMotion = false;
        climbingState = ClimbingState.None;
        anim.SetBool("ledgeClimbing", false);
        anim.SetBool("lowLedgeClimbing", false);
        ledgeTarget = null;
    }

    #endregion

    #region JET PACK
    [Header("---JET PACK---")]
    public float jetForce = 5;
    public float jetpackDrainTimeOut = 5f;
    [ReadOnly] public float jetpackRemainTime;

    public GameObject jetpackObj;
    public AudioClip jetpackSound;
    [Range(0f, 1f)]
    public float jetpackSoundVolume = 0.5f;
    AudioSource jetpackAScr;
    public ParticleSystem[] jetpackEmission;
    public GameObject jetpackReleaseObj;
    [ReadOnly] public bool isJetpackActived = false;
    [ReadOnly] public bool isUsingJetpack = false;

    public void ActiveJetpack(bool active)
    {
        if (active)
        {
            if (isSliding)
                SlideOff();

            jetpackRemainTime = jetpackDrainTimeOut;
            isJetpackActived = true;
            jetpackObj.SetActive(true);
        }
        else if(isJetpackActived)
        {
            isJetpackActived = false;
            isUsingJetpack = false;
            jetpackObj.SetActive(false);

            var obj = Instantiate(jetpackReleaseObj, jetpackObj.transform.position, jetpackReleaseObj.transform.rotation);
            obj.GetComponent<Rigidbody>().velocity = new Vector2(isFacingRight ? -1 : 1, 2);
            Destroy(obj, 2);
        }
    }

    public void AddJetpackFuel()
    {
        jetpackRemainTime = jetpackDrainTimeOut;
    }

    public void UseJetpack(bool use)
    {
        if (!isJetpackActived)
            return;

        if (climbingState != ClimbingState.None)
            return;

        if (jetpackRemainTime <= 0)
            return;

        if (isWallSliding)
            isWallSliding = false;

        isFallingFromWall = false;      //reset falling state if it is happing
        wallStickTimeCounter = wallStickTime;       //set reset wall stick timer when on ground

        if (isUsingJetpack)
            isRunning = false;

        isUsingJetpack = use;
    }

    void UpdateJetPackStatus()
    {
        for (int i = 0; i < jetpackEmission.Length; i++)
        {
            var emission = jetpackEmission[i].emission;
            emission.enabled = isUsingJetpack;
            jetpackAScr.volume = (isUsingJetpack & GlobalValue.isSound)? jetpackSoundVolume : 0;
        }
    }

    #endregion

    #region ROPE SYSTEM
    [Header("---ROPE--")]
    public Vector3 rotateAxis = Vector3.forward;
    public float speed = 100;
    public float releaseForce = 10;
    float distance, releasePointY;
    public float ropeCheckRadius = 6;
    [Tooltip("draw rope offset")]
    public Vector2 grabOffset = new Vector2(0, 1.6f);
    LineRenderer ropeRenderer;
    public AudioClip soundGrap, soundRopeJump;

    [ReadOnly] public bool isGrabingRope = false;
    [ReadOnly] public RopePoint currentAvailableRope;
    public LayerMask layerAsRope;
    RopePoint lastRopePointObj;
   [ReadOnly] public  bool isJumpingOutFromRope = false;

    void SetNoRope()
    {
        if (currentAvailableRope != null)       //set time scale back to normal if it active the slow motion before but player don't grab
        {
            if (currentAvailableRope.showTutorial)
                Time.timeScale = 1;
        }

        currentAvailableRope = null;
    }

    void CheckRopeInZone()
    {
        if (isGrabingRope)
            return;

        var hits = Physics.OverlapSphere(transform.position + Vector3.up * characterController.height * 0.5f, ropeCheckRadius, layerAsRope);
        
        if (hits.Length > 0)
        {
            for (int i = 0; i < hits.Length; i++)
            {
                if (isFacingRight)
                {
                    if (hits[i].transform.position.x > transform.position.x)
                    {
                        currentAvailableRope = hits[i].GetComponent<RopePoint>();
                        if (lastRopePointObj != currentAvailableRope)
                        {
                            if (currentAvailableRope.showTutorial)
                                Time.timeScale = 0.1f;
                        }
                        else
                            SetNoRope();
                    }
                    else
                        SetNoRope();
                }
                else
                {
                    if (hits[i].transform.position.x < transform.position.x)
                    {
                        currentAvailableRope = hits[i].GetComponent<RopePoint>();
                        if (lastRopePointObj != currentAvailableRope)
                        {
                            if (currentAvailableRope.showTutorial)
                                Time.timeScale = 0.1f;
                        }
                        else
                            SetNoRope();
                    }
                    else
                        SetNoRope();
                }
            }
        }
        else
        {
            SetNoRope();
        }
    }

    public void GrabRope()
    {
        if (isGrabingRope)
            return;

        if (isGrounded)
            return;     //don't allow grab rope when standing on ground

        if (lastRopePointObj != currentAvailableRope)
        {
            isRunning = false;
            if (currentAvailableRope.showTutorial)
                Time.timeScale = 1;

            lastRopePointObj = currentAvailableRope;
            isGrabingRope = true;
            SoundManager.PlaySfx(soundGrap);
            distance = Vector2.Distance(transform.position, currentAvailableRope.transform.position);
            releasePointY = currentAvailableRope.transform.position.y - distance / 10f;
        }
    }

    public void GrabRelease()
    {
        if (!isGrabingRope)
            return;

        velocity = releaseForce * transform.forward;
        characterController.Move(velocity * Time.deltaTime);
        Time.timeScale = 1;
        SoundManager.PlaySfx(soundRopeJump);
        isGrabingRope = false;
        isJumpingOutFromRope = true;
        //input = Vector2.zero; //prevent error
    }

    #endregion

    #region PIPE
    public void SetInThePipe(bool state, Vector2 direction)
    {
        if (MovingInPipeCoWork != null)
            StopCoroutine(MovingInPipeCoWork);
        if (state)
        {
            isMovingInPipe = true;
            MovingInPipeCoWork = MovingInPipeCo(direction);
            StartCoroutine(MovingInPipeCoWork);
        }
        else
        {
            isMovingInPipe = false;
            ControllerInput.Instance.StopMove(0);
            velocity.y = -10;
        }
    }

    bool isMovingInPipe = false;
    float movingPipeSpeed = 1.5f;
    IEnumerator MovingInPipeCoWork;
    IEnumerator MovingInPipeCo(Vector2 direction)
    {

        Vector2 movingTarget = Vector2.zero;
        if (direction.y == -1)
        {
            movingTarget = transform.position + Vector3.down * 2;

        }
        else if (direction.x == 1)
        {
            movingTarget = transform.position + Vector3.right;
        }

        while (true)
        {
            transform.position = Vector3.MoveTowards(transform.position, movingTarget, movingPipeSpeed * Time.deltaTime);
            yield return null;
        }
    }
    #endregion

    #region HANGING TOP PIPE
    public LayerMask layerTopPipe;
    [ReadOnly] public bool isHangingTopPipe = false;
    public Vector2 hangingOffset = new Vector2(0, -1.2f);
    public float hangingMoveSpeed = 3;

    void CheckGrabHangingTopPipe()
    {
        if (isHangingTopPipe)
            return;
        if (velocity.y <= 0)
            return;

        RaycastHit hit;

        if (Physics.SphereCast(transform.position + Vector3.up * characterController.radius, characterController.radius, Vector3.up, out hit, characterController.height - characterController.radius, layerTopPipe))
        {
            if (!isFacingRight)
                Flip();
            isRunning = false;
            isHangingTopPipe = true;
            characterController.enabled = false;
            transform.position = hit.point + (Vector3) hangingOffset;
            characterController.enabled = true;
        }
    }

    void CheckIfInTopPipe()
    {
        RaycastHit hit;

        if (!Physics.SphereCast(transform.position + Vector3.up * 0.5f + Vector3.right * 0.2f * input.x, characterController.radius, Vector3.up, out hit, characterController.height, layerTopPipe))
        {
            DropHangingTopPipe();
        }
    }

    public void DropHangingTopPipe()
    {
        isHangingTopPipe = false;
    }
    #endregion

    private void OnAnimatorMove()
    {
        // Vars that control root motion
        if (!anim || !anim.applyRootMotion)
            return;

        bool useRootMotion = true;
        bool verticalMotion = true;
        Vector3 multiplier = Vector3.one;

        if (Mathf.Approximately(Time.deltaTime, 0f) || !useRootMotion) { return; } // Conditions to avoid animation root motion

        Vector3 delta = anim.deltaPosition;

        delta.z = 0;
        delta = transform.InverseTransformVector(delta);
        delta = Vector3.Scale(delta, multiplier);
        delta = transform.TransformVector(delta);

        Vector3 vel = (delta) / Time.deltaTime; // Get animator movement

        if (!verticalMotion)
            vel.y = characterController.velocity.y; // Preserve vertical velocity

        characterController.Move(vel * Time.deltaTime);
    }

    public void TakeDamage(int damage, Vector2 force, GameObject instigator, Vector3 hitPoint)
    {
        Die(instigator.GetComponent<EnemyFlowerMonster>() != null);
    }
}
