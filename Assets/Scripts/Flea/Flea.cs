using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flea : MonoBehaviour
{
    public int hp;
    private bool invulnerable = false;
    public float invulnerableTime;
    private Rigidbody2D rb;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (hp <= 0)
        {
            //dies, offer to restart?
        }
    }

    IEnumerable SetInvulnerable()
    {
        invulnerable = true;
        yield return new WaitForSeconds(invulnerableTime);
        invulnerable = false;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Enemy") && !invulnerable)
        {
            hp -= 5;
            StartCoroutine("SetInvulnerable");
        }
        rb.AddForce(new Vector2(-2, 2));
    }
}
