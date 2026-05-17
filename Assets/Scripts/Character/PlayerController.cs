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

    private void Awake()
    {
        inputReader = GetComponent<InputReader>();
        characterAppearance = new CharacterAppearance(transform, playerConfig);
        runtimeState = Instantiate(playerRuntimeStateTemp);
        inkSystem = new InkSystem(runtimeState);

        // 初始化为人类形态
    }

    private void Start()
    {
        moveSystem = new MoveSystem(inputReader.inputData, runtimeState);
        humanAni = new HumanAni(characterAppearance.humanAnimator, inputReader.inputData);

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

        humanAni.Update();
    }

    public void ChangeToHuman()
    {
        runtimeState.isSquid = false;
        characterAppearance.SwitchToHuman();
        inkSystem.SetResourceData(playerConfig.humanResources);
        moveSystem.SetMovementParamsSet(playerConfig.humanMovement);
    }

    public void ChangeToSquid()
    {
        runtimeState.isSquid = true;
        characterAppearance.SwitchToSquid();
        inkSystem.SetResourceData(playerConfig.squidResources);
        moveSystem.SetMovementParamsSet(playerConfig.squidMovement);
    }
}