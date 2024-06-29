using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flea : MonoBehaviour
{
    public int hp;
    private bool invulnerable = false;
    public float invulnerableTime;

    public IEnumerable SetInvulnerable()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
    }

    public void ChangeHp(int amount)
    {
        if (!invulnerable)
        {
            hp += amount;

        }
    }
}
