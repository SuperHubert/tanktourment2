using UnityEngine;

public class CameraController : MonoBehaviour
{
    [field: Header("Components")]
    [field:SerializeField] public Camera Cam { get; private set; }
    [field:SerializeField] public Camera OverlayCam { get; private set; }
    [SerializeField] private Transform fowParent;
    [SerializeField] private Transform fowPanelLeft;
    [SerializeField] private Transform fowPanelRight;
    [field:SerializeField] public Transform CamTransform { get; private set; }

    [Header("Settings")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed;
    private float speedMultiplier = 1f;
    private float Speed => speed * speedMultiplier;

    [Header("Debug")]
    [SerializeField] private Transform target;
    private bool hasTarget = false;
    
    [SerializeField] private float fowAngle;

    public void SetTarget(Transform tr)
    {
        target = tr;

        hasTarget = target != null;

        Cam.enabled = hasTarget;

        if(!hasTarget) return;
        
        CamTransform.position = target.position + offset; 
        
        CamTransform.LookAt(target);
        
        OverlayCam.gameObject.SetActive(false);
    }

    public void SetFow(float angle,float range)
    {
        var mod = angle * 0.5f;
        
        fowPanelRight.localRotation = Quaternion.Euler(0f,0f,-mod);
        fowPanelLeft.localRotation = Quaternion.Euler(0f,0f,mod);
    }
    
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
    
    private void Update()
    {
        MoveWithTarget();
    }

    [ContextMenu("Look At Target")]
    private void LookAtTarget()
    {
        if(!hasTarget) return;
        
        CamTransform.LookAt(target);
    }

    private void MoveWithTarget()
    {
        if(!hasTarget) return;
        
        var pos = CamTransform.position;

        var targetPos = target.position + offset; //TODO: offset this by the input direction if aiming
        
        CamTransform.position = Vector3.MoveTowards(pos, targetPos, Speed * Time.deltaTime);
    }

    public void SetLayerVisible(int layer, bool visible)
    {
        if(layer == 0) return;
        if (visible)
        {
            Cam.cullingMask |= (1 << layer);
            return;    
        }
        Cam.cullingMask &= ~(1 << layer);
    }

    public void SetCameraRect(Vector2 position, Vector2 size)
    {
        var rect = Cam.rect;
        rect.position = position;
        rect.size = size;
        Cam.rect = rect;
    }
}
