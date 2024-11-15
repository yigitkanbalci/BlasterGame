using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using TMPro;
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
    public Goal[] goals;

    public Sprite boxSprite;
    public Sprite vaseSprite;
    public Sprite stoneSprite;

    public Transform goalContainer;
    public GameObject goalItemPrefab;

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

        // Initialize rows from bottom to top
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

            // Explicitly set Z-position for rows to control depth
            float zPosition = y * -0.1f; // Rows lower in hierarchy are rendered behind
            row.transform.localPosition = new Vector3(0, 0, zPosition);

            // Calculate vertical position for bottom-to-top layout
            float rowOffsetY = y * (tileSize + verticalPadding);
            rowRect.anchoredPosition = new Vector2(0, rowOffsetY);

            // Set sibling index for bottom-to-top hierarchy

            string sortingLayerName = $"Row_{y + 1}";
            Renderer rowRenderer = row.GetComponent<Renderer>();
            if (rowRenderer != null)
            {
                rowRenderer.sortingLayerName = sortingLayerName;
                rowRenderer.sortingOrder = y;
            }

            // Initialize tiles and cubes for this row
            for (int x = 0; x < width - 1; x++)
            {
                GameObject tile = Instantiate(tilePrefab, row.transform);
                if (tile == null)
                {
                    Debug.LogError($"Failed to instantiate tile at ({x}, {y})");
                    continue;
                }

                RectTransform tileRect = tile.GetComponent<RectTransform>();
                tileRect.sizeDelta = new Vector2(tileSize, tileSize);

                // Calculate horizontal position for left-to-right layout
                float xOffset = x * (tileSize + horizontalPadding);
                tileRect.anchoredPosition = new Vector2(xOffset, 0);

                tile.name = $"Tile_{x + 1}_{y + 1}";

                // Initialize a cube inside this tile
                Cube cube = tile.GetComponent<Cube>();
                if (cube == null)
                {
                    cube = tile.AddComponent<Cube>();
                }

                if (cube != null)
                {
                    cube.x = x;
                    cube.y = y; // Correct y-index for bottom-to-top layout
                    allCubes[x, y] = cube;
                }
            }

            Canvas rowCanvas = row.GetComponent<Canvas>();
            if (rowCanvas != null)
            {
                rowCanvas.overrideSorting = true;
                rowCanvas.sortingOrder = height - y; // Rows higher up have higher sortingOrder
            }

            if (row.GetComponent<GraphicRaycaster>() == null)
            {
                row.AddComponent<GraphicRaycaster>();
            }
        }

        Debug.Log("Board initialized with explicit Z-order and hierarchy.");
    }




    Goal[] CalculateGoals(string[] grid)
    {
        // Create a dictionary to store counts for each goal type
        Dictionary<string, int> goalCounts = new Dictionary<string, int>
    {
        { "bo", 0 }, // Box
        { "s", 0 },  // Stone
        { "v", 0 }   // Vase
    };

        // Iterate through the grid and count occurrences
        foreach (string item in grid)
        {
            if (goalCounts.ContainsKey(item))
            {
                goalCounts[item]++;
            }
        }

        // Convert dictionary to Goal array
        List<Goal> goals = new List<Goal>();
        foreach (var pair in goalCounts)
        {
            if (pair.Value > 0)
            {
                goals.Add(new Goal { goalType = pair.Key, count = pair.Value });
            }
        }

        return goals.ToArray();
    }

    Sprite GetGoalSprite(string goalType)
    {
        switch (goalType)
        {
            case "bo": return boxSprite;   // Box sprite
            case "s": return stoneSprite; // Stone sprite
            case "v": return vaseSprite;  // Vase sprite
            default: return null;
        }
    }

    void PopulateGoals(Goal[] goals)
    {
        foreach (Transform child in goalContainer)
        {
            Destroy(child.gameObject); // Clear any existing UI
        }

        foreach (Goal goal in goals)
        {
            GameObject goalItem = Instantiate(goalItemPrefab, goalContainer);
            goalItem.transform.localPosition = Vector3.zero;
            goalItem.transform.localRotation = Quaternion.identity;
            goalItem.transform.localScale = Vector3.one;
            print(goalItem);
            Image icon = goalItem.GetComponentInChildren<Image>();
            TMP_Text count = goalItem.GetComponentInChildren<TMP_Text>();
            print(goalItem.GetComponent<RectTransform>().anchoredPosition);

            print(icon);
            print(count);
            // Set icon and count dynamically
            icon.sprite = GetGoalSprite(goal.goalType); // Fetch sprite based on goal type
            icon.SetNativeSize();
            RectTransform iconRect = icon.GetComponent<RectTransform>();
            iconRect.localScale = Vector3.one; // Reset scale

            // Ensure it fits within a maximum size
            float maxSize = 50f; // Adjust this value based on your UI design
            float largestDimension = Mathf.Max(iconRect.sizeDelta.x, iconRect.sizeDelta.y);
            if (largestDimension > maxSize)
            {
                float scaleFactor = maxSize / largestDimension;
                iconRect.sizeDelta *= scaleFactor; // Scale down proportionally
            }
            count.text = goal.count.ToString();
        }
    }


    void LoadLevel(LevelData levelData)
    {
        width = levelData.grid_width;
        height = levelData.grid_height;
        moveCount = levelData.move_count;
        allCubes = new Cube[width, height];
        levelUIManager.SetMoveText(moveCount);
        goals = CalculateGoals(levelData.grid);
        for (int i = 0; i < goals.Length; i++)
        {
            print(goals[i].goalType + " : "+ goals[i].count);
        }
        PopulateGoals(goals);
        InitializeBoard();
        SetUpBoard(levelData.grid);
    }

    void SetUpBoard(string[] grid)
    {
        // Iterate over rows in reverse order (top to bottom)
        for (int y = height - 1; y >= 0; y--)
        {
            if (y >= transform.childCount)
            {
                Debug.LogError($"Row {y} is out of bounds for transform child count {transform.childCount}");
                continue;
            }

            Transform row = transform.GetChild(height - 1 - y);

            row.SetSiblingIndex(height - 1 - y);
            // Iterate over columns in reverse order (right to left)
            for (int x = width - 1; x >= 0; x--)
            {
                if (x >= row.childCount)
                {
                    Debug.LogError($"Column {x} in row {y} is out of bounds for row child count {row.childCount}");
                    continue;
                }

                Transform tile = row.GetChild(x); // Get the tile transform
                string itemType = grid[y * width + x];

                // Spawn the item in reverse order
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
            Cube cube = item.GetComponent<Cube>();
            cube.Initialize(x, y, color, sprite);

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
