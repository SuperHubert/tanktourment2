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
        public static WaveCollapseManager Instance { get; private set; }
     
        
        [field:Header("Wave Collapse Manager")]
        [SerializeField] private int creationSpeed;
        [SerializeField] private int width;
        [SerializeField] private int height;
        [SerializeField] private float offset;
        
        
        [field:Header("Prefabs")]
        [field:SerializeField] private List<PrefabData> tilePrefabs;

        
        private Node[,] nodes;

        
        // Start is called before the first frame update
        void Start()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            nodes = new Node[width, height];
            
            CollapseWave();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        void GenerateVisualMap()
        {
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
            GameObject prefabToInstantiate = tilePrefabs.First(t => t.name == nodes[i,j].TileTypeSelected).go;
            Instantiate(prefabToInstantiate, new Vector3(i * (1+offset) + 0.5f, 0, j * (1+offset) +0.5f), Quaternion.identity);
        }
    
        private void CollapseWave()
        {
            
            // Init all Nodes
            InitNodes();
        
            // Set Fix nodes
            
            
            StartCoroutine( IterateSlowly() );
        }

        IEnumerator IterateSlowly()
        {
            while (!IsFullyCollapsed())
            {
                yield return new WaitForSeconds(creationSpeed);
                IterateWaveCollapse();
            }
        }
    
        private void InitNodes()
        {
            nodes = new Node[width, height];
            var allShapes = tilePrefabs.Select(t => t.name).ToList();
            
            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    nodes[i, j] = new Node(new Vector2Int(i, j), allShapes);
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
            if (nodeToCollapse.TileTypeSelected != "") GenerateVisualFor(nodeToCollapse.Position.x, nodeToCollapse.Position.y);
            
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
            /* Remove possibilities if it does not correspond to nodeUpToDate's selected or available */
            
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
            
            // Possible neighbors for nodeUpToDate
            List<string> possibleNeighbors =  nodeUpToDate.GetPossibleNeighborsDir(direction);
            if (possibleNeighbors.Count == 0) return;
            
            
            // Remove possibilities if it does not correspond to nodeUpToDate's selected or available
            foreach (var tileTypesPossible in nodeToUpdate.TileTypesPossibles.ToList())
            {
                var neighborType = GetPrefabConnexionByName(tileTypesPossible, Enums.ReverseDirection(direction));
                
                //Enums.GetNeighborTypeByDirection(tileTypesPossible, Enums.ReverseDirection(direction) );
                
                if (!neighborType.Any( t => possibleNeighbors.Contains(t) ))
                {
                    // Remove tile from possible tiles
                    nodeToUpdate.TileTypesPossibles.Remove(tileTypesPossible);
                    
                    if (!nodesToUpdate.Contains(nodeToUpdate)) nodesToUpdate.Add(nodeToUpdate);
                }
            }
        }


        public string[] GetPrefabConnexionByName(string name, Enums.Direction direction)
        {
            return GetPrefabConnexion(GetPrefabIndex(name), direction);
        }
        
        private int GetPrefabIndex(string name)
        {
            return tilePrefabs.FindIndex(t => t.name == name);
        }

        private string[] GetPrefabConnexion(int i, Enums.Direction direction)
        {
            switch (direction)
            {
                case Enums.Direction.Top:
                    return tilePrefabs[i].top.Split(";");
                case Enums.Direction.Bottom:
                    return tilePrefabs[i].bot.Split(";");
                case Enums.Direction.Left:
                    return tilePrefabs[i].left.Split(";");
                case Enums.Direction.Right:
                    return tilePrefabs[i].right.Split(";");
                default:
                    return Array.Empty<string>();
            }
        }
    }

    [Serializable] public class PrefabData
    {
        [SerializeField] public string name; //ID but called name so it renames Element i in Inspector
        [SerializeField] public GameObject go;
        
        [SerializeField] public string top;
        [SerializeField] public string bot;
        [SerializeField] public string left;
        [SerializeField] public string right;

    }
}
