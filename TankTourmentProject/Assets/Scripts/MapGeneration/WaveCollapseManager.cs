using System.Collections.Generic;
using UnityEngine;

namespace MapGeneration
{
    public class WaveCollapseManager : MonoBehaviour
    {
        [SerializeField] private int width;
        [SerializeField] private int height;
        
        private Node[,] nodes;
    
        // Start is called before the first frame update
        void Start()
        {
            nodes = new Node[width, height];
        }

        // Update is called once per frame
        void Update()
        {
        
        }
    
        private void CollapseWave()
        {
            
            // Init all Nodes
            InitNodes();
        
            // Set Fix nodes
            
            //
            while (!IsFullyCollapsed())
            {
                IterateWaveCollapse();
            }
        }
    
        private void InitNodes()
        {
            nodes = new Node[width, height];
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    nodes[i, j] = new Node(new Vector2Int(i, j), Enums.AllTileTypes);
                }
            }
        }
    
        private bool IsFullyCollapsed()
        {
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (!nodes[i, j].IsCollapsed)
                        return false;
                }
            }
            return true;
        }
    
        private void IterateWaveCollapse()
        {
            Node nodeToCollapse = GetMinEntropyNode();
            CollapseNode(nodeToCollapse);
            
            PropagateWave(nodeToCollapse);
        }

        private Node GetMinEntropyNode()
        {
            List<Node> possiblesNodes = new List<Node>();
            int minEntropy = -1;
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    if (!nodes[i, j].IsCollapsed)
                    {
                        if (nodes[i, j].Entropy < minEntropy)
                        {
                            possiblesNodes.Clear();
                            possiblesNodes.Add(nodes[i, j]);
                            minEntropy = nodes[i, j].Entropy;
                        }
                        else if (nodes[i, j].Entropy == minEntropy)
                        {
                            possiblesNodes.Add(nodes[i, j]);
                        }
                    }
                }
            }
            
            return possiblesNodes[Random.Range(0, possiblesNodes.Count)];
        }
        
        private void CollapseNode(Node node)
        {
            node.AutoSelectTileType();
        }
        
        private void PropagateWave(Node node)
        {
            List<Node> nodeToUpdate = new List<Node>();
            Node nodeUpdating;
            nodeToUpdate.Add(node);

            while (nodeToUpdate.Count > 0)
            {
                nodeUpdating = nodeToUpdate[^1];
                nodeToUpdate.RemoveAt(nodeToUpdate.Count - 1);
                
                // Update Top
                UpdateNode(nodeUpdating, Enums.Direction.Top, ref nodeToUpdate);

                // Update Bottom
                UpdateNode(nodeUpdating, Enums.Direction.Bottom, ref nodeToUpdate);

                // Update Left
                UpdateNode(nodeUpdating, Enums.Direction.Left, ref nodeToUpdate);

                // Update Right
                UpdateNode(nodeUpdating, Enums.Direction.Right, ref nodeToUpdate);
            }
        }
        
        private void UpdateNode(Node nodeOrigin, Enums.Direction direction, ref List<Node> nodesToUpdate)
        {
            int x = nodeOrigin.Position.x;
            int y = nodeOrigin.Position.y;

            switch (direction)
            {
                case Enums.Direction.Top:
                    y++;
                    break;
                case Enums.Direction.Bottom:
                    y--;
                    break;
                case Enums.Direction.Left:
                    x--;
                    break;
                case Enums.Direction.Right:
                    x++;
                    break;
            }
            
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;
            
            Node nodeToUpdate = nodes[x, y];
            
            if (nodeToUpdate.IsCollapsed)
                return;
            
            
            // Possible neighbors at direction
            List<Enums.NeighborType> possibleNeighbors = nodeToUpdate.GetPossibleNeighborsDir(direction);
            
            if (possibleNeighbors.Count == 0)
                return;
            
            foreach (var tileTypesPossible in nodeOrigin.TileTypesPossibles)
            {
                var neighborType = Enums.GetNeighborTypeByDirection(tileTypesPossible, direction);
                
                if (!possibleNeighbors.Contains(neighborType))
                {
                    // Remove tile from possible tiles
                    nodeOrigin.TileTypesPossibles.Remove(tileTypesPossible);
                    
                    if (!nodesToUpdate.Contains(nodeToUpdate))
                        nodesToUpdate.Add(nodeToUpdate);
                }
            }
        }
        
        
    }
}
