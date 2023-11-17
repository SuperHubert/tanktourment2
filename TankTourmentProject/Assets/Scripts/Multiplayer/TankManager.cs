using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Tank tankPrefab;

    [Header("Settings")]
    [SerializeField] private List<Tank> availableTanksModels = new List<Tank>();
    [SerializeField] private Vector3[] spawnPoints;
    [SerializeField] private float respawnDuration;
    [SerializeField] private float respawnCamSpeedMultiplier;
    [SerializeField] private float minSpawnDistance = 5f;
    
    private List<Vector3> availableSpawnPoints = new List<Vector3>();
    [SerializeField] private List<Tank> tanks = new List<Tank>();
    
    [SerializeField] private bool isRunning = false;

    public void SetRunning(bool value)
    {
        isRunning = value;
    }
    
    public void SpawnTanks(List<PlayerController> controllers)
    {
        foreach (var controller in controllers)
        {
            SpawnTank(controller);
        }
    }

    public void SetSpawnPoints(List<Vector3> transforms)
    {
        spawnPoints = transforms.ToArray();
    }
    
    public Tank[] GetAvailableTankModels()
    {
        return availableTanksModels.ToArray();
    }
    
    private Vector3 NextAvailableSpawnPoint()
    {
        if (availableSpawnPoints.Count == 0)
        {
            availableSpawnPoints.AddRange(spawnPoints);
        }
        
        var spawnPoint = availableSpawnPoints[0];
        availableSpawnPoints.RemoveAt(0);
        
        return spawnPoint;
    }

    private void SpawnTank(PlayerController controller)
    {
        var pos = NextAvailableSpawnPoint();

        var validPos = false;
        while (!validPos)
        {
            validPos = true;
            foreach (var other in tanks)
            {
                if (Vector3.Distance(other.transform.position, pos) < minSpawnDistance)
                {
                    validPos = false;
                    pos = NextAvailableSpawnPoint();
                    break;
                }
            }
        }
        
        pos.y += tankPrefab.SpawnHeight;
        
        var data = controller.TankSelectionData;

        var prefab = data.SelectedTank; //TODO, put it in arguments
        
        var tank = Instantiate(prefab,pos, Quaternion.identity);

        tank.SetLayer(controller.Layer);
        tank.SetColor(controller.TankSelectionData.SelectedColor);
        
        tank.gameObject.name = $"GameTank ({LayerMask.LayerToName(controller.Layer)})";
        
        tank.RespawnValues();
        
        tank.OnTankKilled += Killed;
        tank.OnTankRespawned += ResetCamSpeed;
        
        tank.SetPointAmount(controller.PointAmount);
        
        tanks.Add(tank);
        
        controller.TankController.ConnectTank(tank);
        controller.CameraController.SetTarget(tank);

        return;
        
        void Killed(Tank killed,Tank killer)
        {
            controller.CameraController.SetSpeedMultiplier(respawnCamSpeedMultiplier);
            
            OnTankKilled(killed,killer);
        }

        void ResetCamSpeed()
        {
            controller.CameraController.SetSpeedMultiplier(1f);
        }
        
    }

    private void OnTankKilled(Tank tank, Tank killer)
    {
        var pos = NextAvailableSpawnPoint();
        pos.y += tankPrefab.SpawnHeight;
        
        tank.transform.position = pos;
        
        StartCoroutine(WaitRespawn());
        
        return;
        
        IEnumerator WaitRespawn()
        {
            yield return new WaitForSeconds(respawnDuration);
        
            tank.RespawnValues();
        }
    }
    
    
    
}
