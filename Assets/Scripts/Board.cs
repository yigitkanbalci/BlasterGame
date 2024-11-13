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
    private LevelUIManager levelUIManager;


    void Start()
    {
        Debug.Log("Board Start");
        levelLoader = FindObjectOfType<LevelLoader>();
        levelUIManager = FindObjectOfType<LevelUIManager>();

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

        RectTransform boardRect = GetComponent<RectTransform>();

        // Padding values for tiles and rows
        float horizontalPadding = 5f; // Space between tiles horizontally
        float verticalPadding = 5f;   // Space between rows vertically

        // Calculate the tile size considering the padding
        float tileSize = Mathf.Min(
            (boardRect.rect.width - (horizontalPadding * (width - 1))) / width,
            (boardRect.rect.height - (verticalPadding * (height - 1))) / height
        );

        allCubes = new Cube[width, height];

        for (int y = 0; y < height; y++)
        {
            // Instantiate a row
            GameObject row = Instantiate(rowPrefab, transform);
            if (row == null)
            {
                Debug.LogError($"Failed to instantiate row at index {y}");
                continue;
            }

            row.name = $"Row_{y + 1}";

            RectTransform rowRect = row.GetComponent<RectTransform>();
            rowRect.sizeDelta = new Vector2(boardRect.rect.width, tileSize);

            // Add vertical padding to the row's position
            float rowOffsetY = -y * (tileSize + verticalPadding);
            rowRect.anchoredPosition = new Vector2(0, rowOffsetY);

            // Set the sibling index to ensure rows are rendered in the correct order
            row.transform.SetSiblingIndex(y); // Assign each row its correct sibling index based on its loop iteration

            // Optional: Use Canvas sorting for extra control
            Canvas rowCanvas = row.GetComponent<Canvas>();
            if (rowCanvas == null)
            {
                rowCanvas = row.AddComponent<Canvas>(); // Add Canvas if not already present
            }

            rowCanvas.overrideSorting = true;
            rowCanvas.sortingOrder = height - y; // Rows at the top get higher sorting order

            for (int x = 0; x < width - 1; x++)
            {
                // Instantiate a tile inside the row
                GameObject tile = Instantiate(tilePrefab, row.transform);
                if (tile == null)
                {
                    Debug.LogError($"Failed to instantiate tile at ({x}, {y})");
                    continue;
                }

                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileSize, tileSize);

                // Add horizontal padding to the tile's position
                float xOffset = x * (tileSize + horizontalPadding);
                tileRect.anchoredPosition = new Vector2(xOffset, 0);

                tile.name = $"Tile_{x + 1}_{y + 1}";

                // Initialize Cube if necessary
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
                }
            }
        }

        Debug.Log("Board initialization complete with proper sibling indices and sorting.");
    }









    void LoadLevel(LevelData levelData)
    {
        width = levelData.grid_width;
        height = levelData.grid_height;
        moveCount = levelData.move_count;
        allCubes = new Cube[width, height];
        levelUIManager.SetMoveText(moveCount);
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
        { // Initial position above the board


            // Set the Image component to stretch
            Image itemImage = item.GetComponent<Image>();
            if (itemImage != null)
            {
                RectTransform itemRect = itemImage.rectTransform;

                // Set anchor points (this won't affect the tween)
                itemRect.anchorMin = Vector2.zero; // Bottom-left corner
                itemRect.anchorMax = Vector2.one;  // Top-right corner

                // Set padding (if needed, adjust these values)
                float padding = 0.5f;
                itemRect.offsetMin = new Vector2(padding, padding);   // Add padding from bottom-left
                itemRect.offsetMax = new Vector2(-padding, -padding); // Add padding from top-right

                // Ensure the starting position is animated
                itemRect.anchoredPosition = new Vector2(0, 100); // Start off-screen or above the board
                itemRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBounce); // Animate to its final position
            }

            //itemRect.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutBounce); // Animate to the correct position

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
