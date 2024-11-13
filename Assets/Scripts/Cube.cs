using UnityEngine;
using UnityEngine.UI;

public class Cube : MonoBehaviour
{
    public int x, y;
    public Color cubeColor;
    private Board board;
    private Image image;

    private void Awake()
    {
        board = FindObjectOfType<Board>();
        image = GetComponent<Image>();
    }

    private void Start()
    {
        if (image == null)
        {
            image = GetComponent<Image>();
        }

        //Debug.Log("Cube initialized at: " + x + ", " + y + " with color: " + cubeColor);
    }

    public void OnMouseDown()
    {
        Debug.Log("Cube clicked at: " + x + ", " + y + " with color: " + cubeColor);
        if (board != null)
        {
            board.HandleCubeClick(this);
        }
    }

    public void Initialize(int x, int y, Color color, Sprite sprite)
    {
        this.x = x;
        this.y = y;
        this.cubeColor = color;
        if (image == null)
        {
            image = GetComponent<Image>();
        }
        image.color = color;
        image.sprite = sprite;
    }
}
