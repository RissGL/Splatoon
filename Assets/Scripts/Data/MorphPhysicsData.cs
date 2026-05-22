using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Morph Physics")]
public class MorphPhysicsData : ScriptableObject
{
    [Label("碰撞体高度")]
    public float height = 1.8f;

    [Label("碰撞体半径")]
    public float radius = 0.3f;

    [Label("碰撞体中心偏移")]
    public Vector3 center = new Vector3(0, 0.9f, 0);

    [Label("坡度限制")]
    public float slopeLimit = 45f;

    [Label("台阶高度")]
    public float stepOffset = 0.3f;

    [Label("相机目标点坐标")]
    public Vector3 cameraTargetPosition = new Vector3(0,0,0);
}
