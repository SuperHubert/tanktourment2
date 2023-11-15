using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Tank tankPrefab;
    
    [Header("Settings")]
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float respawnDuration;
    
    private List<Transform> availableSpawnPoints = new List<Transform>();
    private List<Tank> tanks = new List<Tank>();
    
    public void SpawnTanks(List<PlayerController> controllers)
    {
        foreach (var controller in controllers)
        {
            SpawnTank(controller);
        }
    }
    
    private Vector3 NextAvailableSpawnPoint()
    {
        if (availableSpawnPoints.Count == 0)
        {
            availableSpawnPoints.AddRange(spawnPoints);
        }
        
        var spawnPoint = availableSpawnPoints[0];
        availableSpawnPoints.RemoveAt(0);

        return spawnPoint.position;
    }

    public void SpawnTank(PlayerController controller)
    {
        var tank = Instantiate(tankPrefab,NextAvailableSpawnPoint(), Quaternion.identity);
        
        tank.RespawnValues();
        
        tank.OnTankKilled += Killed;
        
        tanks.Add(tank);
        
        controller.TankController.ConnectTank(tank);
        controller.CameraController.SetTarget(tank.transform);

        return;
        
        void Killed(Tank killer)
        {
            OnTankKilled(tank,killer);
        }
    }

    private void OnTankKilled(Tank tank, Tank killer)
    {
        Debug.Log($"{killer.name} killed {tank.name}");
        
        StartCoroutine(WaitRespawn(tank));
    }
    
    IEnumerator WaitRespawn(Tank tank)
    {
        yield return new WaitForSeconds(respawnDuration);
        
        tank.transform.position = NextAvailableSpawnPoint();
        
        tank.RespawnValues();
    }
    
}
