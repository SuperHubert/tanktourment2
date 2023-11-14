using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace MapGeneration
{
    public class Node
    {
        public Vector2Int Position { get; private set; } //Maybe not needed (bonnus?)
        public string TileTypeSelected { get; private set; }
        public List<string> TileTypesPossibles { get; private set; }
        public bool IsCollapsed { get; private set; }

        public int Entropy => TileTypesPossibles.Count; // Function if we want to add weight to each neighbor
        
        
        public Node(Vector2Int position, List<string> tileTypesPossibles)
        {
            Position = position;
            TileTypesPossibles = tileTypesPossibles.ToList();
            TileTypeSelected = "";

            IsCollapsed = false;
        }

        public List<string> GetPossibleNeighborsDir(Enums.Direction direction)
        {
            List<string> possibleNeighbors = new List<string>();
            string[] neighborTypes;
            
            foreach (var tileTypesPossible in TileTypesPossibles)
            {
                neighborTypes = WaveCollapseManager.Instance.GetPrefabConnexionByName(tileTypesPossible, direction);

                foreach (var neighborType in neighborTypes)
                {
                    if (!possibleNeighbors.Contains(neighborType) ) possibleNeighbors.Add(neighborType);
                }
            }

            return possibleNeighbors;
        }
        
        public void AutoSelectTileType()
        {
            TileTypeSelected = TileTypesPossibles[Random.Range(0, TileTypesPossibles.Count)];
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