using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Flea : MonoBehaviour
{
    [SerializeField] public int hp;
    [SerializeField] public float knockbackForce;
    private bool invulnerable = false;
    public float invulnerableTime;
    private Rigidbody2D rb;
    public FleaMovementController MovementController;
    private bool _isDead = false;

    void Start()
    {
        rb = gameObject.GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!_isDead)
        {
            if (hp <= 0)
            {
                //dies, offer to restart?
                _isDead = true;
                MovementController.Death();
            }
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
        if (_isDead) { return; }
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

        /*else if (other.gameObject.CompareTag("Bounce"))
        {
            Vector2 difference = (transform.position - other.transform.position).normalized;
            Vector2 force = difference * knockbackForce * 2.5f;
            rb.AddForce(force, ForceMode2D.Impulse);
        }*/
    }
}
