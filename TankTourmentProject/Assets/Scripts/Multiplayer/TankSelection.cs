using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TankSelection : MonoBehaviour
{
    [field: SerializeField] public Transform TankTr { get; private set; }
    [field: SerializeField] public GridLayoutGroup ColorLayoutTr { get; private set; }
    public Vector3 TankTrOrigin { get; private set; }
    [field: SerializeField] public Transform CamTr { get; private set; }
    
    [SerializeField] private GameObject readyIndicatorGo; //TODO, put something on canvas jsp
    
    [SerializeField] private Canvas canvas;
    
    [SerializeField] private GameObject[] allGameObjects;
    [SerializeField] private TextMeshProUGUI statsText;
    [SerializeField] private TextMeshProUGUI statsTextBot1;
    [SerializeField] private TextMeshProUGUI statsTextBot2;
    
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

    public void ConnectToPlayerController(PlayerController controller)
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
        
        camTr.SetPositionAndRotation(CamTr.position,CamTr.rotation);
        
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
        var isDualShooter = stats.DualShooter;
        var alwaysUseMain = stats.AlwaysUseMain;

        statsText.text = stats.GetText();

        var attackHeader = "<size=40>Main Attack:</size>";
        var attackText = tank.Stats.MainCanonData.GetText(tank.ShotOriginsLeftCount);
        
        statsTextBot1.text = $"{attackHeader}\n{attackText}";

        statsTextBot2.text = "";
        
        if(!isDualShooter) return;

        attackHeader = "<size=40>Secondary Attack:</size>";
        if (!alwaysUseMain) attackText = tank.Stats.SecondaryCanonData.GetText(tank.ShotOriginsRightCount);
        
        statsTextBot2.text = $"{attackHeader}\n{attackText}";
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
