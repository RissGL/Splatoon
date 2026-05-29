using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using UnityEngine;

public class InkParticles : MonoBehaviour
{
    private ParticleSystem particles;
    private List<ParticleCollisionEvent> collisionEvents;

    [SerializeField] private float minRadius = 0.05f;
    [SerializeField] private float maxRadius = 0.2f;
    [SerializeField] private float strength = 1;
    [SerializeField] private float hardness = 1;

    [SerializeField] private Color paintColor;
    

    private void Awake()
    {
        particles=GetComponent<ParticleSystem>();
        collisionEvents= new List<ParticleCollisionEvent>();
    }

    private void OnParticleCollision(GameObject other)
    {
        Debug.Log("Á£×ÓÊÂ¼þ");
        int numCollisionEvents = particles.GetCollisionEvents(other, collisionEvents);

        Paintable p = other.GetComponent<Paintable>();
        if (p != null)
        {
            for (int i = 0; i < numCollisionEvents; i++)
            {
                Vector3 pos = collisionEvents[i].intersection;
                float radius = Random.Range(minRadius, maxRadius);
                PaintManager.Instance.paint(p, pos, radius, hardness, strength, paintColor);
            }
        }
    }
}
