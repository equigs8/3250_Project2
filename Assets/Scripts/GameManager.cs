using InfimaGames.LowPolyShooterPack;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // The Singleton instance allows other scripts to access this easily
    public static GameManager Instance;

    [Header("Game Settings")]
    public int killsToWin = 10;
    //public string winSceneName = "WinMenu"; // Change this to your exact scene name
    //public GameObject winMenu;

    [Header("Current Game Stats")]
    

    public int currentKills = 0;
    private GameObject player;
    private void Awake()
    {
        SceneManager.sceneLoaded += SceneLoaded;
        // Singleton Setup: Ensure only one GameManager exists
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Debug.LogWarning("Multiple GameManagers found! Destroying duplicate.");
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= SceneLoaded;
    }

    void SceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Level1")
        {
            player = GameObject.Find("Player");
        }
    }

    public void Update()
    {
        if (player == null) return;
        if(player.GetComponent<Health>().GetCurrentHealth() <= 0){
            TriggerLoseCondition();
        }
    }
    // This method gets called by the enemy when it dies
    public void AddKill()
    {
        currentKills++;
        Debug.Log($"Enemy eliminated. Current Kills: {currentKills} / {killsToWin}");

        if (currentKills >= killsToWin)
        {
            TriggerWinCondition();
        }
        
    }

    private void TriggerWinCondition()
    {
        Debug.Log("Win Condition Met! Loading next level...");
        //set timescale to 0
        Time.timeScale = 0;
        
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameObject.Find("Player").GetComponentInChildren<CameraLook>().enabled = false;

        GetWinMenu().transform.GetChild(0).gameObject.SetActive(true);
        AssignButtons();
        
    }
    public void TriggerLoseCondition()
    {
        Debug.Log("Lose Condition Met! Loading next level...");
        //set timescale to 0
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        GameObject.Find("Player").GetComponentInChildren<CameraLook>().enabled = false;

        GetLoseMenu().transform.GetChild(0).gameObject.SetActive(true);
        AssignButtons(false);
    }

    public void AssignButtons(bool winMenu = true)
    {
        if (winMenu){
            //winMenu/0. Background/1. ButtonContainer/0 - 2 buttons
            GetWinMenu().transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(RestartLevel);
            GetWinMenu().transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(MainMenu);
            GetWinMenu().transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<Button>().onClick.AddListener(ExitGame);
        }
        else
        {
            GetLoseMenu().transform.GetChild(0).GetChild(1).GetChild(0).GetComponent<Button>().onClick.AddListener(RestartLevel);
            GetLoseMenu().transform.GetChild(0).GetChild(1).GetChild(1).GetComponent<Button>().onClick.AddListener(MainMenu);
            GetLoseMenu().transform.GetChild(0).GetChild(1).GetChild(2).GetComponent<Button>().onClick.AddListener(ExitGame);
        }
    }

    public GameObject GetWinMenu()
    {
        return GameObject.Find("WinMenu");
    }
    public GameObject GetLoseMenu()
    {
        return GameObject.Find("LoseMenu");
    }
    public void RestartLevel()
    {
        Debug.Log("Restarting level...");
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        
    }

    public void ExitGame()
    {
        Debug.Log("Exiting game...");
        ResetGameManager();
        Application.Quit();
    }

    public void MainMenu()
    {
        Debug.Log("Returning to main menu...");
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
        ResetGameManager();
    }

    public void PlayGame()
    {
        SceneManager.LoadScene("Level1");
    }
    public void ResetGameManager()
    {
        currentKills = 0;
    }

    public void TogglePage(GameObject page)
    {
        Debug.Log("Toggling page: " + page.name);
        page.transform.GetChild(0).gameObject.SetActive(!page.transform.GetChild(0).gameObject.activeSelf);
    }
}