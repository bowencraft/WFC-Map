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
        public SuperPosition[,] Grid;

        public GridState(SuperPosition[,] grid)
        {
            int width = grid.GetLength(0);
            int height = grid.GetLength(1);
            Grid = new SuperPosition[width, height];

            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    // 使用SuperPosition的深拷贝构造函数
                    Grid[x, y] = new SuperPosition(grid[x, y]);
                }
            }
        }
    }

    Stack<GridState> states = new Stack<GridState>();
    Stack<Vector3Int> nodesValues = new Stack<Vector3Int>();
    

    [Header("Grid Settings")]
    public int GRID_WIDTH = 8;
    public int GRID_HEIGHT = 8;

    public Vector2Int initialObservePoint;
    // public int MAX_TRIES = 16;
    [Header("Tiles")]
    [SerializeField] List<MultipleTile> _tileset;
    
    [Header("Test")]
    [SerializeField] GameObject _titlePrefab;

    [SerializeField] private TextMeshProUGUI _stackText;
    public float _timeBetweenSteps = 0.2f;
    public bool runtimeDrawing = true;
    SuperPosition[,] _grid;

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(RunWFC());
        // int tries = 0;
        // bool result;
        //
        // do
        // {
        //     tries++;
        //     result = RunWFC();
        // }
        // while (result == false && tries < MAX_TRIES);
        //
        // if (result == false)
        // {
        //     print("Unable to solve wave function collapse after " + tries + " tries.");
        // }
        // else
        // {
        //     DrawTiles();
        // }
    }
    
    IEnumerator RunWFC()
    {
        InitGrid(); // initial grid and create object prefab
    
        while (DoUnobservedNodesExist()) // when there are any tiles IsObserved() == false
        {
            Vector2Int node = initialObservePoint != new Vector2Int(-1,-1) ?initialObservePoint :GetNextUnobservedNode(); // get tile with lowest options
            initialObservePoint = new Vector2Int(-1, -1);
            if (node.x == -1)
            {
                // return false;
                print("Unable to solve wave function collapse");
            }

            // states.Push(new GridState((SuperPosition[,])_grid.Clone())); // push current state to stack

            int observedValue = SelectOption(node); // observe the super position
            //
            // if (observedValue == -1) // if node is _observed || _possibleValues.Count == 0
            // {
            //     if (states.Count > 0)
            //     {
            //         _grid = states.Pop().Grid; // 恢复上一个状态
            //     }
            //     else
            //     {
            //         Debug.LogWarning("Count error for observation at node " + node);
            //         return false;
            //     }
            // }
            // else
            // {
            states.Push(new GridState((SuperPosition[,])_grid.Clone()));
            nodesValues.Push(new Vector3Int(node.x,node.y,observedValue));
            // _stackText.text += "(" + node.x + "," + node.y + ") - " + observedValue + "\n";
            
            if (PropogateNeighbors(node, observedValue))
            {
                _grid[node.x, node.y].SetCurrentValue(observedValue);
                _grid[node.x, node.y].SetObserved(true);
                // DrawTile( node.x, node.y);
                // print("observed " + node.x + "," + node.y + " as " + observedValue);
            }
            else
            {
                _grid = states.Pop().Grid; // 恢复上一个状态
                nodesValues.Pop();
                // _stackText.text = _stackText.text.Substring(0, _stackText.text.LastIndexOf("\n"));
                
                _grid[node.x, node.y].RemovePossibleValue(observedValue);
                print("failed to propogate neighbors for " + node.x + "," + node.y + " as " + observedValue + " removing from options");
                if (_grid[node.x, node.y].NumOptions == 0)
                {
                    if (states.Count > 0)
                    {
                        _grid = states.Pop().Grid; // 恢复上一个状态
                        Vector3Int nodeValue = nodesValues.Pop();
                        
                        // _stackText.text = _stackText.text.Substring(0, _stackText.text.LastIndexOf("\n"));
                        _grid[nodeValue.x, nodeValue.y].RemovePossibleValue(nodeValue.z);
                    }
                    // else
                    // {
                    print("all options for this node is not available");
                        // return false;
                    // }
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
            // }

        }
        if (!runtimeDrawing) DrawTiles();
        _stackText.text += "Done!";

        // return true; // 成功
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
                if (_grid[x, y].GetCurrentValue() != -1 && _grid[x, y].IsObserved())
                {
                    GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y].GetCurrentValue()].gameObject);
                    tile.transform.position = tile.transform.position + new Vector3(x,0f, y) - new Vector3((GRID_WIDTH-1)/2f, 0f, (GRID_HEIGHT-1)/2f);
                    tile.transform.parent = tilesParent.transform;
                }
                GameObject title = GameObject.Instantiate(_titlePrefab);
                title.transform.position = title.transform.position + new Vector3(x,0f, y) - new Vector3((GRID_WIDTH-1)/2f, 0f, (GRID_HEIGHT-1)/2f);
                title.transform.SetParent(tilesParent.transform);
                title.gameObject.GetComponent<TextMeshPro>().text = _grid[x, y].NumOptions.ToString();
            }
        }

        string text = "Stacks: \n";
        List<Vector3Int> reversedNodesValues = nodesValues.ToList();
        reversedNodesValues.Reverse();

        foreach (Vector3Int nodeValue in reversedNodesValues)
        {
            text += "(" + nodeValue.x + "," + nodeValue.y + ") - " + nodeValue.z + "\n";
        }
        _stackText.text = text;
    }


    bool DoUnobservedNodesExist()
    {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (_grid[x, y].IsObserved() == false) {
                    return true;
                }
            }
        }

        return false;
    }

    int SelectOption(Vector2Int node)
    {
        // Debug.LogWarning(node.x + "," + node.y);
        // if (_grid[node.x, node.y].PossibleValues.Count == 0)
        // {
        //     Debug.LogWarning("No possible values for observation at node " + node);
        //     return -1; // 表示观测失败
        // }
        return _grid[node.x, node.y].SelectOption();
    }


    private void InitGrid()
    {
        _grid = new SuperPosition[GRID_WIDTH, GRID_HEIGHT];

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                _grid[x, y] = new SuperPosition(_tileset.Count);

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
    
    bool PropogateNeighbors(Vector2Int node, int observedValue)
    {
        return
        (PropogateTo(node, new Vector2Int(-1, 0), _tileset[observedValue]) &&
        PropogateTo(node, new Vector2Int(1, 0), _tileset[observedValue]) &&
        PropogateTo(node, new Vector2Int(0, -1), _tileset[observedValue]) &&
        PropogateTo(node, new Vector2Int(0, 1), _tileset[observedValue]) );

    }

    bool PropogateTo(Vector2Int node, Vector2Int direction, MultipleTile mustWorkAdjacentTo)
    {
        Vector2Int neighborNode = node + direction;
        
        if (neighborNode.x >= 0 && neighborNode.x < GRID_WIDTH && neighborNode.y >= 0 && neighborNode.y < GRID_HEIGHT)
        {
            SuperPosition neighborSuperPosition = _grid[neighborNode.x, neighborNode.y];
        
            List<int> incompatibleValues = new List<int>();
            foreach (var possibleValue in neighborSuperPosition.PossibleValues)
            {
                if (!MultipleTile.IsCompatible( mustWorkAdjacentTo, _tileset[possibleValue],direction))
                {
                    incompatibleValues.Add(possibleValue);
                }
            }

            
            foreach (var value in incompatibleValues)
            {
                neighborSuperPosition.RemovePossibleValue(value);
            }
            //
            return (neighborSuperPosition.NumOptions != 0);

        }

        return true; // not dealing with edge cases
    }

    // find next node with minimum options
    Vector2Int GetNextUnobservedNode()
    {
        Vector2Int minOptionNode = new Vector2Int(-1, -1);
        int minOptions = int.MaxValue;
        
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                if (!_grid[x, y].IsObserved())
                {
                    int options = _grid[x, y].NumOptions;
        
                    if (options < minOptions && options > 0) 
                    {
                        minOptions = options;
                        minOptionNode = new Vector2Int(x, y);
        
                        if (minOptions == 1)
                        {
                            return minOptionNode;
                        }
                    }
                }
            }
        }
        
        return minOptionNode; 

    }
    

}
