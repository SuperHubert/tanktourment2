using System;
using UnityEngine;
using UnityEngine.Serialization;

public class Tank : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [field: SerializeField] public Transform HeadTransform { get; private set; }
    [SerializeField] private Transform canonTip;
    [field:SerializeField] public Renderer[] ColoredRenderers { get; private set; }
    [SerializeField] public GameObject[] layerGameobjects;
    [SerializeField] private Transform[] raycastOrigins;

    [Header("Settings")]
    [SerializeField] private bool moveTowardsDirection = false;
    [SerializeField] private float maxSpeed = 10f;
    [SerializeField] private float acceleration = 10f;
    [SerializeField] private float maxTurnSpeed = 10f;
    [SerializeField] private float headRotationSpeed = 360f;
    [SerializeField, Range(0f, 360f), Tooltip("°")] private float maxVisibilityAngle = 90f;
    [SerializeField] private float visibilityOverride = -1f;
    public float MaxVisibilityAngle =>  currentHp > 0 ? visibilityOverride < 0 ? maxVisibilityAngle : visibilityOverride : 0;
    
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
    private int currentHp;

    public int CurrentHp
    {
        get => currentHp;
        set
        {
            currentHp = value;
            foreach (var rend in ColoredRenderers)
            {
                rend.material.SetFloat(Hp,currentHp / (float) maxHp);
            }
        }
    }
    
    private static readonly int Hp = Shader.PropertyToID("_Hp");

    public PointsManager.PointAmount PointAmount { get; private set; }
    public Color Color { get; private set; }
    
    public int Layer { get; private set; }
    public event Action<Tank> OnTankKilled;
    public event Action OnTankRespawned;
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
    }
    
    public void SetPointAmount(PointsManager.PointAmount pa)
    {
        PointAmount = pa;
    }

    public void SetColor(Color color)
    {
        Color = color;
        foreach (var rend in ColoredRenderers)
        {
            var mat = rend.material;
            mat.color = Color;
            rend.material = mat;
        }
    }

    public void SetVisibilityOverride(float value)
    {
        visibilityOverride = value;
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

        CurrentHp = maxHp;
        
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
        
        HeadTransform.forward = headDirection; // TODO : make it smooth (lerp)
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
        SoundManager.instance.PlaySound(SoundManager.instance.shoot);
        foreach (var shotOrigin in shotOrigins)
        {
            var projectile =  ObjectPooler.Pool(projectilePrefab,shotOrigin.position,shotOrigin.rotation);
        
            projectile.Shoot(projectileData,this);
        }
        
        //TODO - don't forget animation
    }

    public void TakeDamage(Projectile.DamageData data)
    {
        CurrentHp -= data.Damage;

        if (CurrentHp > 0) return;
        
        gameObject.SetActive(false);
            
        OnTankKilled?.Invoke(data.Shooter);
    }
    
    public void IncreaseCapturePercent(float amount)
    {
        PointAmount.IncreaseCapturePercent(amount);
    }
}

public interface IDamageable
{
    void TakeDamage(Projectile.DamageData data);
}
