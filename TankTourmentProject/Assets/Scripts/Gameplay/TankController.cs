using UnityEngine;
using UnityEngine.InputSystem;

public class TankController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInput playerInput;
    
    [SerializeField] private Tank tank; //TODO Instantiate this with game manager
    
    private void Start()
    {
        ConnectTankInputs();
    }

    private void ConnectTankInputs()
    {
        playerInput.actions["Move"].performed += HandleMovement;
        playerInput.actions["Move"].canceled += HandleMovement;
        
        playerInput.actions["Shoot"].started += HandleShooting;
    }
    
    private void HandleMovement(InputAction.CallbackContext context)
    {
        var direction = context.ReadValue<Vector2>();
        tank.HandleInputs(direction);
    }
    
    private void HandleShooting(InputAction.CallbackContext context)
    {
        tank.Shoot();
    }
    
    
}
