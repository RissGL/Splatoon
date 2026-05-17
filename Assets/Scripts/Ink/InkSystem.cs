using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkSystem
{
    private PlayerRuntimeState runtimeState;
    private MorphResourceData morphResourceData;

    public InkSystem(PlayerRuntimeState runtimeState)
    {
        this.runtimeState = runtimeState;
    }

    public void SetResourceData(MorphResourceData data) 
    {
        morphResourceData = data;
        runtimeState.maxInk = data.maxInk;
    }

    public void RecoverInk()
    {
        runtimeState.currentInk = Mathf.Min(
            runtimeState.currentInk + morphResourceData.inkRechargeRate * Time.deltaTime,
            runtimeState.maxInk
        );
    }

    public bool ConsumeInk(int amount)
    {
        if (runtimeState.currentInk >= amount)
        {
            runtimeState.currentInk -= amount;
            return true;
        }
        return false;
    }
}
