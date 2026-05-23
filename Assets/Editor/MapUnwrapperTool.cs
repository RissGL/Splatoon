using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEngine.SceneManagement;

public class MapUnwrapperTool : EditorWindow
{
    private int textureSize = 2048;
    private Shader unwrapShader;

    [MenuItem("Tools/生成地图世界坐标贴图 (开源原版适配)")]
    public static void ShowWindow()
    {
        GetWindow<MapUnwrapperTool>("烘焙器");
    }

    private void OnGUI()
    {
        textureSize = EditorGUILayout.IntField("贴图分辨率:", textureSize);
        unwrapShader = (Shader)EditorGUILayout.ObjectField("Unwrap Shader:", unwrapShader, typeof(Shader), false);

        if (GUILayout.Button("开始烘焙"))
        {
            if (unwrapShader == null) unwrapShader = Shader.Find("LSQ/Effect/Paint/UnwrapWorld");
            if (unwrapShader != null) BakeMapData();
            else Debug.LogError("找不到 Unwrap Shader，请将原版 Shader 拖入框中！");
        }
    }

    private void BakeMapData()
    {
        // 创建浮点纹理承载世界坐标
        RenderTexture rt = RenderTexture.GetTemporary(textureSize, textureSize, 24, RenderTextureFormat.ARGBFloat);

        GameObject camGo = new GameObject("BakeCamera");
        Camera bakeCam = camGo.AddComponent<Camera>();

        // 严格遵守开源 Shader 注释的参数
        bakeCam.orthographic = true;
        bakeCam.orthographicSize = 1.0f;
        bakeCam.nearClipPlane = 0.0f;
        bakeCam.farClipPlane = 1.0f;
        bakeCam.aspect = 1.0f;

        bakeCam.transform.position = Vector3.zero;
        bakeCam.transform.rotation = Quaternion.identity;
        bakeCam.targetTexture = rt;
        bakeCam.backgroundColor = Color.clear;
        bakeCam.clearFlags = CameraClearFlags.SolidColor;

        // 【最关键的一行】：用无限大的包围盒替换相机的视锥体，骗过 CPU 剔除机制
        bakeCam.cullingMatrix = Matrix4x4.Ortho(-100000, 100000, -100000, 100000, -100000, 100000);

        // 执行渲染
        bakeCam.RenderWithShader(unwrapShader, "RenderType");

        // 读取像素
        RenderTexture.active = rt;
        Texture2D tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBAFloat, false);
        tex.ReadPixels(new Rect(0, 0, textureSize, textureSize), 0, 0);
        tex.Apply();

        // 编码为 .exr
        byte[] bytes = tex.EncodeToEXR(Texture2D.EXRFlags.OutputAsFloat);

        string activeSceneName = SceneManager.GetActiveScene().name;
        string dirPath = Application.dataPath + "/MapBakeData/";
        if (!Directory.Exists(dirPath)) Directory.CreateDirectory(dirPath);

        string savePath = dirPath + activeSceneName + "_WorldPos.exr";
        File.WriteAllBytes(savePath, bytes);

        // 扫尾清理
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        DestroyImmediate(camGo);
        DestroyImmediate(tex);

        AssetDatabase.Refresh();
        Debug.Log($"烘焙完成！请检查 {savePath}");
    }
}