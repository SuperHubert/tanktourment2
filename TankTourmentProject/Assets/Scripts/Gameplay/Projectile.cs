using System;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trailRenderer;
    
    [SerializeField] private ParticleSystem explosionPrefab;
    
    private Tank owner;
    
    private Tank.ProjectileData currentData;
    
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

    public void Shoot(Tank.ProjectileData data,Tank shooter)
    {
        Cleanup();
        
        owner = shooter;
        
        trailRenderer.colorGradient = data.Color;
        
        var velocity = data.Velocity * transform.forward;
        
        currentData = data;
        
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
        var transform1 = transform;

        var offset = currentData.ExplosionOffset;
        
        var position = transform1.position - transform1.forward.normalized * offset;
        
        if (go.TryGetComponent(out IDamageable mainDamageable))
        {
            if(ReferenceEquals(mainDamageable, owner)) return;

            mainDamageable.TakeDamage(currentData,position,owner);
        }
        
        SoundManager.PlaySound(SoundManager.instance.explosion);
        
        var explosionRadius = currentData.ExplosionRadius;
        
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
            if (damageable.HitByObject(position) && damageable != mainDamageable) damageable.TakeDamage(currentData,position,owner);
        }
    }
}
