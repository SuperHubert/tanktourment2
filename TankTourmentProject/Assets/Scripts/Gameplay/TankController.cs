using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput playerInput;

    private Camera inputCam;
    
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction pointerLookAction;
    
    private const string KeyboardControlScheme = "Keyboard & Mouse";
    private bool useMouse = false;
    private bool hasTank = false;
    
    [SerializeField] private Tank tank; //TODO Instantiate this with game manager
    [SerializeField] private Vector2 mousePos;

    
    private bool connected = false;
    
    private void Start()
    {
        //playerInput.neverAutoSwitchControlSchemes = true;
        
        inputCam = playerInput.camera;
        
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Look"];
        shootAction = playerInput.actions["Shoot"];
        pointerLookAction = playerInput.actions["LookMouse"];
        
        playerInput.controlsChangedEvent.AddListener(SwitchControlSchemes);
        
        SwitchControlSchemes(playerInput);
    }
    
    public void ConnectTank(Tank t)
    {
        tank = t;
        
        hasTank = tank != null;
        
        if(!hasTank) return;
        
        ConnectTankInputs();
    }

    private void SwitchControlSchemes(PlayerInput context)
    {
        Debug.Log(context.currentControlScheme);
        useMouse = context.currentControlScheme == KeyboardControlScheme;
    }

    private void Update()
    {
        HandleMouseHeadInputs();
    }

    private void ConnectTankInputs()
    {
        moveAction.performed += HandleMovement;
        moveAction.canceled += HandleMovement;
        
        lookAction.performed += HandleHeadInputs;
        lookAction.canceled += HandleHeadInputs;
        
        shootAction.started += HandleShooting;
        
        connected = true;
    }
    
    private void HandleMovement(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();
        tank.HandleMovementInputs(direction);
    }
    
    private void HandleHeadInputs(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();
        tank.HandleHeadInputs(direction);
    }

    private void HandleMouseHeadInputs()
    {
        if(!useMouse) return;
        
        if(!hasTank) return;
        
        mousePos = pointerLookAction.ReadValue<Vector2>();
        
        var dir = mousePos - (Vector2)inputCam.WorldToScreenPoint(tank.Position);
        
        tank.HandleHeadInputs(dir.normalized);
    }
    
    private void HandleShooting(InputAction.CallbackContext context)
    {
        tank.Shoot();
    }
    
    
}
