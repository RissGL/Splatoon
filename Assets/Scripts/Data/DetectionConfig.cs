using UnityEngine;

[CreateAssetMenu(menuName = "Data/Detection Config")]
public class DetectionConfig : ScriptableObject
{
    [Header("层遮罩")]
    [Label("地面层")]
    public LayerMask groundMask;
    [Label("墨水层（预留，当前用Tag判断）")]
    public LayerMask inkMask;
    [Label("墙壁层")]
    public LayerMask wallMask;
    [Label("铁丝网层")]
    public LayerMask netMask;

    [Header("检测距离")]
    [Label("地面检测距离")]
    public float groundCheckDistance = 0.1f;
    [Label("墙壁检测距离")]
    public float wallCheckDistance = 0.6f;

    [Header("墙壁判定")]
    [Label("可攀爬墙壁最小角度")]
    [Range(60f, 90f)]
    public float minWallAngle = 75f;
}