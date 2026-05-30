using Cinemachine;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.VersionControl.Asset;
using ZGameFrameWork.Core;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private GameObject humanModel;
    [SerializeField] private GameObject squidModel;

    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private PlayerRuntimeState playerRuntimeStateTemp;
    [SerializeField] private GameObject cameraTarget;

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

    private Camera mainCamera;

    private InkSurfaceDetector inkSurfaceDetector;

    [Label("墨水")]
    [SerializeField] private InkData inkData;

    [Header("射击系统")]
    [Label("主粒子")]
    [SerializeField] private ParticleSystem shootParticleSystem;
    [Label("枪口")]
    [SerializeField] private Transform splatGunNozzle;
    [Label("枪")]
    [SerializeField] private Transform gun;
    private CinemachineImpulseSource impulseSource;
    private ShootingSystem shootingSystem;

    [Label("视角")]
    [SerializeField] private CameraInputAdapter cameraInputAdapter;
    private AimTargetController aimTargetController;

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
        characterController = GetComponent<CharacterController>();
        characterAppearance = new CharacterAppearance(transform, playerConfig,humanModel,squidModel);
        runtimeState = Instantiate(playerRuntimeStateTemp);
        inkSystem = new InkSystem(runtimeState);
        impulseSource= GetComponent<CinemachineImpulseSource>();


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
        cameraInputAdapter.Initialize(inputReader.inputData, transform, cameraTarget.transform);

        // 订阅输入事件
        inputReader.inputData.OnJumpPressed += HandleJump;
        inputReader.inputData.OnSquidToggled += HandleSquidToggle;
        inputReader.inputData.OnShootToggled += HandleShootToggle;

        EventCenter.AddEventListener<bool>((int)EventID.OnSquidDiveChange, ChangeSquidModel);

        moveSystem = new MoveSystem(inputReader.inputData, runtimeState,
            characterController,playerConfig.humanMovement,detector);

        SetInitialState(PlayerMovementState.HumanRun);

        humanAni = new HumanAni(characterAppearance.humanAnimator, inputReader.inputData, moveSystem, transform);
        squidAni = new SquidAni(characterAppearance.squidAnimator, inputReader.inputData, moveSystem,transform);

        currentAni = humanAni;

        ChangeToHuman();

        shootingSystem = new ShootingSystem
    (inputReader.inputData, inkSystem, shootParticleSystem, this, splatGunNozzle,gun);
        inkSurfaceDetector = new InkSurfaceDetector(transform, inkData.inkColor);

        aimTargetController = GetComponent<AimTargetController>();
        aimTargetController.Initialize(inputReader.inputData);
    }

    private void OnDisable()
    {
        EventCenter.RemoveEventListener<bool>((int)EventID.OnSquidDiveChange, ChangeSquidModel);
    }

    private void ChangeSquidModel(bool t) 
    {
        squidModel.SetActive(t);

        StartCoroutine(CheckSquidModel());
    }

    private IEnumerator CheckSquidModel() 
    {
        yield return null;
        if (currentState.stateType == PlayerMovementState.HumanRun ||
    currentState.stateType == PlayerMovementState.HumanAir)
        {
            squidModel.SetActive(false);
        }
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

        shootingSystem.Update();

        inkSurfaceDetector.Update();

        AutoCorrectState();

        CheckLanding();

        CheckWallClimb();

        if (UnityEngine.Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("当前状态 " + currentState.stateType);
        }

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
                ChangeState(airState);
            }
        }

        if (runtimeState.isSquid && inkSurfaceDetector != null)
        {
            inkSurfaceDetector.Update();

            if (state == PlayerMovementState.SquidFlop &&
    inkSurfaceDetector.IsOnAllyInk)
            {
                ChangeState(PlayerMovementState.SquidDive);
            }
            else if (state == PlayerMovementState.SquidDive &&
    !inkSurfaceDetector.IsOnAllyInk)
            {
                ChangeState(PlayerMovementState.SquidFlop);
            }
        }
    }

    public void RotateToCamera() 
    {
        Vector3 target= new Vector3
            (cameraTarget.transform.position.x, transform.position.y, cameraTarget.transform.position.z);

        float rotateSpeed = 10f; // 旋转速度

        if (target.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(target);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed);
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
        cameraTarget.transform.localPosition=physicsData.cameraTargetPosition;
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
            return;
        }

        if (detector == null || !detector.IsGrounded) return;

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

        if (pressed)
        {
            inkSurfaceDetector.ForceCheck();
            TryBecomeSquid();
        }
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
            if (inkSurfaceDetector.IsOnAllyInk)
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
            if (inkSurfaceDetector.IsOnAllyInk)
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
                if (inkSurfaceDetector.IsOnAllyInk)
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