using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    public GameObject pauseMenuUI; // UI for the pause menu
    private bool isPaused = false;

    private PlayerControls playerInput;
    private InputAction pauseAction;

    private bool isGameOver = false;

    [SerializeField] GameObject resumeBtn;

    private void Awake()
    {
        // Initialize PlayerInput and get the pause action from the Input System
        playerInput = new PlayerControls();
        pauseAction = playerInput.UI.Pause;

        // Subscribe to the pause action
        pauseAction.performed += _ => TogglePauseMenu();
    }

    private void OnEnable()
    {
        pauseAction.Enable(); // Enable the input action when the script is enabled
    }

    private void OnDisable()
    {
        pauseAction.Disable(); // Disable the input action when the script is disabled
    }

    public void TogglePauseMenu()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void PauseGame()
    {
        pauseMenuUI.SetActive(true);  // Activate the pause menu
        Time.timeScale = 0;           // Freeze the game
        isPaused = true;
        resumeBtn.SetActive(!isGameOver);
    }

    public void ResumeGame()
    {
        pauseMenuUI.SetActive(false); // Deactivate the pause menu
        Time.timeScale = 1;           // Resume the game
        isPaused = false;
    }

    public void RestartGame()
    {
        Time.timeScale = 1; // Resume the game
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex); // Reload the current scene
    }

    public void QuitGame()
    {
        Time.timeScale = 1; // Resume time before quitting

        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false; // If running in the editor, stop playing
        #else
            Application.Quit(); // If running in a build, quit the application
        #endif
    }

    // Call this method when the player dies
    public void OnPlayerDeath()
    {
        isGameOver = true;
        PauseGame(); // Automatically pause when the player dies
    }

    public void OnPlayerWin()
    {
        isGameOver = true;
        PauseGame(); // Automatically pause when the player wins
    }
}
