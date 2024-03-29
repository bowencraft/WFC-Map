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
            Grid = grid.Clone() as SuperPosition[,]; // 深复制网格状态
        }
    }

    Stack<GridState> states = new Stack<GridState>();

    
    const int GRID_WIDTH = 17;
    const int GRID_HEIGHT = 9;
    const int MAX_TRIES = 1;
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

    // bool RunWFC()
    // {
    //     InitGrid();
    //
    //     while (DoUnobservedNodesExist())
    //     {
    //         Vector2Int node = GetNextUnobservedNode();
    //         if (node.x == -1)
    //         {
    //             return false; //failure
    //         }
    //
    //         int observedValue = Observe(node);
    //         PropogateNeighbors(node, observedValue);
    //     }
    //
    //     return true; //success
    // }
    
    bool RunWFC()
    {
        InitGrid();
        // 存储节点和尝试失败的瓦片索引
        Stack<KeyValuePair<Vector2Int, int>> backtrackSteps = new Stack<KeyValuePair<Vector2Int, int>>();

        while (DoUnobservedNodesExist())
        {
            Vector2Int node = GetNextUnobservedNode();
            if (node.x == -1)
            {
                if (backtrackSteps.Count > 0)
                {
                    var lastStep = backtrackSteps.Pop();
                    _grid[lastStep.Key.x, lastStep.Key.y].RemovePossibleValue(lastStep.Value); // 移除失败的瓦片选择
                    continue; // 从上一个状态尝试
                }
                else
                {
                    return false; // 如果没有更多的回溯步骤，算法失败
                }
                
            }

            int observedValue = Observe(node);
            // if (observedValue == -1)
            // {
            //     // 如果观测失败，记录当前节点和失败的值，然后尝试回溯
            //     Debug.LogWarning("Observed failure: " + node);
            //     backtrackSteps.Push(new KeyValuePair<Vector2Int, int>(node, observedValue));
            //     continue;
            // }
            backtrackSteps.Push(new KeyValuePair<Vector2Int, int>(node, observedValue));
            PropogateNeighbors(node, observedValue);
            // 如果没有观测失败，可以选择记录当前成功的步骤，但在这个简化的例子中不是必需的
        }

        return true; // 成功
    }



    void DrawTiles() {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y].GetObservedValue()].gameObject);
                tile.transform.position = tile.transform.position + new Vector3(x,0f, y) - new Vector3((GRID_WIDTH-1)/2f, 0f, (GRID_HEIGHT-1)/2f);
            }
        }
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

    int Observe(Vector2Int node)
    {
        // Debug.LogWarning(node.x + "," + node.y);
        if (_grid[node.x, node.y].PossibleValues.Count == 0)
        {
            Debug.LogWarning("No possible values for observation at node " + node);
            return -1; // 表示观测失败
        }
        
        Debug.Log("Observing cell: " + node.x + ", " +  node.y + ", Possible Values: " + _grid[node.x, node.y].PossibleValues.Count);
        return _grid[node.x, node.y].Observe();
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
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
    }
    void PropogateNeighbors(Vector2Int node, int observedValue)
    {
        PropogateTo(node, new Vector2Int(-1, 0), _tileset[observedValue]);
        PropogateTo(node, new Vector2Int(1, 0), _tileset[observedValue]);
        PropogateTo(node, new Vector2Int(0, -1), _tileset[observedValue]);
        PropogateTo(node, new Vector2Int(0, 1), _tileset[observedValue]);
    }

    void PropogateTo(Vector2Int node, Vector2Int direction, MultipleTile mustWorkAdjacentTo)
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
            // if (neighborSuperPosition.NumOptions == 0)
            // {
            //     Debug.LogWarning("No possible values for neighbor " + neighborNode);
            // }

        }
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
