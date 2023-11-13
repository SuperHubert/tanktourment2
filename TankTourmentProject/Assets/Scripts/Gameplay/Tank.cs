using System;
using UnityEngine;

public class Tank : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;

    [Header("Settings")]
    [SerializeField] private bool moveTowardsDirection = false;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxTurnSpeed = 10f;

    [Header("Debug")]

     [SerializeField] private Vector2 targetDirection;
    
    private float speed;
    
    public void HandleInputs(Vector2 inputs)
    {
        targetDirection = inputs;
    }

    private void FixedUpdate()
    {
        HandleMovement();
        HandleRotation();
    }

    private void HandleMovement()
    {
        switch (moveTowardsDirection)
        {
            case false when targetDirection.y == 0:
            case true when targetDirection is {x: 0, y: 0}:
                return;
        }
        
        var velocity = rb.velocity;

        var targetVelocity = moveTowardsDirection ? transform.forward : transform.forward * Mathf.Sign(targetDirection.y);
        
        targetVelocity *= maxSpeed;
        
        velocity = Vector3.MoveTowards(velocity, targetVelocity, acceleration * Time.fixedDeltaTime);
        
        rb.velocity = velocity;
    }

    private void HandleRotation()
    {
        switch (moveTowardsDirection)
        {
            case false when targetDirection.x == 0:
            case true when targetDirection is {x: 0, y: 0}:
                return;
        }

        var target = moveTowardsDirection ? new Vector3(targetDirection.x,0,targetDirection.y) : (transform.right * targetDirection.x);
        var targetRotation = Quaternion.LookRotation(target);
        var rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, maxTurnSpeed * Time.fixedDeltaTime);
        
        rb.MoveRotation(rotation);
    }

    private void OnDrawGizmos()
    {
        var targetRot = moveTowardsDirection ? new Vector3(targetDirection.x,0,targetDirection.y) : (transform.right * targetDirection.x);
        
        var pos = transform.position;
        Gizmos.color = Color.red;
        Gizmos.DrawRay(pos, targetRot * maxSpeed);
        
        var targetVelocity = moveTowardsDirection
            ? new Vector3(targetDirection.x, 0, targetDirection.y) * maxSpeed
            : transform.forward; 
        
        Gizmos.color = Color.yellow;
        Gizmos.DrawRay(pos, targetVelocity * maxSpeed);
    }

    public void Shoot()
    {
        
    }
}
