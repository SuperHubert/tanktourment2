using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class TankSelectionManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private UIColorSelection colorSelectionPrefab;
    [SerializeField] private Transform colorSelectionLayout;

    [SerializeField] private TankSelection tankSelectionPrefab;
    
    [Header("Settings")]
    [SerializeField] private List<Color> colors = new List<Color>();
    public List<Color> Colors => colors; 

    [SerializeField] private List<Tank> tanks = new List<Tank>();
    
    private List<UIColorSelection> colorSelections = new List<UIColorSelection>();
    
    private Dictionary<PlayerController,TankSelection> tankSelections = new Dictionary<PlayerController, TankSelection>();

    public event Action<List<PlayerController>> OnPlayerReadyChanged;
    
    private void Start()
    {
        foreach (var color in colors)
        {
            var colorSelection = Instantiate(colorSelectionPrefab, colorSelectionLayout);

            colorSelection.SetColor(color);
            
            colorSelections.Add(colorSelection);
        }
    }
    
    public void ShowColors(bool value)
    {
        colorSelectionLayout.gameObject.SetActive(value);
    }

    public void HideSelections()
    {
        ShowColors(false);
        
        foreach (var selection in tankSelections.Values)
        {
            selection.gameObject.SetActive(false);
        }
    }
    
    public TankSelection GetTankSelection(PlayerController playerController)
    {
        if (tankSelections.TryGetValue(playerController, out var selection)) return selection;

        selection = Instantiate(tankSelectionPrefab);
        
        tankSelections.Add(playerController, selection);
        
        selection.SetTanks(tanks);
        
        selection.OnReadyChanged += TryStartGame;

        selection.OnColorChanged += playerController.SetColor;
        
        var color = colors[Random.Range(0, colors.Count)]; // TODO - remove when color selection is added
        
        selection.ChangeColor(color);
        
        return selection;
    }

    private void TryStartGame(bool _)
    {
        var readyPlayers = tankSelections.Where(kvp => kvp.Value.IsReady).Select(kvp => kvp.Key).ToList();
        
        Debug.Log($"Invoking {readyPlayers.Count} ready");
        
        OnPlayerReadyChanged?.Invoke(readyPlayers);
    }

    public void ExitSelection()
    {
        foreach (var selections in tankSelections.Values)
        {
            selections.DisconnectInputs();
        }
    }
}
