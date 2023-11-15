using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Tank tankPrefab;
    
    [Header("Settings")]
    [SerializeField] private Vector3[] spawnPoints;
    [SerializeField] private float respawnDuration;
    [SerializeField] private float respawnCamSpeedMultiplier;
    
    private List<Vector3> availableSpawnPoints = new List<Vector3>();
    [SerializeField] private List<Tank> tanks = new List<Tank>();
    
    [SerializeField] private bool isRunning = false;

    public void SetRunning(bool value)
    {
        isRunning = value;
    }
    
    public void Update()
    {
        if(!isRunning) return;

        foreach (var tank0 in tanks)
        {
            foreach (var tank1 in tanks)
            {
                tank0.CheckVisible(tank1);
            }
        }
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

    public void SpawnTank(PlayerController controller)
    {
        var pos = NextAvailableSpawnPoint();
        pos.y += tankPrefab.SpawnHeight;

        var prefab = tankPrefab; //TODO, put it in arguments
        
        var tank = Instantiate(prefab,pos, Quaternion.identity);

        tank.SetLayer(controller.Layer);
        
        tank.gameObject.name = $"GameTank ({LayerMask.LayerToName(controller.Layer)})";
        
        tank.RespawnValues();
        
        tank.OnTankKilled += Killed;
        tank.OnTankRespawned += ResetCamSpeed;
        
        tank.OnLayerVisibleUpdated += controller.CameraController.SetLayerVisible;
        
        tanks.Add(tank);
        
        controller.TankController.ConnectTank(tank);
        controller.CameraController.SetTarget(tank.transform);

        return;
        
        void Killed(Tank killer)
        {
            controller.CameraController.SetSpeedMultiplier(respawnCamSpeedMultiplier);
            
            OnTankKilled(tank,killer);
        }

        void ResetCamSpeed()
        {
            controller.CameraController.SetSpeedMultiplier(1f);
        }
        
    }

    private void OnTankKilled(Tank tank, Tank killer)
    {
        Debug.Log($"{killer.name} killed {tank.name}");
        
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
