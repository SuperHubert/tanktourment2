using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Tank : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Transform headTransform;
    [SerializeField] private Transform canonTip;
    [field:SerializeField] public Renderer[] ColoredRenderers { get; private set; }
    [SerializeField] public GameObject[] layerGameobjects;
    [SerializeField] private Transform[] raycastOrigins;

    [Header("Settings")]
    [SerializeField, Range(-1f, 1f),Tooltip("0 is 180°, -1 360 and 1 0°")] private float maxVisibilityAngle = 0f;
    [SerializeField] private LayerMask baseLayersToCheck;
    [SerializeField] private bool moveTowardsDirection = false;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxTurnSpeed = 10f;
    [SerializeField] private float headRotationSpeed = 360f;
    [Space]
    [SerializeField] private Projectile projectilePrefab;
    [SerializeField] private float shootCooldown = 1f;
    private float currentShootCooldown = 0f;
    [SerializeField] private Transform[] shotOrigins;
    [SerializeField] private Projectile.ProjectileData projectileData;
    [Space]
    [SerializeField] private int maxHp;
    [field: SerializeField] public float SpawnHeight { get; private set; } = 1f;

    [Header("Debug")]
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private Vector3 headDirection;
    [SerializeField] private int currentHp;
    [SerializeField] private LayerMask layersToCheck;
    
    private PointsManager.PointAmount pointAmount;
    
    public int Layer { get; private set; }
    public event Action<Tank> OnTankKilled;
    public event Action OnTankRespawned;
    public event Action<int,bool> OnLayerVisibleUpdated;
    public Vector3 Position => transform.position;

    public void SetStatic()
    {
        rb.isKinematic = true;
    }
    
    public void SetLayer(int layer)
    {
        Layer = layer;
        foreach (var go in layerGameobjects)
        {
            go.layer = Layer;
        }

        layersToCheck = baseLayersToCheck;
        layersToCheck &=  ~(1 << Layer);
    }
    
    public void SetPointAmount(PointsManager.PointAmount pa)
    {
        pointAmount = pa;
    }
    
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
        
        OnTankRespawned?.Invoke();
    }
    
    private void Update()
    {
        DecreaseCooldown();
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

    private void DecreaseCooldown()
    {
        if(currentShootCooldown <= 0 ) return;
        currentShootCooldown -= Time.deltaTime;
    }
    
    public void Shoot()
    {
        if(currentShootCooldown > 0 ) return;
        currentShootCooldown = shootCooldown;
        
        foreach (var shotOrigin in shotOrigins)
        {
            var projectile =  ObjectPooler.Pool(projectilePrefab,shotOrigin.position,shotOrigin.rotation);
        
            projectile.Shoot(projectileData,this);
        }
        
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

    public void CheckVisible(Tank other)
    {
        if(other == this) return;
        
        var dif = (other.transform.position - transform.position).normalized;
        var dot = Vector3.Dot(dif, headTransform.forward);

        if (dot < maxVisibilityAngle)
        {
            OnLayerVisibleUpdated?.Invoke(other.Layer,false);
            return;
        }
        
        var visible = false;
        foreach (var tr in other.raycastOrigins)
        {
            dif = tr.position - headTransform.position;
            var dir = Vector3.Normalize(dif);
            
            var dist = dif.magnitude * 1.1f;

            if (!Physics.Raycast(headTransform.position, dir, out var hit, dist, layersToCheck)) continue;
            
            if (hit.collider.gameObject.layer != other.gameObject.layer) continue;
            
            visible = true;
            break;
        }

        OnLayerVisibleUpdated?.Invoke(other.Layer,visible);
    }

    [ContextMenu("AAAAAAAAA")]
    public void IncreaseScore()
    {
        pointAmount.IncreaseAmount(420);
    }
    
    public void IncreaseScore(float amount)
    {
        pointAmount.IncreaseAmount(amount);
    }
}

public interface IDamageable
{
    void TakeDamage(Projectile.DamageData data);
}
