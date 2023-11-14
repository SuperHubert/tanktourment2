using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace MapGeneration
{
    public class WaveCollapseManager : MonoBehaviour
    {
        
        [field:SerializeField] private List<PrefabData> tilePrefabs;
        
        [SerializeField] private int width;
        [SerializeField] private int height;
        
        private Node[,] nodes;
    
        // Start is called before the first frame update
        void Start()
        {
            nodes = new Node[width, height];
            
            CollapseWave();
            //GenerateVisualMap();
        }

        // Update is called once per frame
        void Update()
        {
        
        }

        void GenerateVisualMap()
        {
            GameObject prefabToInstantiate;
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    GenerateVisualFor(i, j);
                }
            }
        }

        void GenerateVisualFor(int i, int j)
        {
            GameObject prefabToInstantiate = tilePrefabs.First(t => t.tileType == nodes[i,j].TileTypeSelected).go;
            Instantiate(prefabToInstantiate, new Vector3(i+0.5f, 0, j+0.5f), Quaternion.identity);
        }
    
        private void CollapseWave()
        {
            
            // Init all Nodes
            InitNodes();
        
            // Set Fix nodes
            
            //
            // while (!IsFullyCollapsed())
            // {
            //     IterateWaveCollapse();
            // }
            StartCoroutine( Test() );
        }

        IEnumerator Test()
        {
            while (!IsFullyCollapsed())
            {
                yield return new WaitForSeconds(0.5f);
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
            if (nodeToCollapse.TileTypeSelected == Enums.TileType.None)
            {
                Debug.Log("No tile selected");
                //GenerateVisualFor(nodeToCollapse.Position.x, nodeToCollapse.Position.y);
            } else GenerateVisualFor(nodeToCollapse.Position.x, nodeToCollapse.Position.y);
            
            PropagateWave(nodeToCollapse);
        }

        private Node GetMinEntropyNode()
        {
            List<Node> possiblesNodes = new List<Node>();
            int minEntropy = Int32.MaxValue;
            
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

            // Debug.Log(possiblesNodes.Count);
            // Debug.Log(Random.Range(0, possiblesNodes.Count));
            return possiblesNodes[Random.Range(0, possiblesNodes.Count)];
        }
        
        private void CollapseNode(Node node)
        {
            node.AutoSelectTileType();
        }
        
        private void PropagateWave(Node node)
        {
            List<Node> nodeToUpdate = new List<Node>();
            Node nodeUpToDate;
            nodeToUpdate.Add(node);

            while (nodeToUpdate.Count > 0)
            {
                nodeUpToDate = nodeToUpdate[^1];
                nodeToUpdate.RemoveAt(nodeToUpdate.Count - 1);
                
                // Update Top
                UpdateNode(nodeUpToDate, Enums.Direction.Top, ref nodeToUpdate);

                // Update Bottom
                UpdateNode(nodeUpToDate, Enums.Direction.Bottom, ref nodeToUpdate);

                // Update Left
                UpdateNode(nodeUpToDate, Enums.Direction.Left, ref nodeToUpdate);

                // Update Right
                UpdateNode(nodeUpToDate, Enums.Direction.Right, ref nodeToUpdate);
            }
        }
        
        private void UpdateNode(Node nodeUpToDate, Enums.Direction direction, ref List<Node> nodesToUpdate)
        {
            /* Remove possibilities if it deos not correspond to nodeOrigin's selected or available */
            
            int x = nodeUpToDate.Position.x;
            int y = nodeUpToDate.Position.y;

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
            
            if (x < 0 || x >= width || y < 0 || y >= height) return;
            
            Node nodeToUpdate = nodes[x, y];
            
            if (nodeToUpdate.IsCollapsed) return;
            
            
            //// Possible neighbors from nodeUpToDate
            //List<Enums.NeighborType> possibleNeighbors =  nodeToUpdate.GetPossibleNeighborsDir(Enums.ReverseDirection(direction)); //Need opposite direction since it connect
            
            
            // Possible neighbors for nodeUpToDate
            List<Enums.NeighborType> possibleNeighbors =  nodeUpToDate.GetPossibleNeighborsDir(direction);
            if (possibleNeighbors.Count == 0) return;
            
            
            // Remove possibilities if it does not correspond to nodeUpToDate's selected or available
            foreach (var tileTypesPossible in nodeToUpdate.TileTypesPossibles.ToList())
            {
                var neighborType = Enums.GetNeighborTypeByDirection(tileTypesPossible, Enums.ReverseDirection(direction) );
                
                if (!possibleNeighbors.Contains(neighborType))
                {
                    // Remove tile from possible tiles
                    nodeToUpdate.TileTypesPossibles.Remove(tileTypesPossible);
                    
                    if (!nodesToUpdate.Contains(nodeToUpdate))
                        nodesToUpdate.Add(nodeToUpdate);
                }
            }
        }
    }

    [Serializable] public class PrefabData
    {
        [SerializeField] public GameObject go;
        [SerializeField] public Enums.TileType tileType;
    }
}
