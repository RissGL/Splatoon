using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Splatoon;
using UnityEngine.Rendering;

namespace Splatoon
{
    [Serializable]
    public class RayCheck
    {
        public Vector3 checkOffset;
        public float checkDistance;
        public LayerMask checkLayer;

        public bool Shoot(Vector3 origion, Vector3 dir, out RaycastHit hit)
        {
            Ray ray = new Ray(origion + checkOffset, dir);
            return Physics.Raycast(ray, out hit, checkDistance, checkLayer);
        }

        public void DrawGizmos(Vector3 origion, float radius)
        {
            Gizmos.DrawSphere(origion + checkOffset, radius);
        }
    }

    public class SplatTestForDiving : MonoBehaviour
    {
        public RayCheck downCheck;

        public Color splatColor;

        public ParticleSystem wave;

        private Renderer r;

        private bool canDiving = false;

        // Start is called before the first frame update
        void Start()
        {
            r = GetComponent<Renderer>();
            wave.Stop();
        }

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Input.GetKey(KeyCode.F))
            {
                RaycastHit hit;
                if (downCheck.Shoot(transform.position, -transform.up, out hit))
                {
                    MeshRenderer mr = hit.collider.GetComponent<MeshRenderer>();
                    SplatObject so = hit.collider.GetComponent<SplatObject>();
                    if (mr != null && so != null)
                    {
                        RenderTexture splatTex = Splatoon.SplatManager.Instance.splatTex;
                        float uvX = hit.lightmapCoord.x;//hit.textureCoord2.x * mr.lightmapScaleOffset.x + mr.lightmapScaleOffset.z;
                        float uvY = hit.lightmapCoord.y;//hit.textureCoord2.y * mr.lightmapScaleOffset.y + mr.lightmapScaleOffset.w;
                        int x = Mathf.RoundToInt(uvX * splatTex.width);
                        int y = Mathf.RoundToInt(uvY * splatTex.height);
                        //Debug.Log(hit.textureCoord2.x);
                        //Debug.Log(hit.textureCoord2.y);
                        //Debug.Log(x + "_" + y);
                        AsyncGPUReadback.Request(splatTex, 0, x, 1, y, 1, 0, 1, TextureFormat.RGBA32,
                            (req) =>
                            {
                                var colorArray = req.GetData<Color32>();
                            //Debug.Log(colorArray[0]); 
                            canDiving = IsSameColor(splatColor, colorArray[0]);
                            });
                        ;
                    }
                }

                if (canDiving && r.enabled)
                    r.enabled = false;
                else if (!canDiving && !r.enabled)
                    r.enabled = true;

                if (canDiving)
                {
                    if (wave.isStopped)
                        wave.Play();
                }
                else
                {
                    if (wave.isPlaying)
                        wave.Stop();
                }
            }
            else
            {
                if (!r.enabled)
                    r.enabled = true;
                if (wave.isPlaying)
                    wave.Stop();
            }
        }

        private bool IsSameColor(Color c1, Color c2)
        {
            return Mathf.Abs(c1.r - c2.r) + Mathf.Abs(c1.g - c2.g) + Mathf.Abs(c1.b - c2.b) < 0.1f;
        }

        private void OnDrawGizmos()
        {
            downCheck.DrawGizmos(transform.position, 0.1f);
        }
    }
}
