using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This Unity script component is attached to the Wizard gameobject in the scene hierarchy.
/// It handles moving (by pathfinding to the player's location), taunting the player, and collision with the player.
/// </summary>
public class Wizard : MonoBehaviour
{
    [Tooltip("The MovementGrid script component attached to the Grid gameobject.")]
    [SerializeField] private MovementGrid grid;

    [Tooltip("The Avatar gameobject.")]
    [SerializeField] private GameObject avatar;

    [Tooltip("The movement speed of the wizard.")]
    [SerializeField] private float speed;

    [Tooltip("The distance the player should have moved before the path is recalculated.")]
    [SerializeField] private float distanceToRecalulate;

    [Tooltip("The taunt sound effect to play when the wizard begins the chase.")]
    [SerializeField] private AudioClip runCoward;

    [Tooltip("The spell casting sound effect.")]
    [SerializeField] private AudioClip spell;

    [Tooltip("The scream sound effect.")]
    [SerializeField] private AudioClip scream;

    private bool firstTaunt = false;
    private bool moving = false;
    private bool stop = false;
    private bool casting = false;
    private bool cheating = false;
    private float elapsedTime;
    private float tauntTimer;
    private readonly float delay = 1.0f;
    private List<Node> path = new List<Node>();
    private Vector3 avatarPosition;
    
    private Animator _animator;
    private AudioSource _sound;

    [Tooltip("A list of taunt sound effects which will be randomly played at random times.")]
    public List<AudioClip> taunts;

