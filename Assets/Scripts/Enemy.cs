using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public Animator animator;
    public GameObject pointA;
    public GameObject pointB;
    private Rigidbody2D rb;
    private Transform currentPoint;
    public float speed;
    private GameObject flea;
    public float hp = 10;
    private bool firstFlip = true;
    private bool isDead = false;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentPoint = pointB.transform;
        flea = GameObject.FindGameObjectWithTag("Player");
    }

    // Update is called once per frame
    void Update()
    {
        if (isDead) { return; }
        EnemyMovement();

        if (hp <= 0)
        {
            isDead = true;
            if (animator != null) { animator.Play("Death"); }
            //enemy dies, drops blood
        }
    }

    void EnemyMovement()
    {
        if (currentPoint == pointB.transform)
        {
            rb.velocity = new Vector2(speed, 0);
        }
        else
        {
            rb.velocity = new Vector2(-speed, 0);
        }

        if (Vector2.Distance(transform.position, currentPoint.position) < 0.5f)
        {
            currentPoint = (currentPoint == pointA.transform) ? pointB.transform : pointA.transform;
            gameObject.GetComponent<SpriteRenderer>().flipX = !gameObject.GetComponent<SpriteRenderer>().flipX;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if  (collision.gameObject.CompareTag("ProjectileCollider"))
        {
            hp -= 5;
        }
    }

    private void DestroyEnemy()
    {
        GameObject.Destroy(this.gameObject.transform.parent);
    }
}
