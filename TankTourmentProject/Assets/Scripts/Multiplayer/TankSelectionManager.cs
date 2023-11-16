using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;
using DG.Tweening;
using UnityEngine.UI;

public class TankSelectionManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private UIColorSelection colorSelectionPrefab;
    [SerializeField] private GridLayoutGroup colorSelectionLayout;

    [SerializeField] private TankSelection tankSelectionPrefab;
    
    [Header("Settings")]
    [SerializeField] private List<Color> colors = new List<Color>();
    [SerializeField] private float colorMoveCooldown = 0.5f;
    [SerializeField] private int columns = 4;
    public List<Color> Colors => colors; 

    [SerializeField,ReadOnly] private Tank[] tanks;
    [SerializeField] private float tankOffset = 2f;
    [SerializeField] private float tankRotationSpeed = 10f;
    [SerializeField] private float tankMoveDuration = 1f;
    
    private List<UIColorSelection> colorSelections = new List<UIColorSelection>();
    
    private Dictionary<PlayerController,TankSelection> tankSelections = new Dictionary<PlayerController, TankSelection>();

    public event Action<List<PlayerController>> OnPlayerReadyChanged;
    
    private void Start()
    {
        foreach (var color in colors)
        {
            var colorSelection = Instantiate(colorSelectionPrefab, colorSelectionLayout.transform);

            colorSelection.SetColor(color);
            
            colorSelection.RefreshAppearance();
            
            colorSelections.Add(colorSelection);
            
        }
        
        colorSelectionLayout.constraintCount = columns;
    }

    public void SetAvailableTanks(Tank[] availableTanks)
    {
        tanks = availableTanks;
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
        
        selection.SetTanks(tanks,tankOffset);
        selection.SetColorCooldown(colorMoveCooldown);

        var tankIndex = 0;
        var colorIndex = 0;
        
        playerController.TankSelectionData.OnReadyChanged += TryStartGame;
        playerController.TankSelectionData.OnSelectedColorChanged += selection.ChangeColor; // TODO same for tank stats
        
        selection.OnTankChanged += ChangeTankIndex;
        selection.OnColorChanged += ChangeColorIndex;
        
        ChangeTankIndex(0);
        ChangeColor(colorIndex,false);
        
        return selection;

        void ChangeTankIndex(int change)
        {
            var index = tankIndex + change;
            
            if (index < 0) index = tanks.Length - 1;
            else if (index >= tanks.Length) index = 0;

            tankIndex = index;
            
            var tank = tanks[index];
            
            playerController.TankSelectionData.SetTankIndex(tank);
            
            var dir = selection.TankTr.right * -1f * tankOffset * index;
            var dest = selection.TankTrOrigin + dir;
            
            selection.TankTr.DOMove(dest,tankMoveDuration);
        }

        void ChangeColorIndex(Vector2 dir)
        {
            var x = dir.x;
            var y = dir.y;

            var current = colorIndex;
            
            if (x > 0) current++;
            else if (x < 0) current--;
            else if (y > 0) current -= columns;
            else if (y < 0) current += columns;

            if(current < 0) current += colors.Count;
            
            current %= colors.Count;
            
            ChangeColor(current,true);
        }

        void ChangeColor(int index,bool removeSelection)
        {
            if(removeSelection) colorSelections[colorIndex].RemoveSelection();
            colorIndex = index;
            colorSelections[colorIndex].AddSelection();
            
            playerController.TankSelectionData.SetColor(colors[colorIndex]);
        }
    }

    private void TryStartGame(bool _)
    {
        var readyPlayers = tankSelections.Keys.Where(p => p.TankSelectionData.IsReady).ToList();
        
        Debug.Log($"Invoking {readyPlayers.Count} ready");
        
        OnPlayerReadyChanged?.Invoke(readyPlayers);
    }

    public void ExitSelection()
    {
        foreach (var selections in tankSelections.Values)
        {
            selections.CleanupEvents();
            selections.DisconnectInputs();
        }
    }
}
