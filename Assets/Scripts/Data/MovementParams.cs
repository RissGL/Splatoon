
using UnityEngine;

[System.Serializable]
public struct MovementParams 
{
    [Label("最大移动速度")]
    public float maxSpeed;

    [Label("加速度")]
    public float acceleration;

    [Label("减速度")]
    public float deceleration;

    [Label("重力系数，0为无重力，1为正常重力")]
    public float gravityScale;

    [Label("跳跃初速度")]
    public float jumpForce;

    [Label("是否允许跳跃")]
    public bool canJump;

    [Label("空中转向能力（0-1）")]
    public float airControl;

    [Label("爬墙速度（仅乌贼形态有效）")]
    public float wallClimbSpeed;

}