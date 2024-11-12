using UnityEngine;
using UnityEngine.UI;

public class LevelUIManager : MonoBehaviour
{
    public Text goalText;
    public Text moveText;

    public void SetGoalText(string goal)
    {
        goalText.text = goal;
    }

    public void SetMoveText(int moves)
    {
        moveText.text = moves.ToString();
    }
}
