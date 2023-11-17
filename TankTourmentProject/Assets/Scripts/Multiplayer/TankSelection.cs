using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankSelection : MonoBehaviour
{
    [field: SerializeField] public Transform TankTr { get; private set; }
    public Vector3 TankTrOrigin { get; private set; }
    [field: SerializeField] public Transform CamTr { get; private set; }
    
    [SerializeField] private GameObject readyIndicatorGo; //TODO, put something on canvas jsp
    
    [SerializeField] private Canvas canvas;
    
    [SerializeField] private GameObject[] allGameObjects;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI statsTextBot;
    
    public event Action<Vector2> OnColorChanged;
    public event Action<int> OnTankChanged;
    
    private PlayerController playerController;
    private TankSelectionData tankSelectionData => playerController.TankSelectionData;
    
    private readonly List<Tank> tankModels = new List<Tank>();
    
    private InputAction acceptAction;
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction shootAction2;
    private InputAction pointerLookAction;
    
    private float colorCooldown;
    private float currentColorCooldown;

    public void ConnectToPlayerController(PlayerController controller,int layer)
    {
        playerController = controller;
        
        var inputs = playerController.PlayerInput;
        
        acceptAction = inputs.actions["Accept"];
        moveAction = inputs.actions["Move"];
        lookAction = inputs.actions["Look"];
        shootAction = inputs.actions["Shoot"];
        shootAction2 = inputs.actions["Shoot2"];
        pointerLookAction = inputs.actions["LookMouse"];

        var camController = playerController.CameraController;
        var camTr = camController.CamTransform;
        var cam = camController.Cam;
        
        camController.SetFowAngle(0);
        
        camTr.SetPositionAndRotation(CamTr.position,CamTr.rotation);
        
        //SetLayer(layer);
        
        readyIndicatorGo.SetActive(tankSelectionData.IsReady);

        canvas.worldCamera = cam;
        canvas.planeDistance = 0.5f;
        
        ConnectInputs();
    }

    public void SetColorCooldown(float value)
    {
        colorCooldown = value;
        currentColorCooldown = colorCooldown;
    }

    public void CleanupEvents()
    {
        OnColorChanged = null;
        OnTankChanged = null;
    }

    private void ConnectInputs()
    {
        acceptAction.performed += ToggleReady;
        
        moveAction.performed += HandleMoveInput;
        moveAction.canceled += HandleMoveInput;

        shootAction.performed += NextTank;
        shootAction2.performed += PreviousTank;
    }
    
    public void DisconnectInputs()
    {
        acceptAction.performed -= ToggleReady;
        
        moveAction.performed -= HandleMoveInput;
        moveAction.canceled -= HandleMoveInput;
        
        shootAction.performed -= NextTank;
        shootAction2.performed -= PreviousTank;
    }
    
    private void ToggleReady(InputAction.CallbackContext context)
    {
        tankSelectionData.SetReady(!tankSelectionData.IsReady);
        
        // Feedback
        readyIndicatorGo.SetActive(tankSelectionData.IsReady);
    }
    
    private void HandleMoveInput(InputAction.CallbackContext context)
    {
        ChangeColor();
    }
    
    private void NextTank(InputAction.CallbackContext context)
    {
        OnTankChanged?.Invoke(1);
    }
    
    private void PreviousTank(InputAction.CallbackContext context)
    {
        OnTankChanged?.Invoke(-1);
    }

    private void Update()
    {
        if (currentColorCooldown > 0)
        {
            currentColorCooldown -= Time.deltaTime;
        }
        
        ChangeColor();
    }

    private void ChangeColor()
    {
        var move = moveAction.ReadValue<Vector2>();

        if (move == Vector2.zero)
        {
            currentColorCooldown = 0;
            return;
        }   
        
        if(currentColorCooldown > 0) return;
        
        currentColorCooldown = colorCooldown;
        
        OnColorChanged?.Invoke(move);
    }

    public void ChangeColor(Color col)
    {
        foreach (var tank in tankModels)
        {
            tank.SetColor(col);
        }
    }

    public void UpdateStats(Tank tank)
    {
        var stats = tank.Stats;
        var projectile = stats.ProjectileData;
        var hrs = stats.HeadRotationSpeed;
        var hrsString = hrs == -1f ? "infinie" : $"{hrs}";
        var sorc = stats.DualShooter ? $" / ×{tank.ShotOriginsRightCount}" : "";

        statsText.text = $"Max Hp: {stats.MaxHp}\n" +
                         $"Heal: +{stats.HealAmount}/{stats.TimeBetweenHeal}s\n" +
                         $"Heal Cooldown: {stats.HealCooldown}" +
                         "\n" +
                         $"Speed: {stats.MaxSpeed}\n" +
                         $"Acceleration: {stats.Acceleration}\n" +
                         $"Max Turn Speed: {stats.MaxTurnSpeed}\n" +
                         $"Max Rotation Speed: {hrsString}\n";
        
        statsTextBot.text = $"Fire Rate: {stats.ShootCooldown} (×{tank.ShotOriginsLeftCount}{sorc})\n" +
                            $"Damage: {projectile.Damage}\n" +
                            $"KB Force: {stats.ShootKnockBackForce}\n" +
                            $"Self Damage Multiplier: {stats.SelfDamageMultiplier}\n" +
                            "\n" +
                            $"Bullet Speed: {projectile.Velocity}\n" +
                            $"Explosion Size: {projectile.ExplosionRadius}\n" +
                            $"Explosion KB: {projectile.ExplosionForce}\n";
    }

    public void SetLayer(int layer)
    {
        foreach (var go in allGameObjects)
        {
            go.layer = layer;
        }

        foreach (var tank in tankModels)
        {
            tank.SetLayer(layer);
        }
    }

    public void SetTanks(Tank[] tanksToSpawn,float tankOffset)
    {
        tankModels.Clear();

        TankTrOrigin = TankTr.position;
        
        for (var index = 0; index < tanksToSpawn.Length; index++)
        {
            var tankToSpawn = tanksToSpawn[index];
            var model = Instantiate(tankToSpawn, TankTr);
            model.transform.localPosition = Vector3.right * tankOffset * index;
            model.SetStatic();
            tankModels.Add(model);
        }
    }
}
