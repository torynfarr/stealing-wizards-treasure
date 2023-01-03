using TMPro;
using UnityEngine;

/// <summary>
/// This Unity script component is attached to the Stairs tilemap and individual Treasure gameobjects in the scene hierarchy.
/// It handles collision between the player and the treasure chests or stairs.
/// </summary>
public class CollisionDetection : MonoBehaviour
{
    [Tooltip("The TextMeshPro script component for the loot label (the word 'Loot')")]
    [SerializeField] private TextMeshProUGUI lootLabel;

    [Tooltip("The TextMeshPro script component which displays the current score.")]
    [SerializeField] private TextMeshProUGUI lootAmount;

    [Tooltip("The TextMeshPro script component which displays the end game text.")]
    [SerializeField] private TextMeshProUGUI gameOver;

    [Tooltip("The audio source used to play a soundeffect when collision occurs.")]
    [SerializeField] private AudioSource sound;

    [Tooltip("The audio source used to play background music.")]
    [SerializeField] private AudioSource music;

    [Tooltip("The victory music played when the player reaches the stairs.")]
    [SerializeField] private AudioClip victory;

    [Tooltip("The point value of an individual treasure chest.")]
    [SerializeField] private int treasureValue;

    [Tooltip("The Wizard script component attached to the Wizard gameobject.")]
    [SerializeField] private Wizard wizard;

    /// <summary>
    /// Triggered collision event when the player collides with the treasure chests or stairs.
    /// </summary>
    /// <param name="collider"><The 2D collider which the player has collided with./param>
    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (gameObject.CompareTag("Stairs"))
        {
            collider.gameObject.SetActive(false);
            music.Stop();
            sound.Play();
            Invoke(nameof(GameOver), 1.5f);
        }

        if (gameObject.CompareTag("Treasure"))
        {
            Destroy(gameObject);
            int score = int.Parse(lootAmount.text) + treasureValue;
            lootAmount.text = score.ToString();
            sound.Play();
        }
    }

    /// <summary>
    /// Displays end game text and plays end game victory music when the player reaches the stairs.
    /// </summary>
    private void GameOver ()
    {
        wizard.GameOver();
        lootLabel.gameObject.SetActive(false);
        lootAmount.gameObject.SetActive(false);
        gameOver.gameObject.SetActive(true);
        gameOver.text = "You Win!\n$" + lootAmount.text;
        music.loop = false;
        music.clip = victory;
        music.Play();
        Invoke(nameof(WizardScream), 15.0f);
    }

    /// <summary>
    /// After the victory music finishes playing, the Wizard is instructed to scream.
    /// </summary>
    private void WizardScream()
    {
        wizard.GetComponent<Wizard>().Scream();
    }
}
