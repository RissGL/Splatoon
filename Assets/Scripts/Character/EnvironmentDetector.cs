using UnityEngine;

/// <summary>
/// 环境检测器：负责收集角色周围的地形信息，如是否接地、是否在己方墨水表面、是否靠近可攀爬墙壁等。
/// 墙壁墨水归属通过异步回读 SplatManager 的全局 splatTex 动态判断，不再依赖静态 Tag。
/// 所有检测结果通过公共属性暴露，供 MoveSystem 和各个移动状态查询。
/// </summary>
public class EnvironmentDetector : MonoBehaviour
{
    [Header("调试可视化")]
    [SerializeField] private bool showGizmos = true;

    // 注入的配置
    private DetectionConfig config;
    private CharacterController characterController;
    private Color allyInkColor;                     // 己方墨水颜色，用于比对

    // ──────────── 地面检测结果 ────────────
    public bool IsGrounded { get; private set; }
    public Vector3 GroundNormal { get; private set; }
    public Collider CurrentGround { get; private set; }

    // ──────────── 墙壁检测结果 ────────────
    /// <summary>是否靠近可攀爬墙面（基于法线角度，先临时允许）</summary>
    public bool IsNearAllyInkWall { get; private set; }
    /// <summary>最近墙面的法线方向</summary>
    public Vector3 AllyInkWallNormal { get; private set; }
    /// <summary>最近墙面的碰撞体</summary>
    public Collider CurrentWall { get; private set; }
    /// <summary>最近墙面的世界坐标点（用于发起异步回读）</summary>
    public Vector3 CurrentWallPoint { get; private set; }

    // ──────────── 墨水表面检测结果 ────────────
    public bool IsOnAllyInk { get; private set; }
    public bool IsOnEnemyInk { get; private set; }

    // ──────────── 异步墨水确认 ────────────
    private bool pendingInkCheck = false;
    private bool lastAllyInkResult = true;          // 默认乐观为 true，避免刚贴墙就弹出
    private Vector3 pendingCheckPoint;
    private float inkCheckCooldown = 0.2f;          // 检测间隔
    private float lastCheckTime;

    // 内部缓存，用于 Gizmos 绘制
    private RaycastHit groundHit;
    private RaycastHit wallHit;
    private bool groundHitValid;
    private bool wallHitValid;

    private void Awake()
    {
        characterController = GetComponent<CharacterController>();
    }

    /// <summary>注入检测配置资产</summary>
    public void SetConfig(DetectionConfig newConfig) => config = newConfig;
    /// <summary>注入角色控制器</summary>
    public void SetController(CharacterController cc) => characterController = cc;
    /// <summary>注入己方墨水颜色</summary>
    public void SetAllyInkColor(Color color) => allyInkColor = color;

    /// <summary>
    /// 执行一次完整的环境检测，更新所有公共状态属性。
    /// 应在每帧的 MoveSystem.Update 或 PlayerController.Update 中调用。
    /// </summary>
    public void DetectEnvironment()
    {
        if (config == null || characterController == null)
        {
            Debug.LogWarning("EnvironmentDetector 缺少配置或控制器引用");
            return;
        }

        CheckGround();
        CheckWall();
        CheckInkSurface();
    }

    /// <summary>
    /// 地面检测：利用 CharacterController 内置接地 + SphereCast 获取地面法线和碰撞体。
    /// </summary>
    private void CheckGround()
    {
        if (characterController.isGrounded)
        {
            Vector3 origin = transform.position + characterController.center;
            float dist = characterController.height / 2 + 0.1f;

            if (Physics.SphereCast(origin, characterController.radius, Vector3.down, out groundHit, dist, config.groundMask))
            {
                IsGrounded = true;
                GroundNormal = groundHit.normal;
                CurrentGround = groundHit.collider;
                groundHitValid = true;
            }
            else
            {
                IsGrounded = false;
                CurrentGround = null;
                groundHitValid = false;
            }
        }
        else
        {
            IsGrounded = false;
            CurrentGround = null;
            groundHitValid = false;
        }
    }

