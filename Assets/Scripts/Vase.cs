using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vase : MonoBehaviour
{
    public int x, y;
    private int damageCount = 0;

    public void TakeDamage()
    {
        damageCount++;
        if (damageCount >= 2)
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(int x, int y)
    {
        this.x = x;
        this.y = y;
    }
}

