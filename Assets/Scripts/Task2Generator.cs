using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Task2Generator : MonoBehaviour
{
    class GridState
    {
        public SuperPosition[,,] Grid;

        public GridState(SuperPosition[,,] grid)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            int depth = grid.GetLength(2);
            Grid = new SuperPosition[width, height, depth];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    for (int z = 0; z < depth; z++)
                    {
                        // 使用SuperPosition的深拷贝构造函数
                        Grid[x, y, z] = new SuperPosition(grid[x, y, z]);
                    }
                }
            }
        }
    }

    Stack<GridState> states = new Stack<GridState>();
    
    public struct NodeValue
    {
        public Vector3Int Node { get; set; }
        public int Value { get; set; }

        public NodeValue(Vector3Int node, int value)
        {
            Node = node;
            Value = value;
        }
    }

    private Stack<NodeValue> nodesValues = new Stack<NodeValue>();


    [Header("Grid Settings")]
    public int GRID_WIDTH = 8;
    public int GRID_HEIGHT = 8;
    public int GRID_DEPTH = 8; // New depth for 3D

    public Vector3Int initialObservePoint;
    [Header("Tiles")]
    [SerializeField] List<MultipleTile> _tileset;

    [Header("Test")]
    [SerializeField] GameObject _titlePrefab;

    [SerializeField] private TextMeshProUGUI _stackText;
    public float _timeBetweenSteps = 0.2f;
    public bool runtimeDrawing = true;
    SuperPosition[,,] _grid; // Changed to 3D

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunWFC());
    }

    IEnumerator RunWFC()
    {
        InitGrid(); // initial grid and create object prefab

        while (DoUnobservedNodesExist()) // when there are any tiles IsObserved() == false
        {
            Vector3Int node = initialObservePoint != new Vector3Int(-1,-1,-1) ?initialObservePoint :GetNextUnobservedNode(); // get tile with lowest options
            initialObservePoint = new Vector3Int(-1, -1, -1);
            if (node.x == -1)
            {
                print("Unable to solve wave function collapse");
            }
            int observedValue = SelectOption(node); // observe the super position

            states.Push(new GridState((SuperPosition[,,])_grid.Clone()));
            nodesValues.Push(new NodeValue(new Vector3Int(node.x,node.y,node.z), observedValue) );

            if (PropogateNeighbors(node, observedValue))
            {
                _grid[node.x, node.y, node.z].SetCurrentValue(observedValue);
                _grid[node.x, node.y, node.z].SetObserved(true);
            }
            else
            {
                _grid = states.Pop().Grid; // 恢复上一个状态
                nodesValues.Pop();
                _grid[node.x, node.y, node.z].RemovePossibleValue(observedValue);
                print("Popped state to last state");
                if (_grid[node.x, node.y, node.z].NumOptions == 0)
                {
                    if (states.Count > 0)
                    {
                        _grid = states.Pop().Grid; // 恢复上一个状态
                        NodeValue nodeValue = nodesValues.Pop();
                        Vector3Int nodeValueNode = nodeValue.Node;
                        int value = nodeValue.Value;
                        _grid[nodeValueNode.x, nodeValueNode.y, nodeValueNode.z].RemovePossibleValue(value);
                    }
                }
            }

            if (runtimeDrawing)
            {
                DrawTiles();
                yield return new WaitForSeconds(_timeBetweenSteps);
            }
            else
            {
                yield return null;
            }
        }
        if (!runtimeDrawing) DrawTiles();
        _stackText.text += "Done!";
    }





    void DrawTiles()
    {
        GameObject tilesParent = GameObject.Find("Tiles");
        while (tilesParent.transform.childCount > 0)
        {
            DestroyImmediate(tilesParent.transform.GetChild(0).gameObject);
        }

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    if (_grid[x, y, z].GetCurrentValue() != -1 && _grid[x, y, z].IsObserved())
                    {
                        GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y, z].GetCurrentValue()].gameObject);
                        tile.transform.position = tile.transform.position + new Vector3(x, y, z) - new Vector3((GRID_WIDTH-1)/2f, (GRID_HEIGHT-1)/2f, (GRID_DEPTH-1)/2f);
                        tile.transform.parent = tilesParent.transform;
                    }
                    GameObject title = GameObject.Instantiate(_titlePrefab);
                    title.transform.position = title.transform.position + new Vector3(x, y, z) - new Vector3((GRID_WIDTH-1)/2f, (GRID_HEIGHT-1)/2f, (GRID_DEPTH-1)/2f);
                    title.transform.SetParent(tilesParent.transform);
                    title.gameObject.GetComponent<TextMeshPro>().text = _grid[x, y, z].NumOptions.ToString();
                }
            }
        }

        string text = "Stacks: \n";
        List<NodeValue> reversedNodesValues = nodesValues.ToList();
        reversedNodesValues.Reverse();

        foreach (NodeValue nodeValue in reversedNodesValues)
        {
            text += "(" + nodeValue.Node.x + "," + nodeValue.Node.y + "," + nodeValue.Node.z + ") - " + nodeValue.Value + "\n";
        }
        _stackText.text = text;
    }


    bool DoUnobservedNodesExist()
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    if (_grid[x, y, z].IsObserved() == false) {
                        return true;
                    }
                }
            }
        }

        return false;
    }

    int SelectOption(Vector3Int node)
    {
        return _grid[node.x, node.y, node.z].SelectOption();
    }


    private void InitGrid()
    {
        _grid = new SuperPosition[GRID_WIDTH, GRID_HEIGHT, GRID_DEPTH];

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    _grid[x, y, z] = new SuperPosition(_tileset.Count);
                }
            }
        }
    }

    // Update is called once per frame
    // void Update()
    // {
    //     if (Input.GetKeyDown(KeyCode.R))
    //     {
    //         SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    //     }
    // }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }
    
    bool PropogateNeighbors(Vector3Int node, int observedValue)
    {
        return
            (PropogateTo(node, new Vector3Int(-1, 0, 0), _tileset[observedValue]) &&
             PropogateTo(node, new Vector3Int(1, 0, 0), _tileset[observedValue]) &&
             PropogateTo(node, new Vector3Int(0, -1, 0), _tileset[observedValue]) &&
             PropogateTo(node, new Vector3Int(0, 1, 0), _tileset[observedValue]) &&
             PropogateTo(node, new Vector3Int(0, 0, -1), _tileset[observedValue]) &&
             PropogateTo(node, new Vector3Int(0, 0, 1), _tileset[observedValue]));

    }

    bool PropogateTo(Vector3Int node, Vector3Int direction, MultipleTile mustWorkAdjacentTo)
    {
        Vector3Int neighborNode = node + direction;

        if (neighborNode.x >= 0 && neighborNode.x < GRID_WIDTH && 
            neighborNode.y >= 0 && neighborNode.y < GRID_HEIGHT &&
            neighborNode.z >= 0 && neighborNode.z < GRID_DEPTH)
        {
            SuperPosition neighborSuperPosition = _grid[neighborNode.x, neighborNode.y, neighborNode.z];

            List<int> incompatibleValues = new List<int>();
            foreach (var possibleValue in neighborSuperPosition.PossibleValues)
            {
                if (!MultipleTile.IsCompatible(mustWorkAdjacentTo, _tileset[possibleValue], direction))
                {
                    incompatibleValues.Add(possibleValue);
                }
            }

            foreach (var value in incompatibleValues)
            {
                neighborSuperPosition.RemovePossibleValue(value);
            }

            return (neighborSuperPosition.NumOptions != 0);
        }

        return true; // not dealing with edge cases
    }

    // find next node with minimum options
    Vector3Int GetNextUnobservedNode()
    {
        Vector3Int minOptionNode = new Vector3Int(-1, -1, -1);
        int minOptions = int.MaxValue;

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                for (int z = 0; z < GRID_DEPTH; z++)
                {
                    if (!_grid[x, y, z].IsObserved())
                    {
                        int options = _grid[x, y, z].NumOptions;

                        if (options < minOptions && options > 0)
                        {
                            minOptions = options;
                            minOptionNode = new Vector3Int(x, y, z);

                            if (minOptions == 1)
                            {
                                return minOptionNode;
                            }
                        }
                    }
                }
            }
        }

        return minOptionNode;
    }
    

}
