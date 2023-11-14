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
        public Vector2Int Position { get; private set; } //Maybe not needed (bonnus?)
        public PrefabData TileTypeSelected { get; private set; }
        [HideInInspector] public List<PrefabData> TileTypesPossibles { get; private set; }
        public bool IsCollapsed { get; private set; }
        
        public Color debugColor = Color.white;

        public List<Node> neighbors { get; private set; } = new List<Node>();

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
            TileTypeSelected = spawnables[Random.Range(0, spawnables.Count)];
            
            
            TileTypesPossibles.Clear();
            TileTypesPossibles.Add(TileTypeSelected);
            Collapse();
        }

        public void SelectTile(PrefabData selected)
        {
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
        
        // A* pathfinding, nvm its BFS xd
        // https://www.redblobgames.com/pathfinding/a-star/introduction.html
        public bool GetPath(Node destination, out List<Node> path)
        {
            var start = this;
            
            var frontier = new Queue<Node>();
            var cameFromDict = new Dictionary<Node, Node>();

            frontier.Enqueue(start);
            cameFromDict.Add(start,null);

            path = new List<Node>(); //maybe add {destination}
            while (frontier.Count > 0)
            {
                var current = frontier.Dequeue();

                //current.debugColor = Color.black;
                
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
    }
}