using UnityEngine;

/// <summary>
/// This is a data class representing a position in a movement grid.
/// These positions correspond to tiles in the floor tilemap.
/// </summary>
public class Node
{
    /// <summary>
    /// Flag indicating whether the node is traverable (i.e. false if there is a wall in this location).
    /// </summary>
    public bool Walkable;

    /// <summary>
    /// The world position corresponding to this node.
    /// </summary>
    public Vector3 Position;

    /// <summary>
    /// The grid coordinate for this node on the X axis.
    /// </summary>
    public int gridX;

    /// <summary>
    /// The grid coordinate for this node on the Y axis.
    /// </summary>
    public int gridY;

    /// <summary>
    /// The distance from this node to the start node.
    /// </summary>
    public int gCost;

    /// <summary>
    /// The distance from this node to the end node.
    /// </summary>
    public int hCost;

    /// <summary>
    /// The node relative to this node which gCost and hCost is calculated from.
    /// </summary>
    public Node parent;

    /// <summary>
    /// A constructor which sets the various properties for the node when insantiated.
    /// </summary>
    /// <param name="_walkable">A flag indicating whether the node is traverable.</param>
    /// <param name="_position">The world position corresponding to this node.</param>
    /// <param name="_gridX">The grid coordinate for this node on the X axis.</param>
    /// <param name="_gridY">The grid coordinate for this node on the Y axis.</param>
    public Node(bool _walkable, Vector3 _position, int _gridX, int _gridY)
    {
        Walkable = _walkable;
        Position = _position;
        gridX = _gridX;
        gridY = _gridY;
    }

    /// <summary>
    /// A calculated property representing the total cost of this node.
    /// </summary>
    public int FCost => gCost + hCost;
}