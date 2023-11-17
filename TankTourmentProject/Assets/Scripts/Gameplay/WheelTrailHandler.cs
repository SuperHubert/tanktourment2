using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class WheelTrailHandler : MonoBehaviour
{ 
    [Header("Component")]
    [SerializeField] TrailRenderer trailRenderer;
    
    [Header("Settings")]
    [SerializeField] public float tireSensitivity = 0.2f;

    private Tank connectedTank;
    private bool connected = false;
    
    public void ConnectToTank(Tank tank)
    {
        connectedTank = tank;
        
        trailRenderer.emitting = false;

        connected = true;
        
        tank.OnTankRespawned += RestartEmission;
    }
    
    public void StopEmission()
    {
        trailRenderer.emitting = false;
    }

    private void RestartEmission()
    {
        trailRenderer.emitting = true;
    }
    
    private void Update()
    {
        if(!connected) return;
        
        if(!connectedTank.IsAlive) return;
        
        trailRenderer.emitting = Vector3.Dot(connectedTank.Rb.velocity.normalized, transform.forward) < (1 - tireSensitivity);
    }
}
