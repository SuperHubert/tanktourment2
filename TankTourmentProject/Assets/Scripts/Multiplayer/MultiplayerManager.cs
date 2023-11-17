using System.Collections.Generic;
using System.Linq;
using MapGeneration;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class MultiplayerManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private PlayerInputManager playerInputManager;
    [SerializeField] private TankManager tankManager;
    [SerializeField] private WaveCollapseManager waveCollapseManager;
    [SerializeField] private TankSelectionManager tankSelectionManager;
    [SerializeField] private PointsManager pointsManager;
    [SerializeField] private UIManager uiManager;
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
    private LayerMask mainCameraMask;
    
    private void Start()
    {
        Setup();
    }

    private void Setup()
    {
        playerInputManager.EnableJoining();
        
        PlayerController.CleanupEvents();
        
        isInGame = false;
        
        PlayerController.OnPlayerJoin += AddPlayer;
        PlayerController.OnPlayerLeave += RemovePlayer;
        
        tankSelectionManager.OnPlayerReadyChanged += TryStartGame;
        
        tankSelectionManager.SetAvailableTanks(tankManager.GetAvailableTankModels());
    }

    private void AddPlayer(PlayerController playerController)
    {
        playerController.CameraController.Cam.enabled = true;
        
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
            
            controller.CameraController.SetCameraRect(position,size);
        }


        for (int i = 0; i < playerControllers.Count; i++)
        {
            var controller = playerControllers[i];
            
            var selection = tankSelectionManager.GetTankSelection(controller,i);

            selection.ConnectToPlayerController(controller);
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
        playerInputManager.DisableJoining();
        
        PlayerController.OnPlayerJoin -= AddPlayer;
        PlayerController.OnPlayerLeave -= RemovePlayer;
        
        tankSelectionManager.OnPlayerReadyChanged -= TryStartGame;
        
        waveCollapseManager.OnMapGenerated += OnMapGenerated;
        
        pointsManager.OnPlayerWin += OnPlayerWin;
        
        uiManager.CanGenerateMap += GeneratedMap;
        
        playerControllers[0].OnGameCDFinished += OnGameCDFinished;
        
        foreach (var controller in playerControllers)
        {
            controller.CountdownForGameStart();
        }
        
        return;
        
        void OnGameCDFinished()
        {
            playerControllers[0].OnGameCDFinished -= OnGameCDFinished;
            
            tankSelectionManager.HideSelections();
        
            tankSelectionManager.ExitSelection();
        
            foreach (var controller in playerControllers)
            {
                controller.CameraController.Cam.enabled = false;
                
                
            }
            
            uiManager.CountdownFirstTo(pointsManager.PointsToWin);
        }

        void GeneratedMap()
        {
            uiManager.CanGenerateMap -= GeneratedMap;
            // create map
        
            waveCollapseManager.GenerateMap(out var generationData);
        
            // set spawn points
            tankManager.SetSpawnPoints(generationData.SpawnTilePositions);
        
            //move camera
            var worldCenter = generationData.WorldCenter;
            var worldSize = new Vector2(worldCenter.x*2,worldCenter.z*2);
        
            mainCamera.transform.position = generationData.WorldCenter;
            mainCamera.orthographicSize = Vector2.Distance(Vector2.zero, worldSize * (0.5f * 0.80f));
        
            pointsManager.SetPoints(generationData.ControlTilePositions,generationData.Scale);
            pointsManager.SetPlayers(ActivePlayers);

            isInGame = true;
        }
    }

    private void OnMapGenerated()
    {
        // spawn tanks
        mainCameraMask = mainCamera.cullingMask;
        mainCamera.cullingMask = 0;
        
        tankManager.SpawnTanks(playerControllers);
        
        tankManager.SetRunning(true);

        foreach (var controller in ActivePlayers)
        {
            controller.CameraController.Cam.enabled = true;
        }
    }

    private void OnPlayerWin(PlayerController playerController)
    {
        pointsManager.OnPlayerWin -= OnPlayerWin;

        tankManager.SetRunning(false);

        foreach (var controller in playerControllers)
        {
            controller.CameraController.Cam.enabled = false;
        }
        
        Debug.Log($"{playerController} won the game!");

        mainCamera.cullingMask = mainCameraMask;
        
        uiManager.RestartGame += RestartGame;
        
        uiManager.ShowWinner(playerController);
    }
    
    public void RestartGame()
    {
        uiManager.RestartGame -= RestartGame;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
