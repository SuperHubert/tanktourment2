using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class TankSelection : MonoBehaviour
{
    [field: SerializeField] public Transform TankTr { get; private set; }
    [field: SerializeField] public Transform CamTr { get; private set; }
    
    [SerializeField] private GameObject readyIndicatorGo; //TODO, put something on canvas jsp
    
    [SerializeField] private Transform rotator;
    
    [SerializeField] private GameObject[] allGameObjects;

    public int SelectedTankIndex { get; private set; }
    public event Action<int> OnSelectedTankIndexChanged; 
    public int SelectedColorIndex { get; private set; }
    public event Action<int> OnSelectedColorIndexChanged; 
    public bool IsReady { get; private set; }
    public event Action<bool> OnReadyChanged; 
    public event Action<Color> OnColorChanged;
    
    private PlayerController playerController;
    
    private readonly List<Tank> tankModels = new List<Tank>();
    
    private Color color;

    public void ConnectToPlayerController(PlayerController controller,int layer)
    {
        playerController = controller;
        
        var camTr = playerController.CameraController.CamTransform;
        var cam = playerController.CameraController.Cam;
        
        camTr.SetPositionAndRotation(CamTr.position,CamTr.rotation);
            
        SetLayer(layer);
        
        IsReady = false;
        readyIndicatorGo.SetActive(IsReady);
        OnReadyChanged?.Invoke(IsReady);
        
        ConnectInputs();
    }

    private void ConnectInputs()
    {
        var inputs = playerController.PlayerInput;
        
        inputs.actions["Shoot"].performed += ToggleReady;
    }
    
    public void DisconnectInputs()
    {
        var inputs = playerController.PlayerInput;
        
        inputs.actions["Shoot"].performed -= ToggleReady;
    }

    private void HandleMoveInput(InputAction.CallbackContext context)
    {
        
    }

    private void ToggleReady(InputAction.CallbackContext context)
    {
        IsReady = !IsReady;
        
        // Feedback
        readyIndicatorGo.SetActive(IsReady);
        
        OnReadyChanged?.Invoke(IsReady);
    }
    
    public void ChangeColor(Color col)
    {
        color = col;
        foreach (var tank in tankModels)
        {
            tank.SetColor(color);
        }
        
        OnColorChanged?.Invoke(color);
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

    public void SetTanks(List<Tank> tanksToSpawn)
    {
        tankModels.Clear();
        foreach (var tankToSpawn in tanksToSpawn)
        {
            var model = Instantiate(tankToSpawn, TankTr);
            model.SetStatic();
            tankModels.Add(model);
        }
    }

    public void SetColorIndex(int index)
    {
        SelectedColorIndex = index;
        OnSelectedColorIndexChanged?.Invoke(SelectedColorIndex);
    }
    
    public void NextTank()
    {
        SelectedTankIndex++;
        if (SelectedTankIndex >= tankModels.Count)
        {
            SelectedTankIndex = 0;
        }
        OnSelectedTankIndexChanged?.Invoke(SelectedTankIndex);
    }
    
    public void PreviousTank()
    {
        SelectedTankIndex--;
        if (SelectedTankIndex < 0)
        {
            SelectedTankIndex = tankModels.Count - 1;
        }
        OnSelectedTankIndexChanged?.Invoke(SelectedTankIndex);
    }
}
