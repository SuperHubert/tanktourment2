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

    [Header("Settings")]
    [SerializeField] private int layerPlayer0 = 10;
    [SerializeField] public int minPlayer = 2;

    [SerializeField] private List<PlayerController> playerControllers = new List<PlayerController>();
    [SerializeField] private List<PlayerController> inactivePlayerControllers = new List<PlayerController>();


    private bool isInGame = false;
    
    private void Start()
    {
        PlayerController.CleanupEvents();
        
        isInGame = false;
        
        PlayerController.OnPlayerJoin += AddPlayer;
        PlayerController.OnPlayerLeave += RemovePlayer;
        
        tankSelectionManager.OnPlayerReadyChanged += TryStartGame;
        
        tankSelectionManager.ShowColors(false);
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
        
        Debug.Log($"Active and ready : {activeAndReady}, min : {minPlayer}, total active : {playerControllers.Count-inactivePlayerControllers.Count}");
        
        if(activeAndReady < minPlayer || activeAndReady < (playerControllers.Count-inactivePlayerControllers.Count)) return;
        
        LaunchGame();
    }

    private void LaunchGame()
    {
        tankSelectionManager.OnPlayerReadyChanged -= TryStartGame;
        
        tankSelectionManager.ShowColors(false);
        
        // create map
        
        waveCollapseManager.GenerateMap(out var spawnTiles);
        
        // set spawn points
        tankManager.SetSpawnPoints(spawnTiles);
        
        // spawn tanks
        tankManager.SpawnTanks(playerControllers);
    }

    
}
