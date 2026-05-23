using Splatoon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splatoon
{
    public class SplatTest : MonoBehaviour
    {
        public float maxDistance = 100;
        public LayerMask checkLayer;
        public float splatScale = 0.1f;
        public Color splatColor = Color.red;

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Input.GetMouseButton(0))
            {
                Ray ray = Camera.main.ScreenPointToRay(UnityEngine.Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, maxDistance, checkLayer))
                {
                    SplatDataPool.Instance.AddCommand(hit.point, hit.normal, splatScale, splatColor);
                }
            }
        }
    }
}
