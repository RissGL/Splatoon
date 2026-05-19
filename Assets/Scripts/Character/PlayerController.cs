using System.Collections.Generic;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private PlayerRuntimeState playerRuntimeStateTemp;

    private PlayerRuntimeState runtimeState;
    private InkSystem inkSystem;
    private MoveSystem moveSystem;
    private CharacterAppearance characterAppearance;
    private InputReader inputReader;
    private EnvironmentDetector detector;

    private AniCtrl currentAni;
    private HumanAni humanAni;
    private SquidAni squidAni;

    private CharacterController characterController;

    private Dictionary<PlayerMovementState, MoveStateBase> states;
    private MoveStateBase currentState;

    private bool isSquidButtonHeld = false;
    private bool isShootButtonHeld = false;

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
        characterController = GetComponent<CharacterController>();
        characterAppearance = new CharacterAppearance(transform, playerConfig);
        runtimeState = Instantiate(playerRuntimeStateTemp);
        inkSystem = new InkSystem(runtimeState);

        // 创建状态实例
        states = new Dictionary<PlayerMovementState, MoveStateBase>
        {
        { PlayerMovementState.HumanRun, new HumanRunState() },
        { PlayerMovementState.HumanAir, new HumanAirState() },
        { PlayerMovementState.SquidDive, new SquidDiveState() },
        { PlayerMovementState.SquidAir, new SquidAirState() },
        { PlayerMovementState.SquidFlop, new SquidFlopState() },
        { PlayerMovementState.SquidWallClimb, new SquidClimbState() },
        };

        // 订阅环境事件
        detector = GetComponent<EnvironmentDetector>();
        detector.SetConfig(playerConfig.humanDetection);
        detector.SetController(characterController);
        detector.OnEnteredAllyInk += HandleEnteredAllyInk;
        detector.OnExitedAllyInk += HandleExitedAllyInk;
        detector.OnWallDetected += HandleWallDetected;
        detector.OnWallLost += HandleWallLost;
    }

    private void Start()
    {
        // 订阅输入事件
        inputReader.inputData.OnJumpPressed += HandleJump;
        inputReader.inputData.OnSquidToggled += HandleSquidToggle;
        inputReader.inputData.OnShootToggled += HandleShootToggle;

        moveSystem = new MoveSystem(inputReader.inputData, runtimeState,
            characterController,playerConfig.humanMovement,detector);

        SetInitialState(PlayerMovementState.HumanRun);

        humanAni = new HumanAni(characterAppearance.humanAnimator, inputReader.inputData, moveSystem, transform);
        squidAni = new SquidAni(characterAppearance.squidAnimator, inputReader.inputData, moveSystem,transform);

        currentAni = humanAni;

        ChangeToHuman();
    }
    private void SetInitialState(PlayerMovementState state)
    {
        currentState = states[state];
        currentState.OnEnter(moveSystem);
        moveSystem.SetCurrentState(currentState);
    }

    public void ChangeState(PlayerMovementState state)
    {
        currentState.OnExit(moveSystem);
        currentState = states[state];
        currentState.OnEnter(moveSystem);
        moveSystem.SetCurrentState(currentState);
    }

    private void Update()
    {
        moveSystem.Update(Time.deltaTime);

        AutoCorrectState();

        CheckLanding();

        CheckWallClimb();

        currentAni.UpdateAnime(Time.deltaTime);
    }

    private void AutoCorrectState()
    {
        var state = currentState.stateType;
        if (state == PlayerMovementState.HumanRun ||
            state == PlayerMovementState.SquidFlop ||
            state == PlayerMovementState.SquidDive)
        {
            if (detector != null && !detector.IsGrounded)
            {
                PlayerMovementState airState = runtimeState.isSquid ? PlayerMovementState.SquidAir : PlayerMovementState.HumanAir;
                Debug.Log("Auto-correcting to air state: " + airState);
                ChangeState(airState);
            }
        }
    }

    public void ChangeToHuman()
    {
        runtimeState.isSquid = false;
        characterAppearance.SwitchToHuman();
        inkSystem.SetResourceData(playerConfig.humanResources);
        moveSystem.SetMovementParamsSet(playerConfig.humanMovement);
        detector.SetConfig(playerConfig.humanDetection);
        ApplyPhysics(playerConfig.humanPhysics);
        currentAni = humanAni;
    }

    public void ChangeToSquid()
    {
        runtimeState.isSquid = true;
        characterAppearance.SwitchToSquid();
        inkSystem.SetResourceData(playerConfig.squidResources);
        moveSystem.SetMovementParamsSet(playerConfig.squidMovement);
        detector.SetConfig(playerConfig.squidDetection);
        ApplyPhysics(playerConfig.squidPhysics);
        currentAni = squidAni;
    }

    private void ApplyPhysics(MorphPhysicsData physicsData)
    {
        if (characterController == null || physicsData == null) return;
        characterController.height = physicsData.height;
        characterController.radius = physicsData.radius;
        characterController.center = physicsData.center;
        characterController.slopeLimit = physicsData.slopeLimit;
        characterController.stepOffset = physicsData.stepOffset;
    }

    // ——— 事件处理 ———
    private void HandleJump()
    {
        var state = currentState.stateType;
        bool isGroundState = (state == PlayerMovementState.HumanRun ||
                              state == PlayerMovementState.SquidFlop ||
                              state == PlayerMovementState.SquidDive);
        if (!isGroundState) 
        {
            Debug.Log("Jump pressed but not in a ground state: " + state);
            return;
        }

        if (detector == null || !detector.IsGrounded) return;

        Debug.Log("Jump pressed in state: " + state);
        var p = moveSystem.GetParamsForState(currentState.stateType);

        if (!runtimeState.isSquid)
        {
            moveSystem.SetVerticalVelocity(p.jumpForce);
            ChangeState(PlayerMovementState.HumanAir);
        }
        else
        {
            moveSystem.SetVerticalVelocity(p.jumpForce);
            ChangeState(PlayerMovementState.SquidAir);
        }
    }

    private void HandleSquidToggle(bool pressed)
    {
        isSquidButtonHeld = pressed;

        // 正在射击时不允许变乌贼
        if (isShootButtonHeld && pressed) return;

        if (pressed) TryBecomeSquid();
        else TryBecomeHuman();
    }

    private void HandleShootToggle(bool pressed)
    {
        isShootButtonHeld = pressed;

        if (pressed)
        {
            // 强制变人类
            ForceBecomeHuman();
        }
        else
        {
            // 如果还按着潜墨键，恢复乌贼
            if (isSquidButtonHeld) TryBecomeSquid();
        }
    }

    private void TryBecomeSquid()
    {
        if (runtimeState.isSquid) return;

        var cur = currentState.stateType;
        if (cur == PlayerMovementState.HumanRun)
        {
            ChangeToSquid();
            if (detector != null && detector.IsOnAllyInk)
                ChangeState(PlayerMovementState.SquidDive);
            else
                ChangeState(PlayerMovementState.SquidFlop);
        }
        else if (cur == PlayerMovementState.HumanAir)
        {
            ChangeToSquid();
            ChangeState(PlayerMovementState.SquidAir);
        }
    }

    private void TryBecomeHuman()
    {
        if (!runtimeState.isSquid) return;
        var cur = currentState.stateType;

        if (cur == PlayerMovementState.SquidWallClimb)
        {
            // 从墙上直接变人，进入空中自由落体
            ChangeToHuman();
            ChangeState(PlayerMovementState.HumanAir);
        }
        else if (cur == PlayerMovementState.SquidDive || cur == PlayerMovementState.SquidFlop)
        {
            ChangeToHuman();
            ChangeState(PlayerMovementState.HumanRun);
        }
        else if (cur == PlayerMovementState.SquidAir)
        {
            ChangeToHuman();
            ChangeState(PlayerMovementState.HumanAir);
        }
    }

    private void ForceBecomeHuman()
    {
        if (!runtimeState.isSquid) return;

        var currentType = currentState.stateType;
        if (currentType == PlayerMovementState.SquidAir || currentType == PlayerMovementState.SquidWallClimb)
        {
            ChangeToHuman();
            ChangeState(PlayerMovementState.HumanAir);
        }
        else
        {
            ChangeToHuman();
            ChangeState(PlayerMovementState.HumanRun);
        }
    }

    // ────────── 落地检测 ──────────
    private bool wasGroundedLastFrame = true;

    private void CheckLanding()
    {
        if (detector == null) return;

        bool isGrounded = detector.IsGrounded;

        bool isFalling = moveSystem.VerticalVelocity <= 0.1f;

        // 只要当前是空中状态，且碰到了地面，且没有在往天上飞，就强制落地
        if (isGrounded && isFalling
            && (currentState.stateType == PlayerMovementState.HumanAir
                || currentState.stateType == PlayerMovementState.SquidAir))
        {
            OnLanded();
        }

        wasGroundedLastFrame = isGrounded;
    }

    private void OnLanded()
    {
        var st = currentState.stateType;
        if (st == PlayerMovementState.HumanAir)
        {
            ChangeState(PlayerMovementState.HumanRun);
        }
        else if (st == PlayerMovementState.SquidAir)
        {
            if (detector.IsOnAllyInk)
                ChangeState(PlayerMovementState.SquidDive);
            else
                ChangeState(PlayerMovementState.SquidFlop);
        }
    }

    // ────────── 爬墙检测 ──────────
    private void CheckWallClimb()
    {
        if (detector == null || !runtimeState.isSquid) return;
        var cur = currentState.stateType;

        if (detector.IsNearAllyInkWall
            && inputReader.inputData.moveInput.magnitude > 0.1f
            && (cur == PlayerMovementState.SquidDive
                || cur == PlayerMovementState.SquidFlop
                || cur == PlayerMovementState.SquidAir))
        {
            ChangeState(PlayerMovementState.SquidWallClimb);
        }

        if (cur == PlayerMovementState.SquidWallClimb && !detector.IsNearAllyInkWall)
        {
            if (detector.IsGrounded)
            {
                if (detector.IsOnAllyInk)
                    ChangeState(PlayerMovementState.SquidDive);
                else
                    ChangeState(PlayerMovementState.SquidFlop);
            }
            else
            {
                ChangeState(PlayerMovementState.SquidAir);
            }
        }
    }

    // ────────── 环境事件（预留）──────────
    private void HandleEnteredAllyInk() { /* 后续实现 */ }
    private void HandleExitedAllyInk() { /* 后续实现 */ }
    private void HandleEnteredEnemyInk() { /* 后续实现 */ }
    private void HandleExitedEnemyInk() { /* 后续实现 */ }
    private void HandleWallDetected(Vector3 normal) { /* 后续实现 */ }
    private void HandleWallLost() { /* 后续实现 */ }

}