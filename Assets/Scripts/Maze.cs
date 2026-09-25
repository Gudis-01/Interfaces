using UnityEngine;

public class MazeStatic : MonoBehaviour
{
    public GameObject wallPrefab;
    public float cellSize = 1f;

    // 1 = pared
    // 0 = camino
    //
    // Este es el laberinto que se va a generar.
    private int[,] maze =
    {
        {1,1,1,1,1,1,1,1,1,1,1},
        {1,0,0,0,1,0,0,0,0,0,1},
        {1,0,1,0,1,0,1,1,1,0,1},
        {1,0,1,0,0,0,0,0,1,0,1},
        {1,0,1,1,1,1,1,0,1,0,1},
        {1,0,0,0,0,0,1,0,1,0,1},
        {1,1,1,1,1,0,1,0,1,0,1},
        {1,0,0,0,1,0,1,0,0,0,1},
        {1,0,1,0,1,0,1,1,1,1,1},
        {1,0,1,0,0,0,0,0,0,0,1},
        {1,1,1,1,1,1,1,1,1,1,1}
    };

    void Start()
    {
        GenerateMaze();
    }

    void GenerateMaze()
    {
        if (wallPrefab == null)
        {
            Debug.LogError("No has asignado el Wall Prefab.");
            return;
        }

        int width = maze.GetLength(0);
        int height = maze.GetLength(1);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (maze[x, y] == 1)
                {
                    Vector3 position = new Vector3(
                        x * cellSize,
                        y * cellSize,
                        0
                    );

                    Instantiate(
                        wallPrefab,
                        position,
                        Quaternion.identity,
                        transform
                    );
                }
            }
        }
    }
}
