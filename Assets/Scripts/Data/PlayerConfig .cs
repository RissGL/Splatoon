using UnityEngine;

[CreateAssetMenu(menuName = "Data/Player Config")]
public class PlayerConfig : ScriptableObject
{
    // 人类形态的各个数据
    [Header("人形数据")]
    [Label("外观")]
    public MorphAppearanceData humanAppearance;
    [Label("移动数据")]
    public MovementParamsSet humanMovement;
    [Label("资源数据")]
    public MorphResourceData humanResources;

    // 乌贼形态的各个数据
    [Header("乌贼数据")]
    [Label("外观")]
    public MorphAppearanceData squidAppearance;
    [Label("移动数据")]
    public MovementParamsSet squidMovement;
    [Label("资源数据")]
    public MorphResourceData squidResources;
}