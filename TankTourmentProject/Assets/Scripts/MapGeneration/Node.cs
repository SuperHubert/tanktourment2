using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration
{
    public class Node
    {
        public Vector2Int Position { get; private set; } //Maybe not needed (bonnus?)
        
        public Enums.TileType TileTypeSelected { get; private set; }
        
        public List<Enums.TileType> TileTypesPossibles { get; private set; }
        
        public bool IsCollapsed { get; private set; }

        public int Entropy => TileTypesPossibles.Count; // Function if we want to add weight to each neighbor
    
    
        public Node(Vector2Int position, List<Enums.TileType> tileTypesPossibles)
        {
            Position = position;
            TileTypesPossibles = new List<Enums.TileType>(tileTypesPossibles);
            TileTypeSelected = Enums.TileType.None;

            IsCollapsed = false;
        }

        public List<Enums.NeighborType> GetPossibleNeighborsDir(Enums.Direction direction)
        {
            List<Enums.NeighborType> possibleNeighbors = new List<Enums.NeighborType>();
            Enums.NeighborType neighborType;
            
            foreach (var tileTypesPossible in TileTypesPossibles)
            {
                neighborType = Enums.GetNeighborTypeByDirection(tileTypesPossible, direction);
                
                if (!possibleNeighbors.Contains(neighborType)) possibleNeighbors.Add(neighborType);
            }

            return possibleNeighbors;
        }
        
        public void AutoSelectTileType()
        {
            TileTypeSelected = TileTypesPossibles[Random.Range(0, TileTypesPossibles.Count)];
            Collapse();
        }
        
        private void Collapse()
        {
            IsCollapsed = true;
        }
    }
}