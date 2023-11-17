using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [field: Header("Components")]
    [field:SerializeField] public Camera Cam { get; private set; }
    [field:SerializeField] public Transform CamTransform { get; private set; }

    [Header("Settings")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed;
    [SerializeField] private Color fogColor = Color.black;
    private float speedMultiplier = 1f;
    private float Speed => speed * speedMultiplier;

    private Vector3 inputOffset;
    private Tank target;
    private bool hasTarget = false;
    
    private Transform fowRotationTarget;
    
    public void SetTarget(Tank tank)
    {
        target = tank;
        fowRotationTarget = tank.HeadTransform;

        hasTarget = target != null;

        Cam.enabled = hasTarget;

        if(!hasTarget) return;
        
        CamTransform.position = target.Position + offset; 
        
        CamTransform.LookAt(target.Position );
    }
    
    public void SetInputOffset(Vector3 input)
    {
        inputOffset = input.normalized;
    }
    
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
    
    private void FixedUpdate()
    {
        MoveWithTarget();
    }
    
    private void MoveWithTarget()
    {
        if(!hasTarget) return;
        
        var pos = CamTransform.position;

        //var inputDirectionOffset = inputOffset * inputOffsetMultiplier; //TODO - probably do something with this
        
        var targetPos = target.Position + offset; //TODO: offset this by the input direction if aiming
        
        CamTransform.position = Vector3.MoveTowards(pos, targetPos, Speed * Time.deltaTime);
    }
    
    public void SetCameraRect(Vector2 position, Vector2 size)
    {
        var rect = Cam.rect;
        rect.position = position;
        rect.size = size;
        Cam.rect = rect;
    }
}
