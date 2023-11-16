using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MapGeneration;
using UnityEngine;
using UnityEngine.InputSystem;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private TankManager tankManager;
    [SerializeField] private WaveCollapseManager waveCollapseManager;
    [SerializeField] private TankSelectionManager tankSelectionManager;
    [SerializeField] private PointsManager pointsManager;
    [SerializeField] private Camera mainCamera;

    [Header("Settings")]
    [SerializeField] private int layerPlayer0 = 10;
    [SerializeField] public int minPlayer = 2;
    [SerializeField] private float borderMultiplier = 1f;
    

    [SerializeField] private List<PlayerController> playerControllers = new List<PlayerController>();
    [SerializeField] private List<PlayerController> inactivePlayerControllers = new List<PlayerController>();
    private int ActivePlayerCount => playerControllers.Count - inactivePlayerControllers.Count;
    IEnumerable<PlayerController> ActivePlayers => playerControllers.Where(controller => !inactivePlayerControllers.Contains(controller));


    private bool isInGame = false;
    
    private void Start()
    {
        PlayerController.CleanupEvents();
        
        isInGame = false;
        
        PlayerController.OnPlayerJoin += AddPlayer;
        PlayerController.OnPlayerLeave += RemovePlayer;
        
        tankSelectionManager.OnPlayerReadyChanged += TryStartGame;
        
        tankSelectionManager.ShowColors(false);
        
        tankSelectionManager.SetAvailableTanks(tankManager.GetAvailableTankModels());
    }

    private void AddPlayer(PlayerController playerController)
    {
        playerController.gameObject.SetActive(true);
        
        playerControllers.Add(playerController);
        
        if (inactivePlayerControllers.Contains(playerController)) inactivePlayerControllers.Remove(playerController);
        
        OnPlayersUpdate();
    }
    
    private void RemovePlayer(PlayerController playerController)
    {
        playerController.gameObject.SetActive(false);
        
        inactivePlayerControllers.Add(playerController);

        OnPlayersUpdate();
    }

    public void OnPlayersUpdate()
    {
        tankSelectionManager.ShowColors(!isInGame && playerControllers.Count > 1);
        
        // TODO, propably not hardcode it

        var activePlayers = ActivePlayers.ToList();
        for (var index = 0; index < ActivePlayerCount; index++)
        {
            var controller = activePlayers[index];
            var rect = controller.CameraController.Cam.rect;
            var position = rect.position;
            var size = rect.size;

            switch (ActivePlayerCount)
            {
                case 2:
                    position.y = 0.25f;
                    break;
                case 3:
                    if(index == 2) position.x = 0.25f;
                    break;
                default:
                    break;
            }
            
            
            size *= borderMultiplier;
            position += (1 - borderMultiplier) * 0.5f * (Vector2.one - size);
            
            rect.position = position;
            rect.size = size;
            controller.CameraController.Cam.rect = rect;
        }


        for (int i = 0; i < playerControllers.Count; i++)
        {
            var controller = playerControllers[i];
            
            var layer = layerPlayer0 + i;
            
            controller.SetLayer(layer);
            
            var selection = tankSelectionManager.GetTankSelection(controller);

            selection.ConnectToPlayerController(controller, layer);
        }
    }

    private void TryStartGame(List<PlayerController> readyPlayers)
    {
        var activeAndReady = readyPlayers.Count(player => !inactivePlayerControllers.Contains(player));
        
        if(activeAndReady < minPlayer || activeAndReady < ActivePlayerCount) return;
        
        LaunchGame();
    }

    private void LaunchGame()
    {
        tankSelectionManager.OnPlayerReadyChanged -= TryStartGame;
        
        waveCollapseManager.OnMapGenerated += OnMapGenerated;
        
        tankSelectionManager.HideSelections();
        
        foreach (var controller in playerControllers)
        {
            controller.CameraController.Cam.enabled = false;
        }
        
        // create map
        
        waveCollapseManager.GenerateMap(out var generationData);
        
        // set spawn points
        tankManager.SetSpawnPoints(generationData.SpawnTilePositions);
        
        //move camera
        var worldCenter = generationData.WorldCenter;
        var worldSize = new Vector2(worldCenter.x*2,worldCenter.z*2);
        
        mainCamera.transform.position = generationData.WorldCenter;
        mainCamera.orthographicSize = Vector2.Distance(Vector2.zero, worldSize * 0.5f * 0.80f);
        
        pointsManager.SetPlayers(ActivePlayers);
        pointsManager.SetPoints(generationData.ControlTilePositions,generationData.Scale);
    }

    private void OnMapGenerated()
    {
        // spawn tanks
        mainCamera.cullingMask = 0;
        
        tankManager.SpawnTanks(playerControllers);
        
        tankManager.SetRunning(true);

        foreach (var controller in ActivePlayers)
        {
            controller.CameraController.Cam.enabled = true;
        }
    }

    
}
