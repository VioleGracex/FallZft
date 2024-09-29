using UnityEngine;
using TMPro;
using UnityEngine.EventSystems; // Include this for the Event System

public class FinishLine : MonoBehaviour
{
    public TextMeshProUGUI victoryText; // Reference to the TextMeshPro text for victory message
    [SerializeField] GameObject restartBtn; // Reference to the restart button

    [SerializeField] GameManager gameManager;

    private float startTime;

    private void Start()
    {
        startTime = Time.time; // Record the start time when the game starts
        victoryText.gameObject.SetActive(false); // Hide the victory text initially


    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player")) // Check if the object that entered the trigger is the player
        {
            DisplayVictoryMessage();
        }
    }

    private void DisplayVictoryMessage()
    {
        // Calculate the finish time
        float finishTime = Time.time - startTime;

        // Display the victory message
        victoryText.text = "Победа!\nВремя: " + finishTime.ToString("F2") + " seconds";
        victoryText.gameObject.SetActive(true); // Show the victory text
        gameManager.OnPlayerWin();
        Time.timeScale = 0; // Pause the game

        // Set the EventSystem to select the restart button
        EventSystem.current.SetSelectedGameObject(restartBtn);
    }
}
