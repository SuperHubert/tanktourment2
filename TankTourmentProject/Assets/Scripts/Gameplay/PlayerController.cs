using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    [field: SerializeField] public TankController TankController { get; private set; }
    [field: SerializeField] public CameraController CameraController { get; private set; }

    public static event Action<PlayerController> OnPlayerJoin;
    public static event Action<PlayerController> OnPlayerLeave;

    public static void CleanupEvents()
    {
        OnPlayerJoin = null;
        OnPlayerLeave = null;
    }
    
    private void Start()
    {
        InvokeJoin(PlayerInput);

        PlayerInput.deviceRegainedEvent.AddListener(InvokeJoin);
        PlayerInput.deviceLostEvent.AddListener(InvokeLeave);
    }
    
    private void InvokeJoin(PlayerInput _)
    {
        OnPlayerJoin?.Invoke(this);
    }

    private void InvokeLeave(PlayerInput _)
    {
        OnPlayerLeave?.Invoke(this);
    }
    
    
    
    
}
