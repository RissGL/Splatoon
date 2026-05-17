using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputReader : MonoBehaviour
{
    [SerializeField] private InputDataSo inputDataTemplate;
    public InputDataSo inputData { get; private set; }

    private Input inputActions;

    private bool isSquidButtonHeld = false;

    private void Awake()
    {
        inputData = Instantiate(inputDataTemplate);
        inputActions = new Input();
        //ÒÆ¶¯ÊäÈë
        inputActions.Player1.Move.canceled += ctx=> inputData.moveInput=Vector2.zero;
        inputActions.Player1.Move.performed += ctx=> inputData.moveInput=ctx.ReadValue<Vector2>();

        //ÌøÔ¾ÊäÈë
        inputActions.Player1.Jump.performed += ctx => inputData.RaiseJump();

        //ÎÚÔôÊäÈë
        inputActions.Player1.Squid.performed += ctx =>
        {
            isSquidButtonHeld = true;
            inputData.squidInput = true; 
        };
        inputActions.Player1.Squid.canceled += ctx =>
        {
            isSquidButtonHeld = false;
            inputData.squidInput = false;
        };

        //Éä»÷ÊäÈë
        inputActions.Player1.Shoot.performed += ctx => 
        {
            if (inputData.squidInput == true) 
            {
                inputData.squidInput = false;//Éä»÷Ê±È¡ÏûÎÚÔô×´Ì¬
            }
            inputData.shootInput = true;
        };
        inputActions.Player1.Shoot.canceled += ctx =>
        {
            if (isSquidButtonHeld)
            {
                inputData.squidInput = true;//°´×ÅÎÚÔô¼üÊ±£¬Éä»÷È¡Ïûºó»Ö¸´ÎÚÔô×´Ì¬
            }
            inputData.shootInput = false; 
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
