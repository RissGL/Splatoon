using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;


namespace Splatoon
{
    public struct SplatCommand
    {
        public Matrix4x4 matrix;
        public Vector4 color;
        public Vector4 scaleBias;
    }

    public class SplatDataPool
    {
        static SplatDataPool instance;
        static public SplatDataPool Instance
        {
            get
            {
                if (instance == null)
                    instance = new SplatDataPool();
                return instance;
            }
        }

        /// <summary> 所有可以被绘制的游戏对象 </summary>
        public List<Renderer> SplatGOs { get; private set; }
        public void AddRenderer(Renderer renderer)
        {
            SplatGOs.Add(renderer);
        }

        /// <summary> 当前的绘制指令 </summary>
        public List<SplatCommand> SplatCommands { get; private set; }
        public void AddCommand(Vector3 hitPos, Vector3 hitNormal, float splatScale, Color splatColor)
        {
            SplatCommand newSplat;
            // world to local
            float randScale = Random.Range(0.5f, 1.5f);
            Transform newSplatTF = SplatManager.Instance.SplatDecalTF;
            newSplatTF.position = hitPos;
            newSplatTF.up = hitNormal;
            newSplatTF.RotateAround(hitPos, hitNormal, Random.Range(-180, 180));
            newSplatTF.localScale = new Vector3(randScale, randScale * 0.5f, randScale) * splatScale;
            newSplat.matrix = newSplatTF.transform.worldToLocalMatrix;
            // color
            newSplat.color = splatColor;
            // texture
            Vector2Int tileCount = SplatManager.Instance.tileCount;
            float splatscaleX = 1.0f / tileCount.x;
            float splatscaleY = 1.0f / tileCount.y;
            float splatsBiasX = Mathf.Floor(Random.Range(0, tileCount.x * 0.99f)) / tileCount.y;
            float splatsBiasY = Mathf.Floor(Random.Range(0, tileCount.y * 0.99f)) / tileCount.y;
            newSplat.scaleBias = new Vector4(splatscaleX, splatscaleY, splatsBiasX, splatsBiasY);

            SplatCommands.Add(newSplat);
        }

        public SplatDataPool() 
        {
            SplatGOs = new List<Renderer>();
            SplatCommands = new List<SplatCommand>();
        }
    }

    public class SplatManager : MonoBehaviour
    {
        public static SplatManager Instance;

        [Header("Reference")]
        public Shader unwrapWorldShader;
        public ComputeShader splatSmoothCS;
        public ComputeShader splatPaintCS;
        public Texture2D splatTexture;
        public Vector2Int tileCount = new Vector2Int(4, 4);
        
        [Header("Data")]
        public Vector2Int rtSize = new Vector2Int(1024, 1024);
        public bool smoothWorldPos;
        public RenderTexture worldPosTex;
        public RenderTexture splatTex;
        
        private Camera rtCamera;
        private ComputeBuffer splatBuffer;
        const int numThreads = 8;
        private Vector3Int numThreadGroups;

        public Transform SplatDecalTF { get; private set; }

        private void Awake()
        {
            Instance = this;
            Initialize();
        }

        // Start is called before the first frame update
        private void Start()
        {
            CreateUnwrapCamera();
            UnwrapWorld();
        }

        // Update is called once per frame
        private void Update()
        {
            PaintSplats();
        }

