using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trailRenderer;

    [Serializable]
    public struct ProjectileData
    {
        [field: SerializeField] public float Velocity { get; private set; }
        [field: SerializeField] public Gradient Color { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public float ExplosionRadius { get; private set; }
        [HideInInspector] public Vector3 direction;
    }
    
    private int damage;
    private float explosionRadius;
    
    public void OnEnable()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        rb.velocity = Vector3.zero;
        trailRenderer.Clear();
    }

    public void Shoot(ProjectileData data)
    {
        Cleanup();
        trailRenderer.colorGradient = data.Color;
        
        var velocity = data.Velocity * transform.forward;
        
        damage = data.Damage;
        explosionRadius = data.ExplosionRadius;
        
        rb.AddForce(velocity);
    }

    private void OnCollisionEnter(Collision other)
    {
        //Debug.Log($"Collision : {other.gameObject.name}");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Trigger : {other.gameObject.name}");
        
        gameObject.SetActive(false);
        
        
        var colliders = Physics.OverlapSphere(transform.position, explosionRadius);

        foreach (var col in colliders)
        {
            if (col.TryGetComponent(out IDamageable damageable))
            {
                damageable.TakeDamage(damage);
            }
        }
    }
}
