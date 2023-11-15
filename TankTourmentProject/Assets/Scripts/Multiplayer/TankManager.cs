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
    
    private List<Vector3> availableSpawnPoints = new List<Vector3>();
    private List<Tank> tanks = new List<Tank>();
    
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
        
        var pos = NextAvailableSpawnPoint();
        pos.y += tankPrefab.SpawnHeight;
        
        tank.transform.position = pos;
        
        tank.RespawnValues();
    }
    
}
