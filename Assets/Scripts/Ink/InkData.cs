using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Data/Ink Data")]
public class InkData : ScriptableObject
{
    public float minRadius = 0.6f;
    public float maxRadius  = 0.8f;
    public float strength= 1;
    public float hardness  = 1;

    public Color inkColor = Color.red;
}
