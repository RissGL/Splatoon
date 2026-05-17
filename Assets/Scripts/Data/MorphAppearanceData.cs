using UnityEngine;

[CreateAssetMenu(menuName = "Data/Morph Appearance")]
public class MorphAppearanceData : ScriptableObject
{
    [Label("外观名称")]
    public string appearanceName = "Default Appearance";
    [Label("外观模型")]
    public GameObject appearanceModel;
    [Label("控制器")]
    public RuntimeAnimatorController animatorController;
}
