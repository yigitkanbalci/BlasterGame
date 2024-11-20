using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stone : MonoBehaviour
{
    public int x, y;

    public void TakeDamage()
    {
        // Stone takes damage only from TNT
        Destroy(gameObject);
    }

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

