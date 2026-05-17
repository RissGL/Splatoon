using System;
using UnityEngine;

[CreateAssetMenu(fileName ="InputDataSO",menuName ="InputDataSO")]
public class InputDataSo :ScriptableObject
{
    [Header("³ÖÐø×´Ì¬")]
    public Vector2 moveInput;
    public bool squidInput;
    public bool shootInput;

    [Header("Ë²Ê±×´Ì¬")]
    public Action jumpEvent;

    public void RaiseJump()=> jumpEvent?.Invoke();

    private void OnDisable()
    {
        moveInput = Vector2.zero;
        squidInput = false;
        shootInput = false;
        jumpEvent = null;
    }
}