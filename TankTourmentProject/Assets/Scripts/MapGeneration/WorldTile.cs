using System.Collections;
using System.Collections.Generic;
using MapGeneration;
using UnityEngine;

public class WorldTile : MonoBehaviour
{
    [field: SerializeField] public Renderer Renderer { get; private set; }
    [SerializeField] private List<Node> neighbors = new List<Node>();
    private Node node;

    public Node Node
    {
        get => node;
        set
        {
            node = value;
            neighbors = node.neighbors;
        }
        
    }

}
