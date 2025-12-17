using System;
using Sirenix.OdinInspector;
using UnityEngine;
using ImprovedTimers;

/// <summary>
/// 移动,地面检测,跳跃,空中,摩擦力,重力,台阶,斜坡,动量
/// </summary>

[RequireComponent(typeof(PlayerMover))]
public class PlayerControllerAdvanced : MonoBehaviour
{
    #region Fields

    [SerializeField, Required]public InputRead input;
    [SerializeField, Required]public PlayerAttacker attacker;
    Transform tr;
    PlayerMover mover;
    CeilingDetector ceilingDetector; //天花板检测

    bool jumpKeyIsPressed; // 追踪玩家当前是否按住跳跃键
    bool jumpKeyWasPressed; // 指示自上次重置以来是否按下过跳跃键，用于检测跳跃的发起
    bool jumpKeyWasLetGo; // 指示自上次按下以来是否释放了跳跃键，用于检测何时停止跳跃
    bool jumpInputIsLocked; // 设置为 true 时阻止跳跃操作，用于确保每次按下按钮只执行一次跳跃操作

    public float movementSpeed = 7f;
    public float airControlRate = 2f;
    public float jumpSpeed = 10f;
    public float jumpDuration = 0.2f;
    public float airFriction = 0.5f;
    public float groundFriction = 100f;
    public float gravity = 30f;
    public float slideGravity = 5f;
    public float slopeLimit = 30f;
    public bool useLocalMomentum;

    StateMachine stateMachine;
    CountdownTimer jumpTimer;

    [SerializeField] Transform cameraTransform;

    Vector3 momentum, savedVelocity, savedMovementVelocity;

    public event Action<Vector3> OnJump = delegate { };
    public event Action<Vector3> OnLand = delegate { };

    #endregion

    bool IsGrounded() => stateMachine.CurrentState is GroundedState or SlidingState or FightState;
    public Vector3 GetVelocity() => savedVelocity;
    public Vector3 GetMomentum() => useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
    public Vector3 GetMovementVelocity() => savedMovementVelocity;

    #region 二段跳

    [Header("跳跃设置")]
    [SerializeField] int maxJumpCount = 2; // 最大跳跃次数
    int currentJumpCount; // 当前跳跃次数

    #endregion

    void Awake()
    {
        tr = transform;
        mover = GetComponent<PlayerMover>();
        ceilingDetector = GetComponent<CeilingDetector>();

        jumpTimer = new CountdownTimer(jumpDuration);
        SetupStateMachine();
    }

    void Start()
    {
        input.EnablePlayerActions();
        input.Jump += HandleJumpKeyInput;
    }

    void HandleJumpKeyInput(bool isButtonPressed)
    {
        if (!jumpKeyIsPressed && isButtonPressed)
        {
            jumpKeyWasPressed = true;
        }

        if (jumpKeyIsPressed && !isButtonPressed)
        {
            jumpKeyWasLetGo = true;
            jumpInputIsLocked = false;
        }

        jumpKeyIsPressed = isButtonPressed;
    }

    void SetupStateMachine()
    {
        stateMachine = new StateMachine();

        var grounded = new GroundedState(this);
        var falling = new FallingState(this);
        var sliding = new SlidingState(this);
        var rising = new RisingState(this);
        var jumping = new JumpingState(this);
        var fight = new FightState(this, attacker);

        At(grounded, rising, () => IsRising());
        At(grounded, sliding, () => mover.IsGrounded() && IsGroundTooSteep());
        At(grounded, falling, () => !mover.IsGrounded());
        At(grounded, jumping, () => (jumpKeyIsPressed || jumpKeyWasPressed) && !jumpInputIsLocked);
        At(grounded,fight, () => attacker.IsFighting);//地面攻击
        
        At(falling, rising, () => IsRising());
        At(falling, grounded, () => mover.IsGrounded() && !IsGroundTooSteep());
        At(falling, sliding, () => mover.IsGrounded() && IsGroundTooSteep());
        
        At(sliding, rising, () => IsRising());
        At(sliding, falling, () => !mover.IsGrounded());
        At(sliding, grounded, () => mover.IsGrounded() && !IsGroundTooSteep());
        At(sliding,fight, () => attacker.IsFighting);

        At(rising, grounded, () => mover.IsGrounded() && !IsGroundTooSteep());
        At(rising, sliding, () => mover.IsGrounded() && IsGroundTooSteep());
        At(rising, falling, () => IsFalling());
        At(rising, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());

        At(jumping, rising, () => jumpTimer.IsFinished || jumpKeyWasLetGo);
        At(jumping, falling, () => ceilingDetector != null && ceilingDetector.HitCeiling());
        
        At(fight, grounded, () => !attacker.IsFighting&&mover.IsGrounded());//攻击结束
        //二段跳
        // 允许从上升、下落、甚至滑墙状态再次进入跳跃
        At(rising, jumping, CanAirJump);
        At(falling, jumping, CanAirJump);
        At(sliding, jumping, CanAirJump);
        
        stateMachine.SetState(falling);
    }

