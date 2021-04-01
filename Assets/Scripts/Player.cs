using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MovingObject
{
    public int wallDamage = 1;  // Damage to walls
    public int pointsPerFood = 10;
    public int pointsPerSoda = 20;
    public float restartLevelDelay = 1f;
    public Text foodText;

    public AudioClip moveSound1;
    public AudioClip moveSound2;
    public AudioClip eatSound1;
    public AudioClip eatSound2;
    public AudioClip drinkSound1;
    public AudioClip drinkSound2;
    public AudioClip gameOverSound;

    private Animator animator;
    private int food;

    // Start overrides the Start function of MovingObject
    protected override void Start() {
        animator = GetComponent<Animator>();

        food = GameManager.instance.playerFoodPoints;

        foodText.text = $"Food: {food}";
        base.Start();
    }


    // When Player object is disabled, store the current local food total in the GameManager so it can be re-loaded in next level.
    private void OnDisable() {
        GameManager.instance.playerFoodPoints = food;
    }

    // CheckIfGameOver checks if the player is out of food points and if so, ends the game.
    private void CheckifGameOver() {
        if (food <= 0) {
            SoundManager.instance.PlaySingle(gameOverSound);
            SoundManager.instance.musicSource.Stop();

            GameManager.instance.GameOver();
        }
    }

    // AttemptMove overrides the AttemptMove function in the base class MovingObject
    // AttemptMove takes a generic parameter T which for Player will be of the type Wall, it also takes integers for x and y direction to move in.
    protected override void AttemptMove<T>(int xDir, int yDir) {
        food--;
        foodText.text = $"Food: {food}";

        // Call the AttemptMove method of the base class, passing in the component T (in this case Wall) and x and y direction to move.

        base.AttemptMove<T>(xDir, yDir);

        // Hit allows us to reference the result of the Linecast done in Move.
        RaycastHit2D hit;

        if (Move(xDir, yDir, out hit)) {
            SoundManager.instance.RandomizeSfx(moveSound1, moveSound2);
        }

        CheckifGameOver();

        GameManager.instance.playersTurn = false;
    }
    // OnCantMove overrides the abstract function OnCantMove in MovingObject.
    // It takes a generic parameter T which in the case of Player is a Wall which the player can attack and destroy.
    protected override void OnCantMove<T>(T component) {
        Wall hitWall = component as Wall;
        hitWall.DamageWall(wallDamage);

        animator.SetTrigger("playerChop");
    }

    // OnTriggerEnter2D is sent when another object enters a trigger collider attached to this object (2D physics only).

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.tag == "Exit") {
            // Invoke the Restart function to start the next level with a delay of restartLevelDelay (default 1 second).
            Invoke("Restart", restartLevelDelay);
            //Disable the player object since level is over.
            enabled = false;
        }
        else if (other.tag == "Food") {
            food += pointsPerFood;
            foodText.text = $"+ {pointsPerFood} Food: + {food}";
            SoundManager.instance.RandomizeSfx(eatSound1, eatSound2);
            other.gameObject.SetActive(false);
        }
        else if (other.tag == "Soda") {
            food += pointsPerSoda;
            foodText.text = $"+ {pointsPerSoda}";
            SoundManager.instance.RandomizeSfx(drinkSound1, drinkSound2);
            other.gameObject.SetActive(false);
        }

    }

    // Restart reloads the scene when called.
    private void Restart() {
        //Load the last scene loaded, in this case Main, the only scene in the game. And we load it in "Single" mode so it replace the existing one
        //and not load all the scene object in the current scene.
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
    }

    // LoseFood is called when an enemy attacks the player.
    public void LoseFood(int loss) {
        animator.SetTrigger("playerHit");  // Player is hit
        food -= loss;
        foodText.text = $"- {loss} Food: {food}";
        CheckifGameOver();
    }

    // Called every frame
    private void Update() {
        if (!GameManager.instance.playersTurn) return;


        int horizontal = 0;  // Used to store the horizontal move direction.
        int vertical = 0;    // Used to store the vertical move direction.

        horizontal = (int)Input.GetAxisRaw("Horizontal");
        vertical = (int)Input.GetAxisRaw("Vertical");

        if (horizontal != 0) {
            vertical = 0;
        }

        if (horizontal != 0 || vertical != 0) {
            AttemptMove<Wall>(horizontal, vertical);  // We expect to move into a wall
        }
    }
}
