
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Detection Config")]
public class DetectionConfig
{
    [Label("地板层")]
    public LayerMask groundMask;
    [Label("墨水层")]
    public LayerMask inkMask;           // 己方/敌方墨水层
    [Label("墙壁层")]
    public LayerMask wallMask;
    [Label("网层")]
    public LayerMask netMask;
    public float groundCheckDistance = 0.1f;
    public float wallCheckDistance = 0.6f;
    public float slopeLimit = 45f;
}