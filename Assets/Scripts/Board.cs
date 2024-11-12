using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Board : MonoBehaviour
{
    public GameObject cubePrefab;
    public GameObject boxPrefab;
    public GameObject stonePrefab;
    public GameObject vasePrefab;
    public GameObject rowPrefab;
    public GameObject tilePrefab;
    public int width;
    public int height;
    public int moveCount;
    public Sprite[] cubeSprites;
    public Cube[,] allCubes;

    private LevelLoader levelLoader;


    void Start()
    {
        Debug.Log("Board Start");
        levelLoader = FindObjectOfType<LevelLoader>();
        if (levelLoader != null)
        {
            // Load the desired level
            LevelData levelData = levelLoader.GetLevel(1);
            if (levelData != null)
            {
                LoadLevel(levelData);
            }
            else
            {
                Debug.LogError("Failed to load level data");
            }
        }
        else
        {
            Debug.LogError("LevelLoader is null");
        }
    }

    void InitializeBoard()
    {
        if (rowPrefab == null || tilePrefab == null)
        {
            Debug.LogError("RowPrefab or TilePrefab is not assigned.");
            return;
        }

        allCubes = new Cube[width, height];

        for (int y = 0; y < height; y++)
        {
            GameObject row = Instantiate(rowPrefab, transform);
            if (row == null)
            {
                Debug.LogError("Failed to instantiate row at index " + y);
                continue;
            }
            row.name = "Row_" + (y + 1);
            Debug.Log("Created " + row.name);

            for (int x = 0; x < width - 1; x++)
            {
                GameObject tile = Instantiate(tilePrefab, row.transform);
                if (tile == null)
                {
                    Debug.LogError("Failed to instantiate tile at (" + x + ", " + y + ")");
                    continue;
                }
                tile.name = "Tile_" + (x + 1);
                Debug.Log("Created " + tile.name + " in " + row.name);

                Cube cube = tile.GetComponent<Cube>();
                if (cube == null)
                {
                    cube = tile.AddComponent<Cube>();
                }

                if (cube != null)
                {
                    cube.x = x;
                    cube.y = y;
                    allCubes[x, y] = cube;
                    Debug.Log("Initialized Cube at (" + x + ", " + y + ")");
                }
            }
        }

        Debug.Log("Board Initialization Complete");
    }





    void LoadLevel(LevelData levelData)
    {
        width = levelData.grid_width;
        height = levelData.grid_height;
        moveCount = levelData.move_count;
        allCubes = new Cube[width, height];
        InitializeBoard();
        SetUpBoard(levelData.grid);
    }

    void SetUpBoard(string[] grid)
    {
        for (int y = 0; y < height; y++)
        {
            if (y >= transform.childCount)
            {
                Debug.LogError($"Row {y} is out of bounds for transform child count {transform.childCount}");
                continue;
            }

            Transform row = transform.GetChild(y);
            for (int x = 0; x < width; x++)
            {
                if (x >= row.childCount)
                {
                    Debug.LogError($"Column {x} in row {y} is out of bounds for row child count {row.childCount}");
                    continue;
                }

                Transform tile = row.GetChild(x); // Get the tile transform
                string itemType = grid[y * width + x];
                SpawnItem(x, y, tile, itemType);
            }
        }
    }

    void SpawnItem(int x, int y, Transform tile, string itemType)
    {
        GameObject item = null;
        if (itemType == "r" || itemType == "g" || itemType == "b" || itemType == "y" || itemType == "rand")
        {
            item = Instantiate(cubePrefab, tile);
            Sprite sprite;
            if (itemType == "rand")
            {
                sprite = cubeSprites[Random.Range(0, cubeSprites.Length)];
            }
            else
            {
                int index = System.Array.IndexOf(new string[] { "r", "g", "b", "y" }, itemType);
                sprite = cubeSprites[index];
            }
            Color color = Color.white;
            item.GetComponent<Cube>().Initialize(x, y, color, sprite);
        }
        else if (itemType == "bo")
        {
            item = Instantiate(boxPrefab, tile);
        }
        else if (itemType == "s")
        {
            item = Instantiate(stonePrefab, tile);
        }
        else if (itemType == "v")
        {
            item = Instantiate(vasePrefab, tile);
        }

        if (item != null)
        {
            RectTransform itemRect = item.GetComponent<RectTransform>();
            itemRect.anchoredPosition = new Vector2(0, height * 100); // Initial position above the board
            itemRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBounce); // Animate to the correct position

            if (item.GetComponent<Cube>() != null)
            {
                allCubes[x, y] = item.GetComponent<Cube>();
            }
        }
    }


    public void HandleCubeClick(Cube cube)
    {
        Debug.Log("HandleCubeClick called for cube at: " + cube.x + ", " + cube.y);
        List<Cube> matchingCubes = GetMatchingCubes(cube);
        if (matchingCubes.Count >= 2)
        {
            Debug.Log("Matching cubes found: " + matchingCubes.Count);
            foreach (Cube matchingCube in matchingCubes)
            {
                Debug.Log("Cube to destroy: " + matchingCube.x + ", " + matchingCube.y);
            }
            StartCoroutine(DestroyCubes(matchingCubes));
        }
    }

    List<Cube> GetMatchingCubes(Cube startCube)
    {
        List<Cube> matchingCubes = new List<Cube>();
        Queue<Cube> cubesToCheck = new Queue<Cube>();
        cubesToCheck.Enqueue(startCube);
        Sprite matchSprite = startCube.GetComponent<Image>().sprite;

        while (cubesToCheck.Count > 0)
        {
            Cube current = cubesToCheck.Dequeue();
            if (matchingCubes.Contains(current)) continue;

            matchingCubes.Add(current);
            foreach (Cube neighbor in GetNeighbors(current))
            {
                if (neighbor != null && neighbor.GetComponent<Image>().sprite == matchSprite)
                {
                    cubesToCheck.Enqueue(neighbor);
                }
            }
        }

        return matchingCubes;
    }

    IEnumerable<Cube> GetNeighbors(Cube cube)
    {
        List<Cube> neighbors = new List<Cube>();

        // Check left neighbor
        if (cube.x > 0 && allCubes[cube.x - 1, cube.y] != null)
        {
            neighbors.Add(allCubes[cube.x - 1, cube.y]);
        }
        // Check right neighbor
        if (cube.x < width - 1 && allCubes[cube.x + 1, cube.y] != null)
        {
            neighbors.Add(allCubes[cube.x + 1, cube.y]);
        }
        // Check bottom neighbor
        if (cube.y > 0 && allCubes[cube.x, cube.y - 1] != null)
        {
            neighbors.Add(allCubes[cube.x, cube.y - 1]);
        }
        // Check top neighbor
        if (cube.y < height - 1 && allCubes[cube.x, cube.y + 1] != null)
        {
            neighbors.Add(allCubes[cube.x, cube.y + 1]);
        }

        foreach (var neighbor in neighbors)
        {
            Debug.Log($"Neighbor found at: {neighbor.x}, {neighbor.y}");
        }

        return neighbors;
    }


    IEnumerator DestroyCubes(List<Cube> cubes)
    {
        HashSet<GameObject> objectsToDestroy = new HashSet<GameObject>();

        // Add cubes to be destroyed
        foreach (Cube cube in cubes)
        {
            objectsToDestroy.Add(cube.gameObject);
            allCubes[cube.x, cube.y] = null;
        }

        // Check for adjacent obstacles
        foreach (Cube cube in cubes)
        {
            foreach (Cube neighbor in GetNeighbors(cube))
            {
                if (neighbor != null)
                {
                    Debug.Log($"Checking neighbor at: {neighbor.x}, {neighbor.y}");

                    // Check for obstacles and add them to the destruction list if found
                    Box box = neighbor.GetComponent<Box>();
                    Stone stone = neighbor.GetComponent<Stone>();
                    Vase vase = neighbor.GetComponent<Vase>();

                    if (box != null)
                    {
                        Debug.Log("Box found adjacent to cube at: " + cube.x + ", " + cube.y);
                        objectsToDestroy.Add(box.gameObject);
                    }
                    else if (stone != null)
                    {
                        Debug.Log("Stone found adjacent to cube at: " + cube.x + ", " + cube.y);
                        objectsToDestroy.Add(stone.gameObject);
                    }
                    else if (vase != null)
                    {
                        Debug.Log("Vase found adjacent to cube at: " + cube.x + ", " + cube.y);
                        objectsToDestroy.Add(vase.gameObject);
                    }
                    else
                    {
                        Debug.Log("No obstacle component found on neighbor at: " + neighbor.x + ", " + neighbor.y);
                    }
                }
            }
        }

        // Destroy objects
        foreach (GameObject obj in objectsToDestroy)
        {
            Destroy(obj);
        }

        yield return new WaitForSeconds(0.2f); // Add a short delay before refilling

        // StartCoroutine(FillBoard()); // Uncomment when ready to refill
    }



    //IEnumerator FillBoard()
    //{
    //    for (int x = 0; x < width; x++)
    //    {
    //        for (int y = 0; y < height; y++)
    //        {
    //            if (allCubes[x, y] == null)
    //            {
    //                Transform tile = transform.GetChild(y).GetChild(x); // Get the tile transform
    //                yield return new WaitForSeconds(0.1f);
    //                SpawnItem(x, y, tile);
    //            }
    //        }
    //    }
    //}
}
