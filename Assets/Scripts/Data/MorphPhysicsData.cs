using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Morph Physics")]
public class MorphPhysicsData : ScriptableObject
{
    [Tooltip("碰撞体高度")]
    public float height = 1.8f;

    [Tooltip("碰撞体半径")]
    public float radius = 0.3f;

    [Tooltip("碰撞体中心偏移")]
    public Vector3 center = new Vector3(0, 0.9f, 0);

    [Tooltip("坡度限制")]
    public float slopeLimit = 45f;

    [Tooltip("台阶高度")]
    public float stepOffset = 0.3f;
}
