using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// This Unity script component is attached to the Avatar gameobject in the scene hierarchy.
/// It handles player movement and death as well as toggling the noclip cheat mode on/off.
/// </summary>
public class Avatar : MonoBehaviour
{
    [Tooltip("The movement speed of the player.")]
    [SerializeField] private float speed = 2.5f;

    [Tooltip("The TextMeshPro script component for the loot label (the word 'Loot')")]
    [SerializeField] private TextMeshProUGUI lootLabel;

    [Tooltip("The TextMeshPro script component which displays the current score.")]
    [SerializeField] private TextMeshProUGUI lootAmount;

    [Tooltip("The TextMeshPro script component which displays the end game text.")]
    [SerializeField] private TextMeshProUGUI gameOver;

    [Tooltip("The audio source used to play background music.")]
    [SerializeField] private AudioSource music;

    [Tooltip("The defeat music played when the player is killed by the wizard.")]
    [SerializeField] private AudioClip defeat;

    private bool cheating = false;
    private Vector2 movement;
    private Rigidbody2D _avatar;
    private Animator _animator;
    private AudioSource _sound;

    [Tooltip("A flag to indicate when the player is dead.")]
    public bool Dead = false;

    /// <summary>
    /// Connects to various script components attached to the Avatar gameobject and plays the idle animation clip.
    /// </summary>
    private void Awake()
    {
        _avatar = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();
        _animator.Play("Idle");
        _sound = GetComponent<AudioSource>();
    }

    /// <summary>
    /// Handles player movement (excluding diagonal movement or movement when the player is dead).
    /// </summary>
    private void FixedUpdate()
    {
        if (movement.x != 0 && movement.y != 0 || Dead)
        {
            // Ignore diagonal movement. And don't move if the avatar is dead. Obviously.
            return;
        }
        else
        {
            _avatar.MovePosition(_avatar.position + movement * speed * Time.fixedDeltaTime);
        } 
    }

    /// <summary>
    /// Unity Input System event which handles movement via WASD keyboard input or controller d-pad input.
    /// </summary>
    /// <param name="value"></param>
    private void OnMovement(InputValue value)
    {
        if (Dead) {  return; }

        movement = value.Get<Vector2>();

        if (movement.x == 0 && movement.y == 0) 
        {
            _animator.Play("Idle");
        }
        else
        {
            _animator.Play("Walking");
        }
    }

    /// <summary>
    /// Unity Input System event which handles toggling noclip mode on/off via the backslash key or select button on the controller.
    /// </summary>
    private void OnCheating()
    {
        if (!cheating)
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = false;
            Debug.Log("You're cheating! No clip mode activated!");
            cheating = true;
        }
        else
        {
            gameObject.GetComponent<BoxCollider2D>().enabled = true;
            cheating = false;
        }
    }

    /// <summary>
    /// Plays the death animation and sound effect, displays end game text, and plays the defeat music when the wizard kills the player.
    /// </summary>
    public void Die()
    {
        _sound.Play();
        _animator.Play("Death");
        Dead = true;

        lootLabel.gameObject.SetActive(false);
        lootAmount.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(true);
        gameOver.text = "You Lose!";
        music.loop = false;
        music.clip = defeat;
        music.Play();
    }
}