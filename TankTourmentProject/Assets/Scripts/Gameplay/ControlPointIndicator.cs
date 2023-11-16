using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ControlPointIndicator : MonoBehaviour
{
    [SerializeField] private Camera cam;

    [SerializeField] private Image currentImage;
    [SerializeField] private Image nextImage;
    [SerializeField] private int borderOffset;

    //[Header("Debug")]
    private ControlPoint currentControlPoint;
    private bool hasCurrent;
    private ControlPoint nextControlPoint;
    private bool hasNext;
    
    public void SetControlPoints(ControlPoint current, ControlPoint next)
    {
        currentControlPoint = current;
        hasCurrent = currentControlPoint != null;
        nextControlPoint = next;
        hasNext = nextControlPoint != null;
    }

    public void FixedUpdate()
    {
        UpdateCurrent();
        UpdateNext();
    }

    private void UpdateCurrent()
    {
        if(!hasCurrent) return;
        
        UpdateImage(currentImage,currentControlPoint.transform.position);
    }

    private void UpdateNext()
    {
        if(!hasNext) return;
        
        UpdateImage(nextImage,nextControlPoint.transform.position);
    }

    private void UpdateImage(Image img,Vector3 position)
    {
        position = cam.WorldToViewportPoint(position);

        var w = Screen.width;
        var h = Screen.height;
        
        position.x -= 0.5f;
        position.x *= w;
        position.y -= 0.5f;
        position.y *= h;
        position.z = 0f;
        
        position.x = Mathf.Clamp(position.x, -(w/2)+borderOffset, (w/2) - borderOffset);
        position.y = Mathf.Clamp(position.y , -(h/2)+borderOffset, (h/2) - borderOffset);
        
        img.transform.localPosition = position;
    }
    
    
}
