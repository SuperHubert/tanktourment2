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
        [SerializeField] private Vector3 scale;
        
        [field:Header("Map customization")]
        [SerializeField] private Vector2Int spawnPosition;
        [SerializeField] private Vector2Int spawnSize;
        
        [SerializeField] private Vector2Int connectorsPosition;
        [SerializeField] private Vector2Int connectorsSize;
        
        [SerializeField] private Vector2Int controlPosition;
        [SerializeField] private Vector2Int controlPSize;
        
        private int Width => width + 2;
        private int Height => height + 2;


        
        [field:Header("Prefabs")]
        [field:SerializeField] private List<PrefabData> tilePrefabs;

        
        private Node[,] nodes;

        
        // Start is called before the first frame update
        void Start()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            nodes = new Node[Width, Height];
            
            GenerateMap();
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        void GenerateVisualMap()
        {
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    GenerateVisualFor(i, j);
                }
            }
        }

        void GenerateVisualFor(int i, int j)
        {
            if(i < 0 || i >= Width || j < 0 || j >= Height) return;

            var prefabToInstantiate = tilePrefabs[4].go;

            var useDefault = nodes[i, j].TileTypeSelected == null;
            
            if (!useDefault) prefabToInstantiate = nodes[i,j].TileTypeSelected.go;
            
            var tile = Instantiate(prefabToInstantiate,
                new Vector3(i * (offset + scale.x) + 0.5f, 0, j * (scale.z + offset) + 0.5f), Quaternion.identity,
                useDefault ? transform : null);
            tile.transform.localScale = scale;
            
            var mat = tile.Renderer.material;
            mat.color = nodes[i,j].debugColor;
            
            tile.Renderer.material = mat;
            
        }
    
        private void GenerateMap()
        {
            
            // Init all Nodes
            InitNodes();
        
            // Set Fix nodes
            Debug.Log("Control Points");
            SpawnPoints(controlPosition, controlPSize, tilePrefabs[0]);
            
            Debug.Log("Spawn Points");
            SpawnPoints(spawnPosition, spawnSize, tilePrefabs[1]);
            
            Debug.Log("Connectors Points");
            SpawnPoints(connectorsPosition, connectorsSize, tilePrefabs[2]);

            var rotatedPos = new Vector2Int(connectorsPosition.y, connectorsPosition.x);
            var rotatedSize = new Vector2Int(connectorsSize.y, connectorsSize.x);

            SpawnPoints(rotatedPos, rotatedSize, tilePrefabs[2]);
            
            // Connect : Player - connector - control
            // Connect : control - control (2 to 2, then one of each pair to the other one)
            // Wall around the connectors
            // Collapse the rest :happy:
            // Wall around the map (ignore collapse)


            GenerateVisualMap();
            
            //StartCoroutine( IterateSlowly() );
        }
        
        private void SpawnPoints(Vector2Int position, Vector2Int size, PrefabData prefabData)
        {
            position += Vector2Int.one;
            SpawnControlPoint(position, size);
            SpawnControlPoint(new Vector2Int((Width)-position.x, (Height)-position.y), -size);

            SpawnControlPoint(new Vector2Int((Width)-position.y, position.x), new Vector2Int(-size.y, size.x));
            SpawnControlPoint(new Vector2Int(position.y, (Height)-position.x), new Vector2Int(size.y, -size.x));
            
            return;
            
            void SpawnControlPoint(Vector2Int p, Vector2Int s)
            {
                var a = p;
                var b = p + s;

                if(a.x > b.x) (a.x, b.x) = (b.x, a.x);
                if(a.y > b.y) (a.y, b.y) = (b.y, a.y);
                
                for (int y = a.y; y < b.y; y++)
                {
                    for (int x = a.x; x < b.x; x++)
                    {
                        nodes[x, y].debugColor = prefabData.DebugColor;
                    }
                }
                
                var randX = Random.Range(a.x, b.x);
                var randY = Random.Range(a.y, b.y);
                nodes[randX, randY].SelectTile(prefabData);
            }
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
            nodes = new Node[Width, Height];
            
            for (int i = 0; i < Width; i++)
            {
                for (int j = 0; j < Height; j++)
                {
                    nodes[i, j] = new Node(new Vector2Int(i, j), tilePrefabs);
                }
            }
        }
    
        private bool IsFullyCollapsed()
        {
            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < height; j++)
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
            if (nodeToCollapse.TileTypeSelected != null) GenerateVisualFor(nodeToCollapse.Position.x, nodeToCollapse.Position.y);
            
            PropagateWave(nodeToCollapse);
        }
        
        private Node GetMinEntropyNode()
        {
            List<Node> possiblesNodes = new List<Node>();
            int minEntropy = Int32.MaxValue;
            
            for (int i = 1; i < width; i++)
            {
                for (int j = 1; j < height; j++)
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
            
            if (x < 1 || x >= width || y < 1 || y >= height) return;
            
            Node nodeToUpdate = nodes[x, y];
            
            if (nodeToUpdate.IsCollapsed) return;
            
            // Possible neighbors for nodeUpToDate
            List<string> possibleNeighbors =  nodeUpToDate.GetPossibleNeighborsDir(direction);
            if (possibleNeighbors.Count == 0) return;
            
            
            // Remove possibilities if it does not correspond to nodeUpToDate's selected or available
            foreach (var tileTypesPossible in nodeToUpdate.TileTypesPossibles.ToList())
            {
                var neighborType = tileTypesPossible.GetPrefabConnexion(Enums.ReverseDirection(direction));
                
                if (!neighborType.Any( t => possibleNeighbors.Contains(t) ))
                {
                    // Remove tile from possible tiles
                    nodeToUpdate.TileTypesPossibles.Remove(tileTypesPossible);
                    
                    if (!nodesToUpdate.Contains(nodeToUpdate)) nodesToUpdate.Add(nodeToUpdate);
                }
            }
        }
    }

    [Serializable] public class PrefabData
    {
        [SerializeField] public string name; //ID but called name so it renames Element i in Inspector
        [SerializeField] public WorldTile go;

        [SerializeField] public string top;
        [SerializeField] public string bot;
        [SerializeField] public string left;
        [SerializeField] public string right;

        [field: SerializeField] public bool CanSpawn { get; private set; } = true;
        [field: SerializeField] public Color DebugColor { get; private set; } = Color.white;


        public string[] GetPrefabConnexion(Enums.Direction direction)
        {
            return direction switch
            {
                Enums.Direction.Top => top.Split(";"),
                Enums.Direction.Bottom => bot.Split(";"),
                Enums.Direction.Left => left.Split(";"),
                Enums.Direction.Right => right.Split(";"),
                _ => Array.Empty<string>()
            };
        }
    }
}
