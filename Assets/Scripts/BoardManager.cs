using System;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class BoardManager : MonoBehaviour
{
    [Serializable]
    public class Count
    {
        public int min { get; set; }
        public int max { get; set; }

        public Count(int min, int max) {
            this.min = min;
            this.max = max;
        }
    }

    public int columns = 8;                     // The amount of columns for the inner area of the board, not counting the outer walls.
    public int rows = 8;                        // The amount of rows for the inner area of the board, not counting the outer walls.

    public Count wallCount = new Count(5, 9);   // Lower and upper limit for our random number of walls per level.
    public Count foodCount = new Count(1, 5);   // Lower and upper limit for our random number of food/soda per level.

    public GameObject exit;                     // Exit prefab
    public GameObject[] floorTiles;             // Floor prefabs
    public GameObject[] wallTiles;              // Wall prefabs
    public GameObject[] foodTiles;              // Food prefabs
    public GameObject[] enemyTiles;             // Enemey prefabs
    public GameObject[] outerWallTiles;         // Outerwall prefabs

    private Transform boardHolder;                              // A variable to store a reference to the transform of our Board object.
    private List<Vector3> gridPositions = new List<Vector3>();  // List of possible locations to place tiles.


    // Clears gridPositions and prepares it to generate a new board.
    private void InitialiseList() {
        gridPositions.Clear();

        for (int x = 1; x < columns - 1; x++) {
            for (int y = 1; y < rows - 1; y++) {
                // At each index add a new Vector3 to our list with the x and y coordinates of that position.
                gridPositions.Add(new Vector3(x, y, 0f));
            }
        }
    }

    // Sets up the outer walls and floor (background) of the game board.
    private void BoardSetup() {
        boardHolder = new GameObject("Board").transform;

        for (int x = -1; x < columns + 1; x++) {
            for (int y = -1; y < rows + 1; y++) {
                // Picks a random floorTile object and prepares to instantiate it.
                GameObject toInstantiate = floorTiles[Random.Range(0, floorTiles.Length)];

                // If we're outside the play-area, i.e. not inside the middle 8x8 grid, place a wall in that position instead.
                if (x == -1 || x == columns || y == -1 || y == rows) {
                    toInstantiate = outerWallTiles[Random.Range(0, outerWallTiles.Length)];
                }
                // Instantiates the prefab at the Vector3 position we've looped to.
                GameObject instance = Instantiate(toInstantiate, new Vector3(x, y, 0f), Quaternion.identity) as GameObject;  // `Quaternion.identity` means no rotation.
                // Set the parent of our newly instantiated object instance to boardHolder, this is just organizational to avoid cluttering hierarchy.
                instance.transform.SetParent(boardHolder);
            }
        }
    }

    // Returns a random position from gridPositions
    private Vector3 RandomPosition() {
        int randomIndex = Random.Range(0, gridPositions.Count);
        // Gets the position from the list gridPositions and sets it to variable randomPosition
        Vector3 randomPosition = gridPositions[randomIndex];
        // `randomPosition`s Vector3 is removed from gridPositions, meaning it can't be used again.
        gridPositions.RemoveAt(randomIndex);

        return randomPosition;
    }

    // LayoutObjectAtRandom accepts an array of game objects to choose from along with a minimum and maximum range for the number of objects to create.
    private void LayoutObjectAtRandom(GameObject[] tileArray, int minimum, int maximum) {
        int objectCount = Random.Range(minimum, maximum + 1);

        for (int i = 0; i < objectCount; i++) {
            Vector3 randomPosition = RandomPosition();
            GameObject tileChoice = tileArray[Random.Range(0, tileArray.Length)];
            Instantiate(tileChoice, randomPosition, Quaternion.identity);
        }
    }


    public void SetupScene(int level) {
        BoardSetup();
        InitialiseList();
        LayoutObjectAtRandom(wallTiles, wallCount.min, wallCount.max);
        LayoutObjectAtRandom(foodTiles, foodCount.min, foodCount.max);
        // Determine number of enemies based on current level number, based on a logarithmic progression
        int enemyCount = (int)Mathf.Log(level, 2f);
        LayoutObjectAtRandom(enemyTiles, enemyCount, enemyCount);
        // Instantiate the exit tile in the upper right hand corner of our game board
        Instantiate(exit, new Vector3(columns - 1, rows - 1, 0f), Quaternion.identity);
    }
}
