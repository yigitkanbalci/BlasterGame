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
}

