using System.Collections;
using System.Collections.Generic;
using MapGeneration;
using UnityEngine;
using UnityEngine.UI;

public class WorldTile : MonoBehaviour
{
    /*
        Class of a GameObject that represent a Tile in the world.
     */
    [field: SerializeField] public Renderer Renderer { get; private set; }
    [SerializeField] private Transform spawnTransform;
    
    public Vector3 SpawnPosition => spawnTransform != null ? spawnTransform.position : transform.position;
}
