using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName =("RunTimeState"))]
public class PlayerRuntimeState : ScriptableObject
{
    public float currentInk;
    public float currentHealth;
    public float currentMoveSpeed;
    public float maxInk;
    public float maxHealth;
    public bool isSquid;
}
