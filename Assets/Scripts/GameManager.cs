using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class GameManager : MonoBehaviour
{
    public float levelStartDelay = 2f;                  // Time to wait before starting level, in seconds.
    public float turnDelay = 0.1f;                      // Delay between each Player turn.
    public int playerFoodPoints = 100;                  // Starting value for Player food points
    public static GameManager instance = null;  // Singleton
    [HideInInspector] public bool playersTurn = true;


    private Text levelText;
    private GameObject levelImage;
    private BoardManager boardScript;
    private int level = 1;
    private List<Enemy> enemies;
    private bool enemiesMoving;
    private bool doingSetup = true;

    // Start is called before the first frame update
    void Awake() {
        if (instance == null) {
            instance = this;
        }
        else if (instance != this) {
            Destroy(gameObject);
        }


        DontDestroyOnLoad(gameObject);
        enemies = new List<Enemy>();
        boardScript = GetComponent<BoardManager>();
        InitGame();
    }

    //this is called only once, and the paramter tell it to be called only after the scene was loaded
    //(otherwise, our Scene Load callback would be called the very first load, and we don't want that)
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static public void CallbackInitialization() {
        //register the callback to be called everytime the scene is loaded
        SceneManager.sceneLoaded += OnLevelFinishedLoading;
    }

    // This is called each time a scene is loaded
    static private void OnLevelFinishedLoading(Scene scene, LoadSceneMode mode) {
        // Add one to our level number
        instance.level++;
        // Call InitGame to initialize our level.
        instance.InitGame();
    }

    // Initialises the game for each level
    private void InitGame() {
        // While True, Player can't move
        doingSetup = true;

        levelImage = GameObject.Find("LevelImage");
        levelText = GameObject.Find("LevelText").GetComponent<Text>();

        levelText.text = "Day " + level;
        levelImage.SetActive(true);
        Invoke("HideLevelImage", 2f);

        enemies.Clear();
        boardScript.SetupScene(level);
    }

    // Hides black image used between levels
    private void HideLevelImage() {
        levelImage.SetActive(false);
        doingSetup = false;
    }

    // Update is called every frame.
    void Update() {
        if (playersTurn || enemiesMoving || doingSetup) {
            return;
        }
        StartCoroutine(MoveEnemies());
    }

    // Call this to add the passed in Enemy to the List of Enemy objects.
    public void AddEnemyToList(Enemy enemy) {
        enemies.Add(enemy);
    }

    // Coroutine to move enemies in sequence.
    private IEnumerator MoveEnemies() {
        enemiesMoving = true;
        // Wait for turnDelay seconds.
        yield return new WaitForSeconds(turnDelay);


        if (enemies.Count == 0) {
            // Wait for turnDelay seconds between moves, replaces delay caused by enemies moving when there are none.
            yield return new WaitForSeconds(turnDelay);
        }

        for (int i = 0; i < enemies.Count; i++) {
            enemies[i].MoveEnemy();
            yield return new WaitForSeconds(enemies[i].moveTime);
        }
        // Once Enemies are done moving, set playersTurn to true so player can move.
        playersTurn = true;
        enemiesMoving = false;
    }

    // Game Over function
    public void GameOver() {
        levelText.text = $"After {level} days, you starved.";
        levelImage.SetActive(true);
        enabled = false;
    }
}
