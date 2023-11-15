using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPoint : MonoBehaviour
{
    [SerializeField] private Image image;
    
    public event Action<Tank> OnTankStay;
    
    public void ShowProgress(float amount, Color color)
    {
        image.fillAmount = amount;
        image.color = color;
    }
    
    public void Activate()
    {
        gameObject.SetActive(true);
        
        image.fillAmount = 0;
    }

    public void ShowPreview()
    {
        
    }
    
    public void Deactivate()
    {
        gameObject.SetActive(false);
        
        image.fillAmount = 0;
    }
    
    private void OnTriggerStay(Collider other)
    {
        var tank = other.gameObject.GetComponent<Tank>();
        if(tank == null) return;
        
        OnTankStay?.Invoke(tank);
    }
    
}
