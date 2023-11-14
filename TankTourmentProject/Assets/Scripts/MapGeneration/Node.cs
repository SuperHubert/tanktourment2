using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapGeneration
{
    public class Node
    {
        public Vector2Int Position { get; private set; } //Maybe not needed (bonnus?)
        public PrefabData TileTypeSelected { get; private set; }
        public List<PrefabData> TileTypesPossibles { get; private set; }
        public bool IsCollapsed { get; private set; }
        
        public Color debugColor = Color.white;

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
    }
}