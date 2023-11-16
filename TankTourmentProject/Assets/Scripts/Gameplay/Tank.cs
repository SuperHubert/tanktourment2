using System;
using UnityEngine;

public class Tank : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [field: SerializeField] public Transform HeadTransform { get; private set; }
    [SerializeField] private Transform canonTip;
    [field:SerializeField] public Renderer[] ColoredRenderers { get; private set; }
    [SerializeField] public GameObject[] layerGameobjects;
    [SerializeField] private Transform[] raycastOrigins;
    [SerializeField] private Transform[] shotOrigins;
    [SerializeField] private Projectile projectilePrefab;
    
    [Serializable]
    private class TankStats
    {
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public float MaxSpeed { get; private set; } = 10f;
        [field: SerializeField] public float Acceleration { get; private set; } = 10f;
        [field: SerializeField] public float MaxTurnSpeed { get; private set; } = 10f;
        [field: SerializeField] public float HeadRotationSpeed { get; private set; }= 360f;
        [field: SerializeField, Range(0f, 360f), Tooltip("°")] public float MaxVisibilityAngle{  get; private set; } = 90f;
        [field: Space]
        [field: SerializeField] public Projectile.ProjectileData ProjectileData { get; private set; }
        [field: SerializeField] public float ShootCooldown { get; private set; } = 1f;
        [field: SerializeField] public float SelfDamageMultiplier { get; private set; } = 1f;
        [field: Space]
        [field: SerializeField] public int HealAmount { get; private set; } = 1;

        [field: SerializeField] public float TimeBetweenHeal { get; private set; } = 1f;
        [field: SerializeField] public float HealCooldown { get; private set; } = 1f;
        
    }

    [SerializeField] private TankStats stats;
    
    [Header("Settings")]
    [SerializeField] private bool moveTowardsDirection = true;
    public float MaxVisibilityAngle =>  currentHp > 0 ? visibilityOverride < 0 ? stats.MaxVisibilityAngle : visibilityOverride : 0;
    [field: SerializeField] public float SpawnHeight { get; private set; } = 1f;

    [Header("Debug")]
    [SerializeField] private float visibilityOverride = -1f;
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private Vector3 headDirection;
    
    private float currentHp;
    private float currentHealCooldown = 0f;
    private float currentShootCooldown = 0f;

    public float CurrentHp
    {
        get => currentHp;
        set
        {
            currentHp = (value > stats.MaxHp) ? stats.MaxHp : value;
            foreach (var rend in ColoredRenderers)
            {
                rend.material.SetFloat(Hp,currentHp / stats.MaxHp);
            }
        }
    }
    
    private static readonly int Hp = Shader.PropertyToID("_Hp");

    public PointsManager.PointAmount PointAmount { get; private set; }
    public Color Color { get; private set; }
    
    public int Layer { get; private set; }
    public event Action<Tank,Tank> OnTankKilled;
    public event Action OnTankRespawned;
    public Vector3 Position => transform.position;
    
    public void SetStatic()
    {
        rb.isKinematic = true;
        currentHp = stats.MaxHp;
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

        CurrentHp = stats.MaxHp;
        
        OnTankRespawned?.Invoke();
    }
    private void DecreaseHealthRegenCoolDown()
    {
        currentHealCooldown -= Time.deltaTime;
        
        if (currentHealCooldown > 0 || currentHp >= stats.MaxHp) return;
        
        currentHealCooldown = stats.TimeBetweenHeal;
        
        CurrentHp += stats.HealCooldown;
    }
    
    private void Update()
    {
        DecreaseCooldown();
        DecreaseHealthRegenCoolDown();
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

        if (stats.HeadRotationSpeed < 0)
        {
            HeadTransform.forward = headDirection; // TODO : make it smooth (lerp)
            return;
        }
        
        var targetRotation = Quaternion.LookRotation(headDirection);
        var rotation = Quaternion.RotateTowards(HeadTransform.rotation, targetRotation, stats.HeadRotationSpeed * Time.fixedDeltaTime);

        HeadTransform.rotation = rotation;
        
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
        
        targetVelocity *= stats.MaxSpeed;
        
        velocity = Vector3.MoveTowards(velocity, targetVelocity, stats.Acceleration * Time.fixedDeltaTime);
        
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
        var rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, stats.MaxTurnSpeed * Time.fixedDeltaTime);
        
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
        currentShootCooldown = stats.ShootCooldown;
        SoundManager.instance.PlaySound(SoundManager.instance.shoot);
        foreach (var shotOrigin in shotOrigins)
        {
            var projectile =  ObjectPooler.Pool(projectilePrefab,shotOrigin.position,shotOrigin.rotation);
        
            projectile.Shoot(stats.ProjectileData,this);
        }
        
        //TODO - don't forget animation
    }

    public void TakeDamage(Projectile.DamageData data)
    {
        float damage = data.Damage;
        if (data.Shooter == this) damage *= stats.SelfDamageMultiplier;
        CurrentHp -= damage;

        currentHealCooldown = stats.HealCooldown;

        if (CurrentHp > 0) return;
        
        gameObject.SetActive(false);
            
        OnTankKilled?.Invoke(this,data.Shooter);
    }

    public bool HitByObject(Vector3 position)
    {
        foreach (var tr in raycastOrigins)
        {
            var dif = tr.position - position;
            var dir = Vector3.Normalize(dif);
            
            var dist = dif.magnitude * 1.1f;
            
            if (!Physics.Raycast(position, dir, out var hit, dist)) continue;
            
            if (hit.collider.gameObject.layer != gameObject.layer) continue;
            return true;
        }
        return false;
    }

    public void IncreaseCapturePercent(float amount)
    {
        PointAmount.IncreaseCapturePercent(amount);
    }
}

public interface IDamageable
{
    void TakeDamage(Projectile.DamageData data);

    bool HitByObject(Vector3 position);
}