    /// <summary>
    /// 墙壁检测：在四个水平方向进行射线检测，筛选出接近垂直（≥75°）的墙面。
    /// 不依赖静态 Tag，初次检测通过即设为可攀爬，后续通过异步回读 splatTex 确认墨水归属。
    /// </summary>
    private void CheckWall()
    {
        IsNearAllyInkWall = false;
        AllyInkWallNormal = Vector3.zero;
        CurrentWall = null;
        wallHitValid = false;

        Vector3 origin = transform.position + characterController.center;
        Vector3[] directions = { transform.forward, -transform.forward, transform.right, -transform.right };

        float closestDistance = float.MaxValue;
        RaycastHit closestHit = default;
        bool found = false;

        foreach (var dir in directions)
        {
            if (Physics.Raycast(origin, dir, out RaycastHit hit, config.wallCheckDistance, config.wallMask))
            {
                float angle = Vector3.Angle(Vector3.up, hit.normal);
                if (angle < 75f) continue;   // 只允许接近垂直的墙面

                if (hit.distance < closestDistance)
                {
                    closestDistance = hit.distance;
                    closestHit = hit;
                    found = true;
                }
            }
        }

        if (found)
        {
            // 先乐观允许爬墙，后续由异步回读确认
            IsNearAllyInkWall = true;
            AllyInkWallNormal = closestHit.normal;
            CurrentWall = closestHit.collider;
            CurrentWallPoint = closestHit.point;
            wallHit = closestHit;
            wallHitValid = true;

            /*
            // 控制频率发起异步回读
            if (Time.time > lastCheckTime + inkCheckCooldown && !pendingInkCheck)
            {
                RequestInkCheck(closestHit);
            }*/
        }
    }

    /*
    /// <summary>
    /// 异步回读 splatTex 中指定点的颜色，判断是否为己方墨水。
    /// </summary>
    private void RequestInkCheck(RaycastHit hit)
    {
        if (SplatManager.Instance == null || SplatManager.Instance.splatTex == null)
            return;

        // 使用光照贴图 UV (uv2) 定位 splatTex 中的像素
        float uvX = hit.lightmapCoord.x;
        float uvY = hit.lightmapCoord.y;
        RenderTexture splatTex = SplatManager.Instance.splatTex;
        int x = Mathf.RoundToInt(uvX * splatTex.width);
        int y = Mathf.RoundToInt(uvY * splatTex.height);
        x = Mathf.Clamp(x, 0, splatTex.width - 1);
        y = Mathf.Clamp(y, 0, splatTex.height - 1);

        pendingCheckPoint = hit.point;
        pendingInkCheck = true;
        lastCheckTime = Time.time;

        AsyncGPUReadback.Request(splatTex, 0, x, 1, y, 1, 0, 1, TextureFormat.RGBA32, req =>
        {
            pendingInkCheck = false;
            if (req.hasError) return;
            var color = req.GetData<Color32>()[0];
            lastAllyInkResult = IsSameColor(allyInkColor, color);
        });
    }*/

    /// <summary>
    /// 判断两个颜色是否近似（简易色差比较）
    /// </summary>
    private bool IsSameColor(Color c1, Color c2)
    {
        return Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b) < 0.1f;
    }

    /// <summary>
    /// 墨水表面判断：优先通过地面 Tag 判定，如果脚底无地面但正贴着己方墨水墙也视为接触己方墨水。
    /// 异步回读结果会影响此项：如果 lastAllyInkResult 为 false，则即使贴墙也不算己方墨水。
    /// </summary>
    private void CheckInkSurface()
    {
        // 地面优先
        if (CurrentGround != null)
        {
            IsOnAllyInk = CurrentGround.CompareTag("AllyInk");
            IsOnEnemyInk = CurrentGround.CompareTag("EnemyInk");
        }
        else if (IsNearAllyInkWall)
        {
            // 贴墙时：如果异步回读已完成且结果显示不是己方，则不算己方墨水；否则乐观视为己方墨水
            bool wallIsAlly = (pendingInkCheck) ? true : lastAllyInkResult;
            IsOnAllyInk = wallIsAlly;
            IsOnEnemyInk = !wallIsAlly;
        }
        else
        {
            IsOnAllyInk = false;
            IsOnEnemyInk = false;
        }
    }

    /// <summary>
    /// 供状态机查询异步回读的最终结果。如果仍在等待回读，返回 true（乐观策略）。
    /// </summary>
    public bool IsWallInkConfirmedAlly()
    {
        return pendingInkCheck || lastAllyInkResult;
    }

    private void OnDrawGizmos()
    {
        if (!showGizmos || config == null || characterController == null) return;

        Vector3 center = transform.position + characterController.center;
        float dist = characterController.height / 2 + 0.1f;

        // 地面检测球
        Gizmos.color = IsGrounded ? Color.green : Color.red;
        Gizmos.DrawWireSphere(center, characterController.radius);
        Gizmos.DrawWireSphere(center + Vector3.down * dist, characterController.radius);
        Gizmos.DrawLine(center, center + Vector3.down * dist);

        // 墙壁检测射线
        Gizmos.color = IsNearAllyInkWall ? Color.cyan : Color.gray;
        Vector3[] dirs = { transform.forward, -transform.forward, transform.right, -transform.right };
        foreach (var dir in dirs)
            Gizmos.DrawRay(center, dir * config.wallCheckDistance);

        // 地面法线
        if (groundHitValid)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(groundHit.point, groundHit.normal * 0.5f);
        }

        // 墙壁法线
        if (wallHitValid)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawRay(wallHit.point, wallHit.normal * 0.5f);
        }
    }
}