using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public TankController TankController { get; private set; }
    [field: SerializeField] public CameraController CameraController { get; private set; }

    public static event Action<PlayerController> OnPlayerSpawned;

    public static void CleanupEvents()
    {
        OnPlayerSpawned = null;
    }
    
    private void Start()
    {
        OnPlayerSpawned?.Invoke(this);
    }
}
