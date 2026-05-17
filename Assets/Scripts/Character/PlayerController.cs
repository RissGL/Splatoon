using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private PlayerConfig playerConfig;
    [SerializeField] private PlayerRuntimeState playerRuntimeStateTemp;

    private PlayerRuntimeState runtimeState;
    private InkSystem inkSystem;
    private MoveSystem moveSystem;
    private CharacterAppearance characterAppearance;
    private InputReader inputReader;

    private HumanAni humanAni;

    private CharacterController characterController;

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
        characterController = GetComponent<CharacterController>();
        characterAppearance = new CharacterAppearance(transform, playerConfig);
        runtimeState = Instantiate(playerRuntimeStateTemp);
        inkSystem = new InkSystem(runtimeState);

        // 初始化为人类形态
    }

    private void Start()
    {
        moveSystem = new MoveSystem(inputReader.inputData, runtimeState,characterController);
        humanAni = new HumanAni(characterAppearance.humanAnimator, inputReader.inputData,moveSystem,transform);

        ChangeToHuman();
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

        humanAni.UpdateAnime();
    }

    public void ChangeToHuman()
    {
        runtimeState.isSquid = false;
        characterAppearance.SwitchToHuman();
        inkSystem.SetResourceData(playerConfig.humanResources);
        moveSystem.SetMovementParamsSet(playerConfig.humanMovement);
        ApplyPhysics(playerConfig.humanPhysics);
    }

    public void ChangeToSquid()
    {
        runtimeState.isSquid = true;
        characterAppearance.SwitchToSquid();
        inkSystem.SetResourceData(playerConfig.squidResources);
        moveSystem.SetMovementParamsSet(playerConfig.squidMovement);
        ApplyPhysics(playerConfig.squidPhysics);
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
}