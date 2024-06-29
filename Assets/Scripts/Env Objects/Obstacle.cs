using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    public int damage = -5;
    private GameObject flea;
    void Start()
    {
        flea = GameObject.FindGameObjectWithTag("Player");
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.Equals(flea))
        {
            flea.GetComponent<Flea>().ChangeHp(damage);
            flea.GetComponent<Flea>().StartCoroutine("SetInvulnerable");
            flea.GetComponent<Rigidbody2D>().AddForce(new Vector2(-2, 2));
        }
    }
}
