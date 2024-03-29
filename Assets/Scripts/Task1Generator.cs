using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Task1Generator : MonoBehaviour
{
    const int GRID_WIDTH = 17;
    const int GRID_HEIGHT = 9;
    const int MAX_TRIES = 10;
    [SerializeField] List<Tile> _tileset;
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
        InitGrid();

        while (DoUnobservedNodesExist())
        {
            Vector2Int node = GetNextUnobservedNode();
            if (node.x == -1)
            {
                return false; //failure
            }

            int observedValue = Observe(node);
            PropogateNeighbors(node, observedValue);
        }

        return true; //success
    }

    void DrawTiles() {
        for (int x = 0; x < GRID_WIDTH; x++)
        {
            for (int y = 0; y < GRID_HEIGHT; y++)
            {
                GameObject tile = GameObject.Instantiate(_tileset[_grid[x, y].GetObservedValue()].gameObject);
                tile.transform.position = tile.transform.position + new Vector3(x, y, 0f) - new Vector3((GRID_WIDTH-1)/2f, (GRID_HEIGHT-1)/2f, 0f);
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

    void PropogateTo(Vector2Int node, Vector2Int direction, Tile mustWorkAdjacentTo)
    {
        Vector2Int neighborNode = node + direction;
        
        if (neighborNode.x >= 0 && neighborNode.x < GRID_WIDTH && neighborNode.y >= 0 && neighborNode.y < GRID_HEIGHT)
        {
            SuperPosition neighborSuperPosition = _grid[neighborNode.x, neighborNode.y];
        
            List<int> incompatibleValues = new List<int>();
            foreach (var possibleValue in neighborSuperPosition.PossibleValues)
            {
                if (!IsCompatible( mustWorkAdjacentTo, _tileset[possibleValue],direction))
                {
                    incompatibleValues.Add(possibleValue);
                }
            }

            
            foreach (var value in incompatibleValues)
            {
                neighborSuperPosition.RemovePossibleValue(value);
            }

        }
    }


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
    
    bool IsCompatible(Tile a, Tile b, Vector2Int direction)
    {
        
        if (direction == Vector2Int.right)
        {
            return a._rightRoad == b._leftRoad;
        }
        else if (direction == Vector2Int.left) 
        {
            return a._leftRoad == b._rightRoad;
        }
        else if (direction == Vector2Int.up)
        {
            return a._upRoad == b._downRoad;
        }
        else if (direction == Vector2Int.down) 
        {
            return a._downRoad == b._upRoad;
        }

        return false; 
    }

}
