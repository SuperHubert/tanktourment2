using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trailRenderer;
    
    [SerializeField] private ParticleSystem explosionPrefab;

    [Serializable]
    public struct ProjectileData
    {
        [field: SerializeField] public float Velocity { get; private set; }
        [field: SerializeField] public Gradient Color { get; private set; }
        [field: SerializeField] public int Damage { get; private set; }
        [field: SerializeField] public float ExplosionRadius { get; private set; }
        [field: SerializeField] public float ExplosionForce { get; private set; }
        [field: SerializeField] public float ExplosionOffset { get; private set; }
    }

    public struct DamageData
    {
        public Vector3 ExplosionOrigin { get;}
        public Tank Shooter  { get; }
        public int Damage { get;}
        
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
    private float explosionOffset;
    
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
        explosionOffset = data.ExplosionOffset;
        
        rb.AddForce(velocity);
    }

    private void OnCollisionEnter(Collision other)
    {
        OnCollide(other.gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnCollide(other.gameObject);
    }
    
    private void OnCollide(GameObject go)
    {
        Debug.Log("Collided with " + go.name);
        
        var transform1 = transform;
        var position = transform1.position - transform1.forward.normalized * explosionOffset;
        var data = new DamageData(position, owner, damage);
        
        if (go.TryGetComponent(out IDamageable mainDamageable))
        {
            if(ReferenceEquals(mainDamageable, owner)) return;
            
            mainDamageable.TakeDamage(data);
        }
        
        
        SoundManager.PlaySound(SoundManager.instance.explosion);
        
        var explo = ObjectPooler.Pool(explosionPrefab, position, Quaternion.identity);
        explo.gameObject.transform.localScale = Vector3.one * explosionRadius * 0.8f;
        explo.Stop();
        explo.Play();

        gameObject.SetActive(false);
        
        var colliders = Physics.OverlapSphere(position, explosionRadius);
        
        foreach (var col in colliders)
        {
            Debug.DrawLine(position,col.transform.position,Color.red,1f);

            if (!col.TryGetComponent(out IDamageable damageable)) continue;
            
            //raycast to see if there is no wall between the explosion and the damageable
            if (damageable.HitByObject(position) && damageable != mainDamageable) damageable.TakeDamage(data);
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
