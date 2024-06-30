using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FleaCollider : MonoBehaviour
{
    [SerializeField] public int hp;
    [SerializeField] public float knockbackForce;
    private bool invulnerable = false;
    public float invulnerableTime;
    private Rigidbody2D rb;

    void Start()
    {
        rb = transform.parent.gameObject.GetComponent<Rigidbody2D>();
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
            Debug.Log("Collide");
            hp -= 5;
            StartCoroutine("SetInvulnerable");
            
            //Knockback
            Vector2 difference = (transform.position - other.transform.position).normalized;
            Vector2 force = difference * knockbackForce;
            rb.AddForce(force, ForceMode2D.Impulse);
        }
        
    }
}
