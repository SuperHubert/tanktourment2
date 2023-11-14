using System;
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
        
        tank.transform.SetParent(null);
        
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
        if(tank == null) return;
        
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
        // todo - don't do this if controller inputscheme
        if(!useMouse) return;
        
        mousePos = pointerLookAction.ReadValue<Vector2>();
        var dir = mousePos - (Vector2)inputCam.WorldToScreenPoint(tank.transform.position);
        tank.HandleHeadInputs(dir.normalized);
    }
    
    private void HandleShooting(InputAction.CallbackContext context)
    {
        tank.Shoot();
    }
    
    
}
