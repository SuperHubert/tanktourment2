using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class Tank : MonoBehaviour, IDamageable
{
    [Header("Components")]
    [SerializeField] private Rigidbody rb;
    [SerializeField] private TankFieldOfView tankFieldOfView;
    public Rigidbody Rb => rb;
    public TankFieldOfView TankFieldOfView => tankFieldOfView;
    [field: SerializeField] public Transform HeadTransform { get; private set; }
    [field:SerializeField] public Renderer[] ColoredRenderers { get; private set; }
    [SerializeField] public GameObject[] layerGameobjects;
    [SerializeField] private Transform[] raycastOrigins;
    [SerializeField] private Transform[] shotOriginsLeft;
    [SerializeField] private Transform[] shotOriginsRight;
    [SerializeField] private Projectile projectilePrefab;
    [Space]
    [SerializeField] private WheelTrailHandler[] wheelTrailHandlers;
    
    [Serializable]
    public class TankStats
    {
        [field: SerializeField] public int MaxHp { get; private set; }
        [field: SerializeField] public float MaxSpeed { get; private set; } = 10f;
        [field: SerializeField] public float Acceleration { get; private set; } = 10f;
        [field: SerializeField] public float MaxTurnSpeed { get; private set; } = 10f;
        [field: SerializeField] public float HeadRotationSpeed { get; private set; }= 360f;
        [field: Space]
        [field: SerializeField, Range(0f, 360f), Tooltip("°")] public float MaxVisibilityAngle{  get; private set; } = 90f;
        [field: SerializeField, Range(0f, 360f), Tooltip("°")] public float MaxVisibilityRange {  get; private set; } = 10f;
        [field: SerializeField] public float CloseAreaSize  {  get; private set; } = 1f;
        [field: Space]
        [field: SerializeField] public Projectile.ProjectileData ProjectileData { get; private set; }
        [field: SerializeField] public bool DualShooter { get; private set; } = false;
        [field: SerializeField] public float ShootCooldown { get; private set; } = 1f;
        [field: SerializeField] public float ShootKnockBackForce { get; private set; } = 0f;
        [field: SerializeField] public float SelfDamageMultiplier { get; private set; } = 1f;
        [field: Space]
        [field: SerializeField] public int HealAmount { get; private set; } = 1;

        [field: SerializeField] public float TimeBetweenHeal { get; private set; } = 1f;
        [field: SerializeField] public float HealCooldown { get; private set; } = 1f;
        
    }
    [Space]
    [SerializeField] private TankStats stats;
    public TankStats Stats => stats;
    public int ShotOriginsLeftCount => shotOriginsLeft.Length;
    public int ShotOriginsRightCount => shotOriginsRight.Length;
    
    [Header("Settings")]
    [SerializeField] private LayerMask explosionLayers;
    [SerializeField] private bool moveTowardsDirection = true;
    public float MaxVisibilityAngle =>  currentHp > 0 ? visibilityOverride < 0 ? stats.MaxVisibilityAngle : visibilityOverride : 0;
    public float MaxVisibilityRange =>  visibilityOverride < 0 ? stats.MaxVisibilityRange : visibilityOverride;
    [field: SerializeField] public float SpawnHeight { get; private set; } = 1f;

    [Header("Debug")]
    [SerializeField] private float visibilityOverride = -1f;
    [SerializeField] private Vector2 movementDirection;
    [SerializeField] private Vector3 headDirection;
    
    [SerializeField] private float currentHp;
    private float currentHealCooldown = 0f;
    private float currentShotCooldownLeft = 0f;
    private float currentShotCooldownRight = 0f;

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
    public bool IsAlive => currentHp > 0;
    
    private static readonly int Hp = Shader.PropertyToID("_Hp");

    public PointsManager.PointAmount PointAmount { get; private set; }
    public Color Color { get; private set; }
    
    public int Layer { get; private set; }
    public event Action<Tank,Tank> OnTankKilled;
    public event Action<Vector3> OnHeadDirectionChanged; 
    public event Action OnTankRespawned;
    public Vector3 Position => transform.position;
    
    public void SetStatic()
    {
        rb.isKinematic = true;
        tankFieldOfView.enabled = false;
        CurrentHp = stats.MaxHp;
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

        var keys = stats.ProjectileData.Color.colorKeys;
        Array.ForEach(keys, key => key.color = Color);
        
        stats.ProjectileData.Color.SetKeys(keys,stats.ProjectileData.Color.alphaKeys);
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
        OnHeadDirectionChanged?.Invoke(headDirection);
    }

    public void RespawnValues()
    {
        ShowLayerObjects(true);
        
        Array.ForEach(wheelTrailHandlers,handler => handler.ConnectToTank(this));
        
        rb.isKinematic = false;
        tankFieldOfView.enabled = true;
        
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        CurrentHp = stats.MaxHp;

        currentHealCooldown = 0f;
        currentShotCooldownLeft = 0f;
        currentShotCooldownRight = 0f;
        
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
        if(!IsAlive) return;
        DecreaseCooldown();
        DecreaseHealthRegenCoolDown();
        HandleHeadRotation();
    }

    private void FixedUpdate()
    {
        if(!IsAlive) return;
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
        DecreaseRight();
        DecreaseLeft();
        
        return;
        
        void DecreaseRight()
        {
            if(currentShotCooldownRight <= 0 ) return;
            currentShotCooldownRight -= Time.deltaTime;
        }
        
        void DecreaseLeft()
        {
            if(currentShotCooldownLeft <= 0 ) return;
            currentShotCooldownLeft -= Time.deltaTime;
        }
    }

    public void ShootRight()
    {
        if (!stats.DualShooter)
        {
            ShootLeft();
            return;
        }
        
        if(currentShotCooldownRight > 0 ) return;
        currentShotCooldownRight = stats.ShootCooldown;
        
        Shoot(shotOriginsRight);
    }
    
    public void ShootLeft()
    {
        if(currentShotCooldownLeft > 0 ) return;
        currentShotCooldownLeft = stats.ShootCooldown;
        
        Shoot(shotOriginsLeft);
    }

    private void Shoot(IEnumerable<Transform> origin)
    {
        if(!IsAlive) return;
        
        foreach (var shotOrigin in origin)
        {
            SoundManager.PlaySound(SoundManager.instance.shoot);
            
            var position = shotOrigin.position;
            var projectile =  ObjectPooler.Pool(projectilePrefab,position,shotOrigin.rotation);
            
            rb.AddExplosionForce(stats.ShootKnockBackForce, position, 1f, 0f);
            
            projectile.Shoot(stats.ProjectileData,this);
        }
    }

    public void TakeDamage(Projectile.DamageData data)
    {
        if(!IsAlive) return;
        
        var explosionOrigin = data.ExplosionOrigin;
        var explosionForce = data.Shooter.stats.ProjectileData.ExplosionForce;
        var radius = data.Shooter.stats.ProjectileData.ExplosionRadius;
        
        rb.AddExplosionForce(explosionForce, explosionOrigin, radius, 0f);
        
        float damage = data.Damage;
        if (data.Shooter == this) damage *= stats.SelfDamageMultiplier;
        CurrentHp -= damage;

        currentHealCooldown = stats.HealCooldown;

        if (CurrentHp > 0) return;

        Kill(data.Shooter);
    }

    private void Kill(Tank killer)
    {
        ShowLayerObjects(false);
        
        rb.isKinematic = true;
        tankFieldOfView.enabled = false;
        
        Array.ForEach(wheelTrailHandlers,handler => handler.StopEmission());
        
        OnTankKilled?.Invoke(this,killer);
    }

    private void ShowLayerObjects(bool value)
    {
        foreach (var go in layerGameobjects)
        {
            go.SetActive(value);
        }
        gameObject.SetActive(true);
    }

    public bool HitByObject(Vector3 position)
    {
        foreach (var tr in raycastOrigins)
        {
            var dif = tr.position - position;
            var dir = Vector3.Normalize(dif);
            
            var dist = dif.magnitude * 1.1f;
            
            if (!Physics.Raycast(position, dir, out var hit, dist,explosionLayers)) continue;
            
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
