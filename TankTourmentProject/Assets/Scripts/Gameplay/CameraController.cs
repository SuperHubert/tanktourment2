using System;
using UnityEngine;
using UnityEngine.UI;

public class CameraController : MonoBehaviour
{
    [field: Header("Components")]
    [field:SerializeField] public Camera Cam { get; private set; }
    [field: Space]
    [field:SerializeField] public Camera OverlayCam { get; private set; }
    [SerializeField] private Transform fowParentRotator;
    [SerializeField] private Image fowMask;
    private Transform FowTr => fowMask.transform;
    [SerializeField] private RawImage visibleRender;
    private Transform RenderTr => visibleRender.transform;
    [SerializeField] private Image fowFogImage;
    private Transform FowImageTr => fowFogImage.transform;
    
    private RenderTexture visibleRenderTexture;
    [field:SerializeField] public Transform CamTransform { get; private set; }

    [Header("Settings")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed;
    [SerializeField] private Color fogColor = Color.black;
    private float speedMultiplier = 1f;
    private float Speed => speed * speedMultiplier;

    [Header("Debug")]
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

        var w = Screen.width;
        var h = Screen.height;
        
        visibleRenderTexture = new RenderTexture(w,h,16,RenderTextureFormat.ARGB32);
        RenderTr.localScale = Vector3.one * h / w;
        visibleRenderTexture.Create();
        
        visibleRender.texture = visibleRenderTexture;
        
        OverlayCam.targetTexture = visibleRenderTexture;
        
        FowTr.rotation = Quaternion.identity;
        RenderTr.rotation = Quaternion.identity;
        fowParentRotator.rotation = Quaternion.identity;
        
        OverlayCam.enabled = true;
        
        fowFogImage.color = fogColor;
    }
    
    public void SetSpeedMultiplier(float multiplier)
    {
        speedMultiplier = multiplier;
    }
    
    private void Update()
    {
        if(!hasTarget) return;
        
        SetFowAngle(target.MaxVisibilityAngle);
    }

    private void FixedUpdate()
    {
        MoveWithTarget();
        
        UpdateFowPosition();
    }

    private void UpdateFowPosition()
    {
        if(!hasTarget) return;
        
        var pos = OverlayCam.WorldToViewportPoint(fowRotationTarget.position);
        pos.x -= 0.5f;
        pos.x *= 100;
        pos.y -= 0.5f;
        pos.y *= (100*(9/16f));
        pos.z = 0f;

        SetFowPosition(pos);
    }
    
    public void SetFowPosition(Vector3 position)
    {
        if(!hasTarget) return;

        var targetRot = Quaternion.Euler(0f, 0f, -fowRotationTarget.rotation.eulerAngles.y);
        
        fowParentRotator.localPosition = position;
        fowParentRotator.localRotation = targetRot;
    }
    
    public void SetFowAngle(float angle)
    {
        angle /= 360;

        fowMask.fillAmount = angle;
        
        FowTr.localRotation = Quaternion.Euler(0f,0f,180*angle);
        
        RenderTr.position = FowImageTr.position;
        RenderTr.rotation = FowImageTr.rotation;
    }
    
    
    private void MoveWithTarget()
    {
        if(!hasTarget) return;
        
        var pos = CamTransform.position;

        var targetPos = target.Position  + offset; //TODO: offset this by the input direction if aiming
        
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
