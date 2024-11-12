using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Box : MonoBehaviour
{
    public int x, y;

    public void TakeDamage()
    {
        // Box takes one damage and is destroyed
        Debug.Log("Box at: " + x + ", " + y + " took damage.");
        Destroy(gameObject);
    }
}