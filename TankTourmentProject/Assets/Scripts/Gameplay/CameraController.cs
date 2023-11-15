using System;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [field: Header("Components")]
    [field:SerializeField] public Camera Cam { get; private set; }
    [field:SerializeField] public Transform CamTransform { get; private set; }

    [Header("Settings")]
    [SerializeField] private Vector3 offset;
    [SerializeField] private float speed;

    [Header("Debug")]
    [SerializeField] private Transform target;
    private bool hasTarget = false;

    public void SetTarget(Transform tr)
    {
        target = tr;

        hasTarget = target != null;


        Cam.enabled = hasTarget;

        if(!hasTarget) return;
        
        CamTransform.position = target.position + offset; 
        
        CamTransform.LookAt(target);
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
        
        CamTransform.position = Vector3.MoveTowards(pos, targetPos, speed * Time.deltaTime);
    }
}
