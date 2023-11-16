using UnityEngine;
using UnityEngine.UI;

public class UIColorSelection : MonoBehaviour
{
    [field:SerializeField] public Image Image { get; private set; }
    [SerializeField] private GameObject selectedGo;
    [SerializeField] private GameObject overSelectedGo;
    [Header("Debug")]
    [SerializeField] private int selectionCount = 0;
    public int SelectionCount => selectionCount;
    
    public void SetColor(Color color)
    {
        Image.color = color;
        selectionCount = 0;
    }
    
    public void RefreshAppearance()
    {
        selectedGo.SetActive(selectionCount == 1);
        overSelectedGo.SetActive(selectionCount > 1);
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
