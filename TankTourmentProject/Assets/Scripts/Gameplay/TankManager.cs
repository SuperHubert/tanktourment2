using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [SerializeField] private Tank tankPrefab;
    
    [SerializeField] private Transform[] spawnPoints;
    private List<Transform> availableSpawnPoints = new List<Transform>();

    private List<Tank> tanks = new List<Tank>();

    public void Start()
    {
        PlayerController.OnPlayerSpawned += SpawnTank;
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
        
        tanks.Add(tank);
        
        controller.TankController.ConnectTank(tank);
        controller.CameraController.SetTarget(tank.transform);
    }
}
