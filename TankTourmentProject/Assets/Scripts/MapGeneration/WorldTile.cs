using System.Collections;
using System.Collections.Generic;
using MapGeneration;
using UnityEngine;
using UnityEngine.UI;

public class WorldTile : MonoBehaviour
{
    [field: SerializeField] public Renderer Renderer { get; private set; }
    [SerializeField] private Transform spawnTransform;
    
    public Vector3 SpawnPosition => spawnTransform != null ? spawnTransform.position : transform.position;
}
