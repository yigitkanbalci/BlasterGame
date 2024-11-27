using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TNT : MonoBehaviour
{
    public int x, y;

    public void TakeDamage()
    {
        // Box takes one damage and is destroyed
        Debug.Log("TNT at: " + x + ", " + y + " took damage.");
        Destroy(gameObject);
    }

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}
