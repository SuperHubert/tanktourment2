using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [field: SerializeField] public PlayerInput PlayerInput { get; private set; }
    [field: SerializeField] public TankController TankController { get; private set; }
    [field: SerializeField] public CameraController CameraController { get; private set; }
    [field: SerializeField] public ControlPointIndicator ControlPointIndicator { get; private set; }
    public int Layer { get; private set; }

    private PointsManager.PointAmount pointAmount;
    private TankSelectionData tankSelectionData;
    
    public event Action OnGameCDFinished;
    [SerializeField] private TextMeshProUGUI countDownText;
    
    public void CountdownForGameStart()
    {
        countDownText.gameObject.SetActive(true);
        StartCoroutine(CountdownForGameStart(3));
        
        return;
        
        IEnumerator CountdownForGameStart(int seconds)
        {
            for (int i = seconds; i > 0; i--)
            {
                countDownText.text = i.ToString();
                yield return new WaitForSeconds(1f);
            }
            OnGameCDFinished?.Invoke();
            countDownText.gameObject.SetActive(false);
        }
    }

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
        SoundManager.instance.PlaySound(SoundManager.instance.validateEffect);
        OnPlayerJoin?.Invoke(this);
    }

    private void InvokeLeave(PlayerInput _)
    {
        SoundManager.instance.PlaySound(SoundManager.instance.cancelEffect);
        OnPlayerLeave?.Invoke(this);
    }
}

public class TankSelectionData
{
    public Tank SelectedTank{ get; private set; }
    public event Action<Tank> OnSelectedTankChanged; 
    public Color SelectedColor { get; private set; }
    public event Action<Color> OnSelectedColorChanged;
    public bool IsReady { get; private set; }
    public event Action<bool> OnReadyChanged;

    public void SetTankIndex(Tank model)
    {
        SelectedTank = model;
        OnSelectedTankChanged?.Invoke(SelectedTank);
    }
    
    public void SetColor(Color color)
    {
        //SoundManager.instance.PlaySound(SoundManager.instance.validateEffect);
        SelectedColor = color;
        OnSelectedColorChanged?.Invoke(SelectedColor);
    }
    
    public void SetReady(bool value)
    {
        SoundManager.instance.PlaySound(SoundManager.instance.validateEffect);
        IsReady = value;
        OnReadyChanged?.Invoke(value);
    }
}
