using System;
using UnityEngine;

[CreateAssetMenu(fileName ="InputDataSO",menuName ="InputDataSO")]
public class InputDataSo :ScriptableObject
{
    [Header("³ÖÐø×´Ì¬")]
    public Vector2 moveInput;
    public Vector2 lookInput;
    public bool squidInput { get; private set; }
    public bool shootInput { get; private set; }

    [Header("Ë²Ê±×´Ì¬")]
    public Action OnJumpPressed;
    public Action<bool> OnSquidToggled;
    public Action<bool> OnShootToggled;
    public Action OnCameraReset;

    public void RaiseJump()=> OnJumpPressed?.Invoke();
    public void RaiseSquidToggle(bool pressed) => OnSquidToggled?.Invoke(pressed);
    public void RaiseShootToggle(bool pressed) => OnShootToggled?.Invoke(pressed);

    public void SetShootInput(bool t) => shootInput = t;    
    public void SetSquidInput(bool t) => squidInput = t;

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        lookInput = Vector2.zero;
        squidInput = false;
        shootInput = false;
        OnJumpPressed = null;
    }
}