using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(LabelAttribute))]
public class LabelDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        LabelAttribute labelAttribute = attribute as LabelAttribute;

        label.text = labelAttribute.label;

        EditorGUI.PropertyField(position, property, label, true);
    }
}