    void At(IState from, IState to, Func<bool> condition) => stateMachine.AddTransition(from, to, condition);
    void Any<T>(IState to, Func<bool> condition) => stateMachine.AddAnyTransition(to, condition);

    bool IsRising() => VectorMath.GetDotProduct(GetMomentum(), tr.up) > 0f;
    bool IsFalling() => VectorMath.GetDotProduct(GetMomentum(), tr.up) < 0f;
    bool IsGroundTooSteep() => !mover.IsGrounded() || Vector3.Angle(mover.GetGroundNormal(), tr.up) > slopeLimit;

    void Update() => stateMachine.Update();

    void FixedUpdate()
    {
        stateMachine.FixedUpdate();
        mover.CheckForGround();
        HandleMomentum();
        Vector3 velocity = stateMachine.CurrentState is GroundedState ? CalculateMovementVelocity() : Vector3.zero;
        velocity += useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;

        mover.SetExtendSensorRange(IsGrounded());
        mover.SetVelocity(velocity);

        savedVelocity = velocity;
        savedMovementVelocity = CalculateMovementVelocity();

        ResetJumpKeys();

        if (ceilingDetector != null) ceilingDetector.Reset();
    }

    Vector3 CalculateMovementVelocity() => CalculateMovementDirection() * movementSpeed;

    /// <summary>
    /// 返回玩家移动在平面上的向量
    /// </summary>
    /// <returns></returns>
    Vector3 CalculateMovementDirection()
    {
        Vector3 direction = cameraTransform == null
            ? tr.right * input.Direction.x + tr.forward * input.Direction.y
            : Vector3.ProjectOnPlane(cameraTransform.right, tr.up).normalized * input.Direction.x +
              Vector3.ProjectOnPlane(cameraTransform.forward, tr.up).normalized * input.Direction.y;

        return direction.magnitude > 1f ? direction.normalized : direction;
    }

    void HandleMomentum()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        Vector3 verticalMomentum = VectorMath.ExtractDotVector(momentum, tr.up);
        Vector3 horizontalMomentum = momentum - verticalMomentum;

        verticalMomentum -= tr.up * (gravity * Time.deltaTime);
        if (stateMachine.CurrentState is GroundedState or FightState && VectorMath.GetDotProduct(verticalMomentum, tr.up) < 0f)
        {
            verticalMomentum = Vector3.zero;
        }

        if (!IsGrounded())
        {
            AdjustHorizontalMomentum(ref horizontalMomentum, CalculateMovementVelocity());
        }

        if (stateMachine.CurrentState is SlidingState)
        {
            HandleSliding(ref horizontalMomentum);
        }

        float friction = stateMachine.CurrentState is GroundedState ? groundFriction : airFriction;
        horizontalMomentum = Vector3.MoveTowards(horizontalMomentum, Vector3.zero, friction * Time.deltaTime);

        momentum = horizontalMomentum + verticalMomentum;

        if (stateMachine.CurrentState is JumpingState)
        {
            HandleJumping();
        }