    /// <summary>
    /// Connects to various script components attached to the Wizard gameobject and plays the idle animation clip.
    /// </summary>
    private void Awake()
    {
        _animator = GetComponent<Animator>();
        _animator.Play("Idle");
        _sound = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Handles taunting the player at random times and updating the wizard's destination every one second.
    /// </summary>
    private void FixedUpdate()
    {
        elapsedTime += Time.deltaTime;

        // Plays the initial taunt sound effect after a short delay (allowing for the game to start).
        if (elapsedTime >= delay && !firstTaunt)
        {
            _sound.clip = runCoward;
            _sound.Play();
            firstTaunt = true;

            tauntTimer = Time.deltaTime + Random.Range(5.0f, 10.0f);
        }

        // Plays a random taunt sound effect at random times between 3 and 7 seconds.
        if (elapsedTime >= tauntTimer && firstTaunt && !casting)
        {
            Taunt();
            tauntTimer += Time.deltaTime + Random.Range(3.0f, 7.0f);
        }

        // Updates the wizard's destination after a short delay (every one second).
        if (elapsedTime >= delay && !casting) { UpdateDestination(); }
    }

    /// <summary>
    /// Updates the destination node the wizard should move to if the player has moved a specified distance from the node they were at the last time the pathfinding was performed.
    /// </summary>
    private void UpdateDestination()
    {
        if (!moving)
        {
            avatarPosition = avatar.transform.position;
            path = FindPath(transform.position, avatarPosition);
            StartCoroutine(Moving());
        }
        else
        {
            if (Vector3.Distance(avatarPosition, avatar.transform.position) > distanceToRecalulate)
            {
                stop = true;
                avatarPosition = avatar.transform.position;
            }
        }
    }

    /// <summary>
    /// Moves the wizard towards each node in the path at a specified speed.
    /// Detects when the player is cheating (standing on an unreachable node or no node at all) and screams.
    /// </summary>
    /// <returns>An IEnumerator used by the Unity coroutine</returns>
    private IEnumerator Moving()
    {
        if (path.Count == 0)
        {
            if (!cheating)
            {
                _animator.Play("Casting");
                Scream();
                cheating = true;
            }

            moving = false;
            yield break;
        }

        moving = true;
        cheating = false;
        _animator.Play("Walking");

        foreach (Node node in path)
        {
            while (transform.position != node.Position)
            {
                transform.position = Vector3.MoveTowards(transform.position, node.Position, speed * Time.deltaTime);
                yield return new WaitForEndOfFrame();
            }

            float step = speed * Time.deltaTime;
            transform.position = Vector3.MoveTowards(transform.position, node.Position, step);

            if (stop)
            {
                moving = false;
                stop = false;
                yield break;
            }
        }

        path.Clear();
        moving = false;
        _animator.Play("Idle");
    }

    /// <summary>
    /// Finds the optimal movement path (a list of traversable nodes) between two positions using A* pathfinding.
    /// </summary>
    /// <param name="startPos">The starting position to calculate the movement path from. </param>
    /// <param name="destinationPos">The destination position to calculate the movement path to.</param>
    /// <returns>A list of nodes in the order they should be traversed.</returns>
    private List<Node> FindPath(Vector3 startPos, Vector3 destinationPos)
    {
        Node startNode = grid.NodeFromWorldPoint(startPos);
        Node destinationNode = grid.NodeFromWorldPoint(destinationPos);

        // Destination node is null if the player is out-of-bounds.
        if (destinationNode == null) { return new List<Node>(); }

        List<Node> openSet = new List<Node>();
        HashSet<Node> closedSet = new HashSet<Node>();
        openSet.Add(startNode);

        while (openSet.Count > 0)
        {
            Node currentNode = openSet[0];

            // Find the node in the open set with the lowest fCost and make it the current node.
            for (int i = 1; i < openSet.Count; i++)
            {
                if (openSet[i].FCost < currentNode.FCost || openSet[i].FCost == currentNode.FCost && openSet[i].hCost < currentNode.hCost)
                {
                    currentNode = openSet[i];
                }
            }

            // Moves the current node from the open set to the closed set.
            openSet.Remove(currentNode);
            closedSet.Add(currentNode);

            // The optimal path has been found. Return the list of nodes in retraced (reversed) order.
            if (currentNode == destinationNode)
            {
                return RetracePath(startNode, destinationNode);
            }

            // Iterate through each node adjacent to the current node.
            foreach (Node adjacent in grid.GetAdjacentNodes(currentNode))
            {
                // Skip the node if it is not traversable or already in the closed set.
                if (!adjacent.Walkable || closedSet.Contains(adjacent)) { continue; }

                int movementCostToAdjacent = currentNode.gCost + GetDistance(currentNode, adjacent);

                // The movement cost to the adjacent node is lower or the adjacent node is not in the open set.
                if (movementCostToAdjacent < adjacent.gCost || !openSet.Contains(adjacent)) 
                {
                    // Set the gCost and hCost of the adjacent node and the set the parent to the current node.
                    adjacent.gCost = movementCostToAdjacent;
                    adjacent.hCost = GetDistance(adjacent, destinationNode);
                    adjacent.parent = currentNode;      
                    
                    // If the adjacent node is not in the open set, add it.
                    if (!openSet.Contains(adjacent)) {  openSet.Add(adjacent); }
                }
            }
        }

        return new List<Node>();
    }

    /// <summary>
    /// Retraces the path that has been established by accessing each node's parent.
    /// </summary>
    /// <param name="startNode">The node representing the starting position in the movement path.</param>
    /// <param name="endNode">The node repreesnting the destination position in the movement path.</param>
    /// <returns>A list of nodes, reversed back into the order in which they should be traversed.</returns>
    private List<Node> RetracePath(Node startNode, Node endNode)
    {
        List<Node> path = new List<Node>();
        Node currentNode = endNode;

        while (currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }

        path.Reverse();

        return path;
    }

    /// <summary>
    /// Calculates the distance between to specified nodes.
    /// </summary>
    /// <param name="nodeA">The node to calculate the distance from.</param>
    /// <param name="nodeB">The node to calculate the distance to.</param>
    /// <returns>The distance between the two specified nodes.</returns>
    private int GetDistance(Node nodeA, Node nodeB)
    {
        int distanceX = Mathf.Abs(nodeA.gridX - nodeB.gridX);
        int distanceY = Mathf.Abs(nodeA.gridY - nodeB.gridY);

        if (distanceX > distanceY) 
        {
            return 14 * distanceY + 10 * (distanceX - distanceY);
        }
        else
        {
            return 14 * distanceX + 10 * (distanceY - distanceX);
        }
    }

    /// <summary>
    /// Triggered collision event when the wizard collides with the avatar.
    /// Stops all movement, plays the casting animation and sound effect and kills the player.
    /// </summary>
    /// <param name="collider"><The 2D collider which the wizard has collided with./param>
    private void OnTriggerEnter2D(Collider2D collider)
    {
        stop = true;
        casting = true;
        _animator.Play("Casting");
        _sound.clip = spell;
        _sound.Play();

        avatar.GetComponent<Animator>().Play("Idle");
        avatar.GetComponent<Avatar>().Dead = true;
        Invoke(nameof(KillAvatar), 1.7f);
    }

    /// <summary>
    /// Kills the player (plays the death animation and sound effect, displays end game text, and plays the defeat music).
    /// </summary>
    private void KillAvatar()
    {
        avatar.GetComponent<Avatar>().Die();
    }

    /// <summary>
    /// Plays a randomly selected sound effect from the list of taunts.
    /// </summary>
    private void Taunt()
    {
        if (cheating) {  return; }

        _sound.clip = taunts[Random.Range(0, taunts.Count)];
        _sound.Play();
    }

    /// <summary>
    /// Stops the wizard from moving and plays the casting animation when the player reaches the stairs.
    /// </summary>
    public void GameOver()
    {
        stop = true;
        casting = true;
        _animator.Play("Casting");
    }

    /// <summary>
    /// Plays the scream sound effect. 
    /// Called when the player is cheating or at the end of the game, after the victory music.
    /// </summary>
    public void Scream()
    {
        _sound.clip = scream;
        _sound.Play();
    }
}