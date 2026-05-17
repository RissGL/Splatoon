using UnityEngine;

[CreateAssetMenu(menuName = "Data/Morph Resources")]
public class MorphResourceData : ScriptableObject
{
    [Label("最大墨水量")]
    public int maxInk;
    [Label("恢复速度")]
    public float inkRechargeRate;
}