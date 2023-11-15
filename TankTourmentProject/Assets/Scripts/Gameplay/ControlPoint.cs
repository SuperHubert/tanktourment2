using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ControlPoint : MonoBehaviour
{
    public event Action<Tank> OnTankStay;
    
    public void Activate()
    {
        gameObject.SetActive(true);
    }
    
    public void Deactivate()
    {
        gameObject.SetActive(false);
    }
    
    private void OnTriggerStay(Collider other)
    {
        var tank = other.gameObject.GetComponent<Tank>();
        if(tank == null) return;
        
        OnTankStay?.Invoke(tank);
    }
    
}
