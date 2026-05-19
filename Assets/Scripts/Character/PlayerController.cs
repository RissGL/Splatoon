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
        };

        // 订阅输入事件
        inputReader.inputData.OnJumpPressed += HandleJump;
        inputReader.inputData.OnSquidToggled += HandleSquidToggle;
        inputReader.inputData.OnShootToggled += HandleShootToggle;

        // 订阅环境事件
        detector = GetComponent<EnvironmentDetector>();
        detector.OnEnteredAllyInk += HandleEnteredAllyInk;
        detector.OnExitedAllyInk += HandleExitedAllyInk;
        detector.OnWallDetected += HandleWallDetected;
        detector.OnWallLost += HandleWallLost;
    }

    private void Start()
    {
        moveSystem = new MoveSystem(inputReader.inputData, runtimeState,
            characterController,playerConfig.humanMovement,detector);

        SetInitialState(PlayerMovementState.HumanRun);

        humanAni = new HumanAni(characterAppearance.humanAnimator, inputReader.inputData, moveSystem, transform);
        squidAni = new SquidAni(characterAppearance.squidAnimator, inputReader.inputData, moveSystem);

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
        if (inputReader.inputData.squidInput&&!runtimeState.isSquid)
        {
            ChangeToSquid();
        }
        if (!inputReader.inputData.squidInput&&runtimeState.isSquid)
        {
            ChangeToHuman();
        }

        moveSystem.Update(Time.deltaTime);

        CheckLanding();

        CheckWallClimb();

        currentAni.UpdateAnime(Time.deltaTime);
    }

    public void ChangeToHuman()
    {
        runtimeState.isSquid = false;
        characterAppearance.SwitchToHuman();
        inkSystem.SetResourceData(playerConfig.humanResources);
        moveSystem.SetMovementParamsSet(playerConfig.humanMovement);
        ApplyPhysics(playerConfig.humanPhysics);
        currentAni = humanAni;
    }

    public void ChangeToSquid()
    {
        runtimeState.isSquid = true;
        characterAppearance.SwitchToSquid();
        inkSystem.SetResourceData(playerConfig.squidResources);
        moveSystem.SetMovementParamsSet(playerConfig.squidMovement);
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
        if (detector == null || !detector.IsGrounded) return;

        if (!runtimeState.isSquid)
        {
            // 人类跳跃
            var jumpParams = moveSystem.GetParamsForState(PlayerMovementState.HumanAir);
            moveSystem.SetVerticalVelocity(jumpParams.jumpForce);
            ChangeState(PlayerMovementState.HumanAir);
        }
        else
        {
            // 乌贼跳跃
            var jumpParams = moveSystem.GetParamsForState(PlayerMovementState.SquidAir);
            moveSystem.SetVerticalVelocity(jumpParams.jumpForce);
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

        var currentType = currentState.stateType;
        if (currentType == PlayerMovementState.HumanRun)
        {
            ChangeToSquid();
            ChangeState(PlayerMovementState.SquidFlop);
        }
        else if (currentType == PlayerMovementState.HumanAir)
        {
            ChangeToSquid();
            ChangeState(PlayerMovementState.SquidAir);
        }
    }

    private void TryBecomeHuman()
    {
        if (!runtimeState.isSquid) return;

        var currentType = currentState.stateType;
        if (currentType == PlayerMovementState.SquidFlop || currentType == PlayerMovementState.SquidDive)
        {
            ChangeToHuman();
            ChangeState(PlayerMovementState.HumanRun);
        }
        else if (currentType == PlayerMovementState.SquidAir || currentType == PlayerMovementState.SquidWallClimb)
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
        if (isGrounded && !wasGroundedLastFrame)
        {
            OnLanded();
        }
        wasGroundedLastFrame = isGrounded;
    }

    private void OnLanded()
    {
        var stateType = currentState.stateType;

        if (stateType == PlayerMovementState.HumanAir)
        {
            ChangeState(PlayerMovementState.HumanRun);
        }
        else if (stateType == PlayerMovementState.SquidAir)
        {
            if (detector.IsOnAllyInk)
                ChangeState(PlayerMovementState.SquidFlop);
            else if (detector.IsOnEnemyInk || !detector.IsOnAllyInk)
                ChangeState(PlayerMovementState.SquidFlop); // SquidFlop 未实现，先回游泳
        }
    }

    // ────────── 爬墙检测 ──────────
    private void CheckWallClimb()
    {
        if (detector == null) return;
        if (!runtimeState.isSquid) return;

        if (detector.IsNearAllyInkWall && inputReader.inputData.moveInput.magnitude > 0.1f
            && (currentState.stateType == PlayerMovementState.SquidFlop ||
            currentState.stateType == PlayerMovementState.SquidAir||
            currentState.stateType == PlayerMovementState.SquidDive))
        {
            ChangeState(PlayerMovementState.SquidWallClimb);
        }

        if (currentState.stateType == PlayerMovementState.SquidWallClimb && !detector.IsNearAllyInkWall)
        {
            if (detector.IsGrounded)
                ChangeState(PlayerMovementState.SquidFlop);
            else
                ChangeState(PlayerMovementState.SquidAir);
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