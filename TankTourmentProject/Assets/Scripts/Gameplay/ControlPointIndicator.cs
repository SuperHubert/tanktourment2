using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ControlPointIndicator : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Camera cam;

    [Header("Position Settings")]
    [SerializeField] private Image currentImage;
    [SerializeField] private Image currentTrImage;
    [SerializeField] private Image nextImage;
    [SerializeField] private int borderOffset;
    [SerializeField] private int borderViewRange;

    [Header("Score Settings")]
    [SerializeField] private GameObject scoreGo;
    [SerializeField] private Image ownImage;
    [SerializeField] private TextMeshProUGUI currentPercentText;
    [SerializeField] private TextMeshProUGUI totalPointsText;
    
    [Header("Debug")]
    [SerializeField] private ControlPoint currentControlPoint;
    [SerializeField] private bool hasCurrent;
    [SerializeField] private ControlPoint nextControlPoint;
    [SerializeField] private bool hasNext;

    private void Start()
    {
        currentTrImage.gameObject.SetActive(false);
        nextImage.gameObject.SetActive(false);
        scoreGo.SetActive(false);
    }

    public void SetIndicatorColors(Color current, Color next)
    {
        currentTrImage.color = current;
        nextImage.color = next;
    }
    
    public void SetOwnColor(Color color)
    {
        ownImage.color = color;
    }

    public void UpdateOwnPercent(float percent)
    {
        ownImage.fillAmount = percent;
        currentPercentText.text = $"{Mathf.FloorToInt(percent * 100)}";
    }
    
    public void UpdateOwnScore(int score)
    {
        totalPointsText.text = $"{score}";
    }
    
    public void SetControlPoints(ControlPoint current, ControlPoint next)
    {
        if (currentControlPoint != null) currentControlPoint.OnProgressChanged -= UpdateCurrentAppearance;
        
        currentControlPoint = current;
        nextControlPoint = next;
        
        hasCurrent = currentControlPoint != null;
        hasNext = nextControlPoint != null;

        currentTrImage.gameObject.SetActive(hasCurrent);
        nextImage.gameObject.SetActive(hasNext);
        scoreGo.SetActive(true);
        
        currentImage.fillAmount = 0;
        
        if (!hasCurrent) return;
        
        UpdateCurrentAppearance(0, Color.clear);
        currentControlPoint.OnProgressChanged += UpdateCurrentAppearance;
    }
    
    private void UpdateCurrentAppearance(float amount, Color color)
    {
        if(!hasCurrent) return;
        
        currentImage.fillAmount = amount;
        currentImage.color = color;
    }
    
    public void FixedUpdate()
    {
        UpdateCurrentPosition();
        UpdateNextPosition();
    }

    private void UpdateCurrentPosition()
    {
        if(!hasCurrent) return;
        
        UpdateImage(currentTrImage.transform,currentControlPoint.transform.position);
    }

    private void UpdateNextPosition()
    {
        if(!hasNext) return;
        
        UpdateImage(nextImage.transform,nextControlPoint.transform.position);
    }

    private void UpdateImage(Transform tr,Vector3 position)
    {
        position = cam.WorldToViewportPoint(position);

        var w = Screen.width;
        var h = Screen.height;
        
        position.x -= 0.5f;
        position.x *= w;
        position.y -= 0.5f;
        position.y *= h;
        position.z = 0f;

        var marginL = -(w / 2) + borderOffset;
        var marginR = (w / 2) - borderOffset;
        var marginT = (h / 2) - borderOffset;
        var marginB = -(h / 2) + borderOffset;

        var inFrame = (
            position.x > marginL - borderViewRange &&
            position.x < marginR + borderViewRange &&
            position.y > marginB - borderViewRange && 
            position.y < marginT + borderViewRange);
        
        tr.gameObject.SetActive(!inFrame);
        
        position.x = Mathf.Clamp(position.x, marginL, marginR);
        position.y = Mathf.Clamp(position.y , marginB, marginT);
        
        tr.localPosition = position;
    }
    
    
}
