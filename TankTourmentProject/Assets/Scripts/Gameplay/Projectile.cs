using UnityEngine;

public class Projectile : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TrailRenderer trailRenderer;
    
    public void OnEnable()
    {
        Cleanup();
    }

    private void Cleanup()
    {
        rb.velocity = Vector3.zero;
        trailRenderer.Clear();
    }

    public void Shoot(Vector3 velocity,Gradient color)
    {
        Cleanup();
        trailRenderer.colorGradient = color;
        rb.AddForce(velocity);
    }
}
