using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Tank : MonoBehaviour
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
    [SerializeField] private float projectileForce;
    [SerializeField] private int projectileDamage;

    [Header("Debug")]
    [SerializeField] private Gradient projectileGradient;
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private Vector3 headDirection;
    
    public Vector3 Position => transform.position;
    
    public void HandleMovementInputs(Vector2 inputs)
    {
        movementDirection = inputs;
    }

    public void HandleHeadInputs(Vector2 inputs)
    {
        headDirection = new Vector3(inputs.x,0,inputs.y);
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

        projectile.Shoot(canonTip.forward * projectileForce,projectileGradient);
        
        //TODO - don't forget animation
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(other);
    }
}
