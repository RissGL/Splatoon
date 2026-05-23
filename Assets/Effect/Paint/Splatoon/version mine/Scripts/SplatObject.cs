using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splatoon
{
    public class SplatObject : MonoBehaviour
    {
        private void Awake()
        {
            Renderer r = GetComponent<Renderer>();
            if (r != null)
            {
                SplatDataPool.Instance.AddRenderer(r);
            }
        }
    }
}
