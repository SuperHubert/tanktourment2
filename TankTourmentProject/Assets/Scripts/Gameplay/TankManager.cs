using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TankManager : MonoBehaviour
{
    [SerializeField] private Tank tankPrefab;

    private List<Tank> tanks = new List<Tank>();

    public void Start()
    {
        PlayerController.OnPlayerSpawned += SpawnTank;
    }

    public void SpawnTank(PlayerController controller)
    {
        var tank = Instantiate(tankPrefab);
        
        tanks.Add(tank);
        
        controller.TankController.ConnectTank(tank);
        controller.CameraController.SetTarget(tank.transform);
    }
}
