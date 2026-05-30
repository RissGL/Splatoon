using UnityEngine;
using UnityEngine.Rendering;

public class InkSurfaceDetector
{
    private Transform playerTransform;
    private Color allyColor;
    private float checkInterval = 0.05f;
    private float lastCheckTime;
    private bool pending;
    private bool hasResult;
    private string lastGroundName;
    private CharacterController characterController;

    public bool IsOnAllyInk { get; private set; }

    public InkSurfaceDetector(Transform playerTransform, Color allyColor,CharacterController characterController)
    {
        this.playerTransform = playerTransform;
        this.allyColor = allyColor;
        this.characterController = characterController;
    }

    public void Update()
    {
        if (pending) return;
        if (Time.time - lastCheckTime < checkInterval) return;

        lastCheckTime = Time.time;

        if (!Physics.Raycast(playerTransform.position + characterController.center, Vector3.down, out RaycastHit hit, 2f))
        {
            Debug.Log("InkSurfaceDetector: 没打到地面");

            IsOnAllyInk = false;
            return;
        }

        Paintable paintable = hit.collider.GetComponent<Paintable>();
        if (paintable == null)
        {
            Debug.Log("InkSurfaceDetector: 地面没有 Paintable → " +
hit.collider.name);
            IsOnAllyInk = false;
            return;
        }

        lastGroundName = hit.collider.name;
        hasResult = false;

        Vector2 uv = hit.lightmapCoord;
        RenderTexture mask = paintable.getExtend();
        int x = Mathf.Clamp((int)(uv.x * mask.width), 0, mask.width - 1);
        int y = Mathf.Clamp((int)(uv.y * mask.height), 0, mask.height - 1);

        pending = true;
        AsyncGPUReadback.Request(mask, 0, x, 1, y, 1, 0, 1, TextureFormat.RGBA32, req =>
        {
            pending = false;
            if (req.hasError)
            {
                Debug.Log("InkSurfaceDetector: GPU回读失败");
                IsOnAllyInk = false;
                return;
            }

            Color pixel = req.GetData<Color32>()[0];
            Debug.Log($"InkSurfaceDetector:读到颜色 = ({pixel.r:F2},{pixel.g:F2},{pixel.b:F2})己方 = " +
                $"({allyColor.r:F2},{allyColor.g:F2},{allyColor.b:F2})");

            IsOnAllyInk = Mathf.Abs(pixel.r - allyColor.r)
                        + Mathf.Abs(pixel.g - allyColor.g)
                        + Mathf.Abs(pixel.b - allyColor.b) < 0.25f;
            Debug.Log($"InkSurfaceDetector: IsOnAllyInk={IsOnAllyInk}");
            hasResult = true;
        });
    }

    public void ForceCheck()
    {
        lastCheckTime = 0f;  // 跳过冷却
        hasResult = false;   // 跳过同地面跳过
    }
}
