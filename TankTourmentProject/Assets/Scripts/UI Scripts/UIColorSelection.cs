using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorSelection : MonoBehaviour
{
    [field:SerializeField] public Image Image { get; private set; }
    [SerializeField] private GameObject selectedGo;
    [Header("Debug")]
    [SerializeField] private int selectionCount = 0;
    
    public void SetColor(Color color)
    {
        Image.color = color;
        selectionCount = 0;
    }
    
    public void RefreshAppearance()
    {
        selectedGo.SetActive(selectionCount > 0);
    }

    public void AddSelection()
    {
        selectionCount++;
        RefreshAppearance();
    }

    public void RemoveSelection()
    {
        selectionCount--;
        if (selectionCount < 0) selectionCount = 0;
        RefreshAppearance();
    }
}
