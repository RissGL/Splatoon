using Splatoon;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Splatoon
{
    public class SplatTestForParticle : MonoBehaviour
    {
        public float splatScale = 0.1f;
        public Color splatColor = Color.red;

        private ParticleSystem particle;
        private List<ParticleCollisionEvent> collisionEvents;

        // Start is called before the first frame update
        void Start()
        {
            particle = GetComponentInChildren<ParticleSystem>();
            if (particle == null)
            {
                Debug.LogError($"在 {gameObject.name} 及其子物体上未找到 ParticleSystem，脚本无法工作！");
                enabled = false; // 直接禁用脚本，别静默
                return;
            }

            collisionEvents = new List<ParticleCollisionEvent>();
            var collision = particle.collision;
            collision.sendCollisionMessages = true;
        }

        // Update is called once per frame
        void Update()
        {
            if (UnityEngine.Input.GetMouseButton(0))
            {
                if (particle.isStopped)
                    particle.Play();
            }
            else
            {
                if (particle.isPlaying)
                    particle.Stop();
            }
        }

        private void OnParticleCollision(GameObject other)
        {
            int numCollisionEvents = particle.GetCollisionEvents(other, collisionEvents);

            SplatObject p = other.GetComponent<SplatObject>();
            if (p != null)
            {
                for (int i = 0; i < numCollisionEvents; i++)
                {
                    SplatDataPool.Instance.AddCommand(collisionEvents[i].intersection, collisionEvents[i].normal, splatScale, splatColor);
                }
            }
        }
    }
}