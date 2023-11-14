using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Tank : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform canonTip;
    
    [Header("Settings")]
    [SerializeField] private bool moveTowardsDirection = false;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxTurnSpeed = 10f;
    [Space]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private Projectile.ProjectileData projectileData;
    [Space]
    [SerializeField] private int maxHp;

    [Header("Debug")]
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private Vector3 headDirection;
    [SerializeField] private int currentHp;

    public event Action<Tank> OnTankKilled;
    
    public Vector3 Position => transform.position;
    
    public void HandleMovementInputs(Vector2 inputs)
    {
        movementDirection = inputs;
    }

    public void HandleHeadInputs(Vector2 inputs)
    {
        headDirection = new Vector3(inputs.x,0,inputs.y);
    }

    public void RespawnValues()
    {
        gameObject.SetActive(true);
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        currentHp = maxHp;
    }
    
    private void Update()
    {
        HandleHeadRotation();
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }
    
    private void HandleHeadRotation()
    {
        if(headDirection == Vector3.zero) return;
        
        headTransform.forward = headDirection; // TODO : make it smooth (lerp)
    }

    private void HandleMovement()
    {
        switch (moveTowardsDirection)
        {
            case false when movementDirection.y == 0:
            case true when movementDirection is {x: 0, y: 0}:
                return;
        }
        
        var velocity = rb.velocity;

        var targetVelocity = moveTowardsDirection ? transform.forward : transform.forward * Mathf.Sign(movementDirection.y);
        
        targetVelocity *= maxSpeed;
        
        velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        
        rb.velocity = velocity;
    }

    private void HandleRotation()
    {
        switch (moveTowardsDirection)
        {
            case false when movementDirection.x == 0:
            case true when movementDirection is {x: 0, y: 0}:
                return;
        }

        var target = moveTowardsDirection ? new Vector3(movementDirection.x,0,movementDirection.y) : (transform.right * movementDirection.x);
        var targetRotation = Quaternion.LookRotation(target);
        var rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnSpeed * Time.fixedDeltaTime);
        
        rb.MoveRotation(rotation);
    }
    
    public void Shoot()
    {
        var projectile =  ObjectPooler.Pool(projectilePrefab,canonTip.position,canonTip.rotation);
        
        projectile.Shoot(projectileData,this);
        
        //TODO - don't forget animation
    }

    public void TakeDamage(Projectile.DamageData data)
    {
        currentHp -= data.Damage;
        
        if(currentHp <= 0)
        {
            gameObject.SetActive(false);
            
            OnTankKilled?.Invoke(data.Shooter);
        }
    }
}

public interface IDamageable
{
    void TakeDamage(Projectile.DamageData data);
}
