using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// This Unity script component is attached to the Grid gameobject in the scene hierarchy.
/// It handles creating a 2D array of nodes for each floor tile and determines if those nodes are walkable.
/// It also handles converting world positions to nodes, called by the Wizard script component while pathfinding.
/// </summary>
public class MovementGrid : MonoBehaviour
{
    [Tooltip("Toggles a visualization of the tiles that are walkable nodes.")]
    [SerializeField] bool visualize;

    [Tooltip("The tilemap for the floor.")]
    [SerializeField] private Tilemap floor;

    [Tooltip("The tilemap for the walls.")]
    [SerializeField] private Tilemap walls;

    [Tooltip("A prefab representing a tile visualization for walkable nodes.")]
    [SerializeField] private GameObject callPrefab;

    [Tooltip("An X,Y offset to make position 0,0 the lower-left corner of the floor.")]
    [SerializeField] private Vector2 offset;

    [Tooltip("The X,Y dimensions of the grid of nodes (manually match this to the floor dimensions).")]
    [SerializeField] private Vector2 gridSize;

    [Tooltip("A 2D array of movement nodes.")]
    public Node[,] Grid;

    /// <summary>
    /// Sets the size of the 2D array of nodes and populates it with nodes.
    /// </summary>
    private void Awake()
    {
        Grid = new Node[(int)gridSize.x, (int)gridSize.y];
        MakeGrid();
    }

    /// <summary>
    /// Iterates through the tiles in the floor tilemap to create a node for each floor tile.
    /// Optionally, creates gameobjects for each walkable floor tile if visualization is enabled.
    /// </summary>
    private void MakeGrid()
    {
        GameObject Movement = new GameObject();
        Movement.name = "Movement";
        Movement.transform.parent = gameObject.transform;

        TileBase[] tiles = floor.GetTilesBlock(floor.cellBounds);

        for (int x = 0; x < floor.cellBounds.size.x; x++)
        {
            for (int y = 0; y < floor.cellBounds.size.y; y++)
            {
                TileBase tile = tiles[x + y * floor.cellBounds.size.x];

                if (tile != null)
                {
                    Vector3Int cell = new Vector3Int(x, y, (int)floor.transform.position.z);
                    Vector3 position = floor.CellToWorld(cell);

                    // Factor in the offset.
                    position = new Vector3(position.x + offset.x, position.y + offset.y);

                    bool walkable = Walkable(position);
                    Node node = new Node(walkable, position, x, y);
                    Grid[x, y] = node;

                    if (visualize && walkable)
                    {
                        GameObject obj = Instantiate(callPrefab, position, Quaternion.identity);
                        obj.name = $"{x},{y}";
                        obj.transform.parent = Movement.transform;
                    }
                }
            }
        }

        if (!visualize) { Destroy(Movement); };
    }

    /// <summary>
    /// Determines if there is a wall tile at the specified world position.
    /// </summary>
    /// <param name="position">The world position at which to check for the existance of a wall tile.</param>
    /// <returns>True if there is not a wall tile at this position. False if there is a wall tile at this position.</returns>
    private bool Walkable(Vector3 position)
    {
        TileBase wall = walls.GetTile(walls.WorldToCell(position));

        if (wall == null)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Converts a world position into the corresponding node in the 2D array of nodes.
    /// </summary>
    /// <param name="position">The world position at which to retreive the corresponding node.</param>
    /// <returns>The node corresponding to the specified world position.</returns>
    public Node NodeFromWorldPoint(Vector3 position)
    {
        Vector3Int cell = floor.layoutGrid.WorldToCell(position);

        // Factor in the offset.
        cell = new Vector3Int((int)(cell.x + Mathf.Ceil(Mathf.Abs(offset.x))), (int)(cell.y + Mathf.Ceil(Mathf.Abs(offset.y))));

        // Handle the potential for the player to walk out-of-bounds (i.e. off the floor if the walls are disabled).
        if (cell.x < 0 || cell.x > Grid.GetLength(0) - 1 || cell.y < 0 || cell.y > Grid.GetLength(1) - 1) { return null; };

        return Grid[cell.x, cell.y];
    }

    /// <summary>
    /// Returns nodes above, below, to the left, and to the right of the specified node.
    /// </summary>
    /// <param name="node">The center node relative to which adjacent nodes will be returend.</param>
    /// <returns>A list of nodes adjacent to the specified node (cardinal directions only, no diagonals).</returns>
    public List<Node> GetAdjacentNodes(Node node)
    {
        List<Node> adjacentNodes = new List<Node>();

        for (int x = -1; x <= 1; x++) 
        {
            for (int y = -1; y <= 1; y++)
            {
                // Exclude the node in the center of the 3x3 grid.
                if (x == 0 && y == 0) { continue; }

                // Exclude nodes diagonal to the specified node.
                if (x == -1 && y == 1) { continue; }
                if (x == 1 && y == 1) { continue; }
                if (x == -1 && y == -1) { continue; }
                if (x == 1 && y == -1) { continue; }

                int checkX = node.gridX + x;
                int checkY = node.gridY + y; 

                if (checkX >= 0 && checkX < gridSize.x && checkY >= 0 && checkY < gridSize.y) { adjacentNodes.Add(Grid[checkX, checkY]); }
            }
        }

        return adjacentNodes;
    }
}