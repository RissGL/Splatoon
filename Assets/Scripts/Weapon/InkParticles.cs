using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class InkParticles : MonoBehaviour
{
    private ParticleSystem particles;
    private List<ParticleCollisionEvent> collisionEvents;


    [SerializeField] private InkData inkData;

    private void Awake()
    {
        particles=GetComponent<ParticleSystem>();
        collisionEvents= new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        int numCollisionEvents = particles.GetCollisionEvents(other, collisionEvents);

        Paintable p = other.GetComponent<Paintable>();
        if (p != null)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                Vector3 pos = collisionEvents[i].intersection;
                float radius = Random.Range(inkData.minRadius, inkData.maxRadius);
                PaintManager.Instance.paint(p, pos, radius, inkData.hardness, inkData.strength, inkData.inkColor);
            }
        }
    }
}