        // 初始化各种数据
        private void Initialize()
        {
            worldPosTex = new RenderTexture(rtSize.x, rtSize.y, 0, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            worldPosTex.enableRandomWrite = true;
            worldPosTex.Create();
            splatTex = new RenderTexture(rtSize.x, rtSize.y, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
            splatTex.enableRandomWrite = true;
            splatTex.Create();
            ClearRT(worldPosTex, splatTex);

            Shader.SetGlobalTexture("_SplatTex", splatTex);

            numThreadGroups = new Vector3Int(rtSize.x / numThreads, rtSize.y / numThreads, 1);

            SplatDecalTF = new GameObject("SplatDecal").transform;
        }

        // 清除RT中的数据
        private void ClearRT(params RenderTexture[] rts) 
        {
            CommandBuffer cb = new CommandBuffer();
            for (int i = 0; i < rts.Length; i++)
            {
                cb.SetRenderTarget(rts[i]);
                cb.ClearRenderTarget(true, true, new Color(0, 0, 0, 0));
            }
            Graphics.ExecuteCommandBuffer(cb);
            cb.Release();
        }

        // 创建展开uv1的相机
        private void CreateUnwrapCamera()
        {
            rtCamera = new GameObject("rtCameraObject").AddComponent<Camera>();
            rtCamera.transform.position = Vector3.zero;
            rtCamera.transform.rotation = Quaternion.identity;
            rtCamera.transform.localScale = Vector3.one;

            // 移除内置管线的设置，URP 不需要
            // rtCamera.renderingPath = RenderingPath.Forward; 
            rtCamera.clearFlags = CameraClearFlags.SolidColor;
            rtCamera.backgroundColor = new Color(0, 0, 0, 0);
            rtCamera.orthographic = true;
            rtCamera.nearClipPlane = 0.0f;
            rtCamera.farClipPlane = 1.0f;
            rtCamera.orthographicSize = 1.0f;
            rtCamera.aspect = 1.0f;
            rtCamera.useOcclusionCulling = false;
            rtCamera.enabled = false;

            // 添加 URP 相机数据组件，保证相机能被 URP 渲染
            var urpCameraData = rtCamera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            urpCameraData.renderType = CameraRenderType.Overlay; // 作为 Overlay 相机手动渲染
            urpCameraData.renderShadows = false;
            urpCameraData.requiresColorOption = CameraOverrideOption.Off;
            urpCameraData.requiresDepthOption = CameraOverrideOption.Off;
        }

        // 展开uv1绘制世界信息
private void UnwrapWorld() 
{
    Material unwrapWorldMaterial = new Material(unwrapWorldShader);
    List<Renderer> splatGOs = SplatDataPool.Instance.SplatGOs;

    // 构建正交投影矩阵，匹配原 Shader 的 unwrap 逻辑
    Matrix4x4 proj = Matrix4x4.Ortho(-1f, 1f, -1f, 1f, 0f, 1f);
    Matrix4x4 view = Matrix4x4.identity;
    Matrix4x4 vp = proj * view;

    CommandBuffer cb = new CommandBuffer();
    cb.SetViewProjectionMatrices(view, proj);
    cb.SetRenderTarget(worldPosTex);
    for (int i = 0; i < splatGOs.Count; i++)
    {
        cb.DrawRenderer(splatGOs[i], unwrapWorldMaterial, 0, 0);
    }

    Graphics.ExecuteCommandBuffer(cb);
    cb.Release();

    if (smoothWorldPos) 
    {
        RenderTexture temp = RenderTexture.GetTemporary(worldPosTex.descriptor);
        splatSmoothCS.SetTexture(0, "_In", worldPosTex);
        splatSmoothCS.SetTexture(0, "_Out", temp);
        splatSmoothCS.Dispatch(0, numThreadGroups.x, numThreadGroups.y, numThreadGroups.z);
        splatSmoothCS.SetTexture(0, "_In", temp);
        splatSmoothCS.SetTexture(0, "_Out", worldPosTex);
        splatSmoothCS.Dispatch(0, numThreadGroups.x, numThreadGroups.y, numThreadGroups.z);
        RenderTexture.ReleaseTemporary(temp);
    }
}

        // 执行绘制指令
        private void PaintSplats() 
        {
            List<SplatCommand> commands = SplatDataPool.Instance.SplatCommands;
            if (commands.Count > 0) 
            {
                splatBuffer = new ComputeBuffer(commands.Count, 64 + 16 + 16);
                splatBuffer.SetData(commands);
                splatPaintCS.SetBuffer(0, "_SplatCommands", splatBuffer);
                splatPaintCS.SetInt("_CommandCount", commands.Count);
                splatPaintCS.SetTexture(0, "_WorldPosTex", worldPosTex);
                splatPaintCS.SetTexture(0, "_SplatTex", splatTex);
                splatPaintCS.SetTexture(0, "_SplatTexture", splatTexture);

                splatPaintCS.Dispatch(0, numThreadGroups.x, numThreadGroups.y, numThreadGroups.z);
            
                splatBuffer.Release();
                commands.Clear();
            }
        }
    }
}
