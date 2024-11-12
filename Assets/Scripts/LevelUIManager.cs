using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LevelUIManager : MonoBehaviour
{
    public TMP_Text goalText;
    public TMP_Text moveText;

    public void SetGoalText(string goal)
    {
        goalText.text = goal;
    }

    public void SetMoveText(int moves)
    {
        moveText.text = moves.ToString();
    }
}
