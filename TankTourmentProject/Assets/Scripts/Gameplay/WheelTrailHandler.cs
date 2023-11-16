using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class WheelTrailHandler : MonoBehaviour
{ 
    
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] private Rigidbody rb;
    public float tireSensityvity = 0.5f;
    private void Awake()
    {
        rb = GetComponentInParent<Rigidbody>();
        trailRenderer.emitting = false;
    }
    
    
    private void Update()
    {
        if (Vector3.Dot(rb.velocity.normalized, this.transform.forward) < (1 - tireSensityvity))
        {
            trailRenderer.emitting = true;
        }
        else
        {
            trailRenderer.emitting = false;
        }
    }
}
