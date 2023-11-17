using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class UIColorSelection : MonoBehaviour
{
    [field:SerializeField] public Image Image { get; private set; }
    [SerializeField] private GameObject selectedGo;
    [SerializeField] private GameObject overSelectedGo;

    private Color color;
    
    private static Dictionary<Color,int> colorCountDict = new ();
    public static bool AllUnique => colorCountDict.All(kvp => kvp.Value <= 1);
    public static event Action<Color> OnSelectionCountUpdated;
    
    public static void CleanupDict()
    {
        colorCountDict.Clear();
        OnSelectionCountUpdated = null;
    }

    public static bool IsColorAvailable(Color col)
    {
        if (!colorCountDict.ContainsKey(col)) return true;
        return colorCountDict[col] == 0;
    }

    public void SetColor(Color col)
    {
        color = col;
        Image.color = color;
        colorCountDict.TryAdd(color, 0);
    }
    
    public void RefreshAppearance(Color col)
    {
        if(col != color) return;
        
        Debug.Log("refreshing color");
        
        var count = colorCountDict[color];
        selectedGo.SetActive(count == 1);
        overSelectedGo.SetActive(count > 1);
    }

    public void AddSelection()
    {
        Debug.Log("color++");
        
        colorCountDict.TryAdd(color, 0);

        colorCountDict[color]++;
        
        OnSelectionCountUpdated?.Invoke(color);
    }

    public void RemoveSelection()
    {
        Debug.Log("color--");
        
        colorCountDict.TryAdd(color, 0);

        colorCountDict[color]--;
        if(colorCountDict[color] < 0) colorCountDict[color] = 0;
        
        OnSelectionCountUpdated?.Invoke(color);
    }
}
