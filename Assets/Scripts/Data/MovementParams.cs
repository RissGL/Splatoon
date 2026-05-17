
using UnityEngine;

[System.Serializable]
public struct MovementParams 
{
    /// <summary>
    /// 最大移动速度
    /// </summary>
    [Label("最大移动速度")]
    public float maxSpeed;

    /// <summary>
    /// 加速度
    /// </summary>
    [Label("加速度")]
    public float acceleration;

    /// <summary>
    /// 减速度
    /// </summary>
    [Label("减速度")]
    public float deceleration;

    /// <summary>
    /// 重力系数
    /// </summary>
    [Label("重力系数，0为无重力，1为正常重力")]
    public float gravityScale;

    /// <summary>
    /// 跳跃初速度
    /// </summary>
    [Label("跳跃初速度")]
    public float jumpForce;

    /// <summary>
    /// 是否能跳
    /// </summary>
    [Label("是否允许跳跃")]
    public bool canJump;

    /// <summary>
    /// 空中控制能力
    /// </summary>
    [Label("空中转向能力（0-1）")]
    public float airControl;

    /// <summary>
    /// 爬墙速度
    /// </summary>
    [Label("爬墙速度（仅乌贼形态有效）")]
    public float wallClimbSpeed;

}