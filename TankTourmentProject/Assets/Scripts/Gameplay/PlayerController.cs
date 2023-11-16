using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    [field: SerializeField] public TankController TankController { get; private set; }
    [field: SerializeField] public CameraController CameraController { get; private set; }
    public int Layer { get; private set; }

    private PointsManager.PointAmount pointAmount;
    private TankSelectionData tankSelectionData;

    public PointsManager.PointAmount PointAmount
    {
        get
        {
            return pointAmount ??= new PointsManager.PointAmount();
        }
        private set => pointAmount = value;
    }
    
    public TankSelectionData TankSelectionData
    {
        get
        {
            return tankSelectionData ??= new TankSelectionData();
        }
        private set => tankSelectionData = value;
    }

    public static event Action<PlayerController> OnPlayerJoin;
    public static event Action<PlayerController> OnPlayerLeave;

    public static void CleanupEvents()
    {
        OnPlayerJoin = null;
        OnPlayerLeave = null;
    }

    public void SetLayer(int layer)
    {
        CameraController.SetLayerVisible(Layer,false);
        Layer = layer;
        CameraController.SetLayerVisible(Layer,true);
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

public class TankSelectionData
{
    public int SelectedTankIndex { get; private set; }
    public event Action<int> OnSelectedTankIndexChanged; 
    public Color SelectedColor { get; private set; }
    public event Action<Color> OnSelectedColorChanged;
    public bool IsReady { get; private set; }
    public event Action<bool> OnReadyChanged;

    public void SetTankIndex(int index)
    {
        SelectedTankIndex = index;
        
        Debug.Log("Changed selected tank index to " + index);
        
        OnSelectedTankIndexChanged?.Invoke(index);
    }
    
    public void SetColor(Color color)
    {
        SelectedColor = color;
        OnSelectedColorChanged?.Invoke(SelectedColor);
    }
    
    public void SetReady(bool value)
    {
        IsReady = value;
        OnReadyChanged?.Invoke(value);
    }
}
