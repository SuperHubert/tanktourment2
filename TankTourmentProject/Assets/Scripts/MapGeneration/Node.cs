using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapGeneration
{
    [Serializable]
    public class Node
    {
        /*
             Class that represent a the possibilities a Tile of our map has.
             It's used by the Wave Collapse Manager to calculate the possibilities of each tile.
             TileTypePossible is the list that get shrunk as the algorithm goes on.
         */
        
        public Vector2Int Position { get; private set; } //Maybe not needed (bonnus?)
        public PrefabData TileTypeSelected { get; private set; }
        [HideInInspector] public List<PrefabData> TileTypesPossibles { get; private set; }
        public bool IsCollapsed { get; private set; }
        
        public Color debugColor = Color.white;

        public List<Node> neighbors { get; private set; } = new List<Node>();
        
        public WorldTile Tile { get; private set; }

        public int Entropy
        {
            get
            {
                return TileTypesPossibles.Count(t => t.CanSpawn);
                // Function if we want to add weight to each neighbor
            }
        }


        public Node(Vector2Int position, List<PrefabData> tileTypesPossibles)
        {
            Position = position;
            TileTypesPossibles = tileTypesPossibles.ToList();
            TileTypeSelected = null;

            IsCollapsed = false;
        }

        public List<string> GetPossibleNeighborsDir(Enums.Direction direction)
        {
            var possibleNeighbors = new List<string>();
            string[] neighborTypes;
            
            foreach (var tileTypesPossible in TileTypesPossibles)
            {
                neighborTypes = tileTypesPossible.GetPrefabConnexion(direction);

                foreach (var neighborType in neighborTypes)
                {
                    if (!possibleNeighbors.Contains(neighborType) ) possibleNeighbors.Add(neighborType);
                }
            }

            return possibleNeighbors;
        }
        
        public void AutoSelectTileType()
        {
            var spawnables = TileTypesPossibles.Where(t => t.CanSpawn).ToList();
            
            if (spawnables.Count == 0)
            {
                /* Used as debug when the Wave function collapse can't find a corresponding tile. It shows the
                type of tile that could not be spawned because it was missing.
                */
                Debug.Log($"No spawnables: {Position}");
                
                WaveCollapseManager.Instance.DebugNeighbors(Position.x, Position.y);
            }
            
            TileTypeSelected = spawnables[Random.Range(0, spawnables.Count)];
            
            
            TileTypesPossibles.Clear();
            TileTypesPossibles.Add(TileTypeSelected);
            Collapse();
        }

        private void DebugNeighbors(Node neighbor, Enums.Direction direction)
        {
            /* Log neighbors of a node. Used for debug. */
            if (!neighbor.IsCollapsed)
                return;
            
            Debug.Log($"Neigbor {direction} is: {neighbor.TileTypeSelected.GetPrefabConnexionNoSplit(Enums.ReverseDirection(direction))}");
        }

        public void SelectTile(PrefabData selected)
        {
            /* Select and collapse a Node */
            TileTypeSelected = selected;
            TileTypesPossibles.Clear();
            TileTypesPossibles.Add(TileTypeSelected);
            Collapse();
        }
        
        private void Collapse()
        {
            IsCollapsed = true;
        }
        
        public void SetNeighbors(List<Node> nodes)
        {
            neighbors = nodes;
        }
        
        public bool GetPath(Node destination, out List<Node> path)
        { 
            /*
             Get the path from this node to the destination node
             It's an implementation of the BFS algorithm.
             Does not include the destination node in the path. 
             */
            var start = this;
            
            var frontier = new Queue<Node>();
            var cameFromDict = new Dictionary<Node, Node>();

            frontier.Enqueue(start);
            cameFromDict.Add(start,null);

            path = new List<Node>(); //maybe add {destination}
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();
                
                if (current == destination)
                {
                    current = destination;

                    while (current != start)
                    {
                        path.Add(current);
                        current = cameFromDict[current];
                    }

                    path.Remove(destination);
                    path.RemoveAll(n => n.IsCollapsed);
                    path.Reverse();
                    return true;
                }

                var adjacentTiles = current.neighbors;
                foreach (var next in adjacentTiles)
                {
                    if (!cameFromDict.ContainsKey(next))
                    {
                        frontier.Enqueue(next);
                        cameFromDict.Add(next,current);
                    }
                }
            }
            
            return false; 
        }
        
        public void SetTile(WorldTile tile)
        {
            Tile = tile;
        }
    }
}