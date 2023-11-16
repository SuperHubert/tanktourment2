using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class ControlPoint : MonoBehaviour
{
    [SerializeField] private Image progressImage;
    [SerializeField] private Image indicatorImage;
    private float maxProgress;
    private Color contestedColor;
    private Color previewColor;
    private Color indicatorColor;
    
    private bool active;
    
    private float lastProgress;
    private Color lastColor;
    
    private List<Tank> tanksOnControlPoint = new List<Tank>();
    
    public event Action<Tank> OnTankStay;

    public void SetValues(float max, Color previewCol, Color indicatorCol, Color contestedCol)
    {
        maxProgress = max;
        progressImage.fillAmount = 0;
        previewColor = previewCol;
        indicatorColor = indicatorCol;
        contestedColor = contestedCol;

        active = false;
    }
    
    private void Update()
    {
        if(!active) return;
        
        if(tanksOnControlPoint.Count == 1) OnTankStay?.Invoke(tanksOnControlPoint[0]);
        else ShowProgress(lastProgress, lastColor);
    }

    public void ShowProgress(float amount, Color color)
    {
        lastProgress = amount;
        lastColor = color;
        progressImage.fillAmount = amount;
        progressImage.color = color;
    }
    
    public void Activate()
    {
        active = true;
        
        indicatorImage.color = indicatorColor;
        
        Cleanup();
    }

    public void ShowPreview()
    {
        indicatorImage.color = previewColor;
    }
    
    public void Deactivate()
    {
        active = false;
        
        indicatorImage.color = Color.clear;
        
        Cleanup();
    }

    private void Cleanup()
    {
        foreach (var tank in tanksOnControlPoint)
        {
            tank.OnTankKilled -= RemoveTankFromList;
        }
        
        tanksOnControlPoint.Clear();
        
        progressImage.fillAmount = 0;
    }

    private void OnTriggerEnter(Collider other)
    {
        if(!active) return;
        
        var tank = other.gameObject.GetComponent<Tank>();
        if(tank == null) return;
        
        tanksOnControlPoint.Add(tank);
        
        tank.OnTankKilled += RemoveTankFromList;
    }

    private void OnTriggerExit(Collider other)
    {
        if(!active) return;
        
        var tank = other.gameObject.GetComponent<Tank>();
        if(tank == null) return;
        
        RemoveTankFromList(tank);
    }

    private void RemoveTankFromList(Tank tank)
    {
        tank.OnTankKilled -= RemoveTankFromList;
        tanksOnControlPoint.Remove(tank);
    }
    
}
