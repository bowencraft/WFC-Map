using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    
    public int GRID_WIDTH = 8;
    public int GRID_HEIGHT = 8;
    public int MAX_TRIES = 16;
    [SerializeField] List<MultipleTile> _tileset;
    SuperPosition[,] _grid;

    // Start is called before the first frame update
    void Start()
    {
        int tries = 0;
        bool result;

        do
        {
            tries++;
            result = RunWFC();
        }
        while (result == false && tries < MAX_TRIES);

        if (result == false)
        {
            print("Unable to solve wave function collapse after " + tries + " tries.");
        }
        else
        {
            DrawTiles();
        }
    }
    
    bool RunWFC()
    {
        InitGrid(); // initial grid and create object prefab
    
        while (DoUnobservedNodesExist()) // when there are any tiles IsObserved() == false
        {
            Vector2Int node = GetNextUnobservedNode(); // get tile with lowest options
            if (node.x == -1)
            {
                return false;
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
            if (PropogateNeighbors(node, observedValue))
            {
                _grid[node.x, node.y].SetCurrentValue(observedValue);
                _grid[node.x, node.y].SetObserved(true);
            }
            else
            {
                _grid = states.Pop().Grid; // 恢复上一个状态
                _grid[node.x, node.y].RemovePossibleValue(observedValue);
                if (_grid[node.x, node.y].NumOptions == 0)
                {
                    // if (states.Count > 0)
                    // {
                    //     _grid = states.Pop().Grid; // 恢复上一个状态
                    // }
                    // else
                    // {
                    print("all options for this node is not available");
                        return false;
                    // }
                }
            }
                
            // }

        }

        return true; // 成功
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
                GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y].GetCurrentValue()].gameObject);
                tile.transform.position = tile.transform.position + new Vector3(x,0f, y) - new Vector3((GRID_WIDTH-1)/2f, 0f, (GRID_HEIGHT-1)/2f);
                tile.transform.parent = tilesParent.transform;
            }
        }
    }

    void DrawTile(int x, int y)
    {
        GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y].GetCurrentValue()].gameObject);
        tile.transform.position = tile.transform.position + new Vector3(x,0f, y) - new Vector3((GRID_WIDTH-1)/2f, 0f, (GRID_HEIGHT-1)/2f);
        tile.transform.parent = GameObject.Find("Tiles").transform;
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

    public GameObject prefabObject;

    private void InitGrid()
    {
        _grid = new SuperPosition[GRID_WIDTH, GRID_HEIGHT];

        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                _grid[x, y] = new SuperPosition(_tileset.Count);

                if (prefabObject != null)
                {
                    _grid[x, y].gridObject = Instantiate(prefabObject);
                    _grid[x, y].gridObject.transform.position = _grid[x, y].gridObject.transform.position + new Vector3(x,0f, y) - new Vector3((GRID_WIDTH-1)/2f, 0f, (GRID_HEIGHT-1)/2f);

                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }

    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }
    
    bool PropogateNeighbors(Vector2Int node, int observedValue)
    {
        if (
        (PropogateTo(node, new Vector2Int(-1, 0), _tileset[observedValue]) ||
        PropogateTo(node, new Vector2Int(1, 0), _tileset[observedValue])||
        PropogateTo(node, new Vector2Int(0, -1), _tileset[observedValue]) ||
        PropogateTo(node, new Vector2Int(0, 1), _tileset[observedValue]) ) == false)
        {
            return false;
        }

        return true;
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