        if (stateMachine.CurrentState is SlidingState)
        {
            momentum = Vector3.ProjectOnPlane(momentum, mover.GetGroundNormal());
            if (VectorMath.GetDotProduct(momentum, tr.up) > 0f)
            {
                momentum = VectorMath.RemoveDotVector(momentum, tr.up);
            }

            Vector3 slideDirection = Vector3.ProjectOnPlane(-tr.up, mover.GetGroundNormal()).normalized;
            momentum += slideDirection * (slideGravity * Time.deltaTime);
        }

        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    void HandleJumping()
    {
        momentum = VectorMath.RemoveDotVector(momentum, tr.up);
        momentum += tr.up * jumpSpeed;
    }

    void ResetJumpKeys()
    {
        jumpKeyWasLetGo = false;
        jumpKeyWasPressed = false;
    }

    public void OnJumpStart()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        if (currentJumpCount > 0)//二段跳
        {
            momentum = VectorMath.RemoveDotVector(momentum, tr.up);
        }
        currentJumpCount++; // 增加计数
        
        momentum += tr.up * jumpSpeed;
        jumpTimer.Start();
        jumpInputIsLocked = true;
        OnJump.Invoke(momentum);
        
        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    public void OnGroundContactLost()
    {
        if (useLocalMomentum) momentum = tr.localToWorldMatrix * momentum;

        Vector3 velocity = GetMovementVelocity();
        if (velocity.sqrMagnitude >= 0f && momentum.sqrMagnitude > 0f)
        {
            Vector3 projectedMomentum = Vector3.Project(momentum, velocity.normalized);
            float dot = VectorMath.GetDotProduct(projectedMomentum.normalized, velocity.normalized);

            if (projectedMomentum.sqrMagnitude >= velocity.sqrMagnitude && dot > 0f) velocity = Vector3.zero;
            else if (dot > 0f) velocity -= projectedMomentum;
        }

        momentum += velocity;

        if (useLocalMomentum) momentum = tr.worldToLocalMatrix * momentum;
    }

    public void OnGroundContactRegained()
    {
        currentJumpCount = 0; // 重置跳跃次数
        
        Vector3 collisionVelocity = useLocalMomentum ? tr.localToWorldMatrix * momentum : momentum;
        OnLand.Invoke(collisionVelocity);
    }

    public void OnFallStart()
    {
        if (currentJumpCount == 0) currentJumpCount = 1;// 如果是从台阶走下去而非跳下去的，第一次跳跃机会应视为已消耗（Coyote Time逻辑可在此扩展）
        //TODO:玩家在从边缘走下后， 只有很短的时间窗口可以跳跃
        var currentUpMomemtum = VectorMath.ExtractDotVector(momentum, tr.up);
        momentum = VectorMath.RemoveDotVector(momentum, tr.up);
        momentum -= tr.up * currentUpMomemtum.magnitude;
    }

    void AdjustHorizontalMomentum(ref Vector3 horizontalMomentum, Vector3 movementVelocity)
    {
        if (horizontalMomentum.magnitude > movementSpeed)
        {
            if (VectorMath.GetDotProduct(movementVelocity, horizontalMomentum.normalized) > 0f)
            {
                movementVelocity = VectorMath.RemoveDotVector(movementVelocity, horizontalMomentum.normalized);
            }

            horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate * 0.25f);
        }
        else
        {
            horizontalMomentum += movementVelocity * (Time.deltaTime * airControlRate);
            horizontalMomentum = Vector3.ClampMagnitude(horizontalMomentum, movementSpeed);
        }
    }

    void HandleSliding(ref Vector3 horizontalMomentum)
    {
        Vector3 pointDownVector = Vector3.ProjectOnPlane(mover.GetGroundNormal(), tr.up).normalized;
        Vector3 movementVelocity = CalculateMovementVelocity();
        movementVelocity = VectorMath.RemoveDotVector(movementVelocity, pointDownVector);
        horizontalMomentum += movementVelocity * Time.fixedDeltaTime;
    }
    
    // 二段跳判定条件
    bool CanAirJump()
    {
        return jumpKeyWasPressed && currentJumpCount < maxJumpCount && !jumpInputIsLocked;
    }
}