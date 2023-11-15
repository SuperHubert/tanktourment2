using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIColorSelection : MonoBehaviour
{
    [field:SerializeField] public Image Image { get; private set; }

    public void SetColor(Color color)
    {
        Image.color = color;
    }
}
