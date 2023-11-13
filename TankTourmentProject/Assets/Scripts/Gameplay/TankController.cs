using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput playerInput;
    
    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction shootAction;
    private InputAction mouseLookAction;
    
    [SerializeField] private Tank tank; //TODO Instantiate this with game manager
    
    private void Start()
    {
        moveAction = playerInput.actions["Move"];
        lookAction = playerInput.actions["Test"];
        shootAction = playerInput.actions["Shoot"];
        
        Debug.Log("lookAction: " + lookAction);
        
        
        ConnectTankInputs();
    }

    private void ConnectTankInputs()
    {
        moveAction.performed += HandleMovement;
        moveAction.canceled += HandleMovement;
        
        lookAction.performed += HandleHeadInputs;
        lookAction.canceled += HandleHeadInputs;
        
        shootAction.started += HandleShooting;
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
    
    private void HandleShooting(InputAction.CallbackContext context)
    {
        tank.Shoot();
    }
    
    
}
