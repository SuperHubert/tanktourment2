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
    }

    public struct DamageData
    {
        [field: SerializeField] public Vector3 ExplosionOrigin { get; private set; }
        [field: SerializeField] public Tank Shooter  { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        
        public DamageData(Vector3 explosionOrigin, Tank shooter, int damage)
        {
            ExplosionOrigin = explosionOrigin;
            Shooter = shooter;
            Damage = damage;
        }
    }

    private Tank owner;
    
    private int damage;
    private float explosionRadius;
    
    public void OnEnable()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        owner = null;
        rb.velocity = Vector3.zero;
        trailRenderer.Clear();
    }

    public void Shoot(ProjectileData data,Tank shooter)
    {
        Cleanup();
        
        owner = shooter;
        
        trailRenderer.colorGradient = data.Color;
        
        var velocity = data.Velocity * transform.forward;
        
        damage = data.Damage;
        explosionRadius = data.ExplosionRadius;
        
        rb.AddForce(velocity);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == owner.gameObject.layer) return;
        
        gameObject.SetActive(false);

        var position = transform.position;
        
        var data = new DamageData(position, owner, damage);
        
        // TODO - explosion feedback
        //fx
        //maybe push stuff around
        
        var colliders = Physics.OverlapSphere(position, explosionRadius);
        
        foreach (var col in colliders)
        {
            Debug.DrawLine(position,col.transform.position,Color.red,1f);
            
            if (col.TryGetComponent(out IDamageable damageable))
            {
                //raycast to see if there is no wall between the explosion and the damageable
                
                damageable.TakeDamage(data);
            }
        }
    }

    private void OnDrawGizmos()
    {
        var color = Color.red;
        color.a = 0.5f;
        Gizmos.color = color;
        
        Gizmos.DrawWireSphere(transform.position,explosionRadius);
    }
}
