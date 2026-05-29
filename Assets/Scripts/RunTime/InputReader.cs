using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    [SerializeField] private InputDataSo inputDataTemplate;
    public InputDataSo inputData { get; private set; }

    private Input inputActions;

    private void Awake()
    {
        inputData = Instantiate(inputDataTemplate);
        inputActions = new Input();
        //移动输入
        inputActions.Player1.Move.canceled += ctx=> inputData.moveInput=Vector2.zero;
        inputActions.Player1.Move.performed += ctx=> inputData.moveInput=ctx.ReadValue<Vector2>();

        //视角输入
        inputActions.Player1.Look.canceled += ctx => inputData.lookInput = Vector2.zero;
        inputActions.Player1.Look.performed += ctx => inputData.lookInput = ctx.ReadValue<Vector2>();

        //跳跃输入
        inputActions.Player1.Jump.performed += ctx => inputData.RaiseJump();

        // 乌贼输入
        inputActions.Player1.Squid.performed += ctx =>inputData.RaiseSquidToggle(true);
        inputActions.Player1.Squid.canceled += ctx => inputData.RaiseSquidToggle(false);

        // 射击输入：只触发事件，传递 true/false
        inputActions.Player1.Shoot.performed += ctx => 
        {
            inputData.RaiseShootToggle(true); 
            inputData.SetShootInput(true); // 设置持续状态
        };
        inputActions.Player1.Shoot.canceled += ctx => 
        {
            inputData.RaiseShootToggle(false);
            inputData.SetShootInput(false); // 设置持续状态
        };
    }

    private void OnEnable()
    {
        EnableInput();
    }

    private void OnDisable()
    {
        DisableInput();
    }
    public void EnableInput() => inputActions.Enable();
    public void DisableInput() => inputActions.Disable();
}
