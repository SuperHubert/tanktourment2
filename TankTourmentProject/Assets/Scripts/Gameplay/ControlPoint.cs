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

    [SerializeField] private Transform particleSystemParent;
    [SerializeField] private ParticleSystem particleSystem;
    
    private float maxProgress;
    private Color contestedColor;
    private Color previewColor;
    private Color indicatorColor;
    
    private bool active;
    
    private float lastProgress;
    private Color lastColor;
    
    private List<Tank> tanksOnControlPoint = new List<Tank>();
    
    public event Action<Tank> OnTankStay;
    public event Action<float,Color> OnProgressChanged;

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

        switch (tanksOnControlPoint.Count)
        {
            case 0:
                ShowProgress(lastProgress, lastColor);
                break;
            case 1:
                OnTankStay?.Invoke(tanksOnControlPoint[0]);
                break;
            default:
                ShowProgress(lastProgress, Color.black);
                break;
        }
    }

    public void ShowProgress(float amount, Color color)
    {
        if (Math.Abs(lastProgress - amount) > 0.05f) particleSystem.Stop();
        
        lastProgress = amount;
        lastColor = color;
        progressImage.fillAmount = amount;
        progressImage.color = color;
        
        particleSystemParent.rotation = Quaternion.Euler(new Vector3(0f, 360 * amount, 0f));
        if (particleSystem.isStopped) particleSystem.Play();
        
        var settings = particleSystem.main;
        settings.startColor = color;
        
        OnProgressChanged?.Invoke(amount,color);
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
            tank.SetVisibilityOverride(-1);
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
        
        tank.SetVisibilityOverride(360);
        
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

    private void RemoveTankFromList(Tank tank,Tank _ = null)
    {
        tank.SetVisibilityOverride(-1);
        
        tank.OnTankKilled -= RemoveTankFromList;
        tanksOnControlPoint.Remove(tank);
    }
    
}
