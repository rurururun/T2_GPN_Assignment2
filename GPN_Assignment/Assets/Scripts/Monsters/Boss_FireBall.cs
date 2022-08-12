using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss_FireBall : MonoBehaviour
{
    int damage;
    public AudioSource dieSound;
    GameObject player;
    float playerHealth;
    GameObject boss;
    Collider2D bossCollider;
    public Collider2D bodyCollider;

    // Start is called before the first frame update
    void Start()
    {
        damage = 500;
        player = GameObject.Find("Player");
        playerHealth = player.GetComponent<PlayerController>().currentHealth;
        boss = GameObject.FindGameObjectWithTag("Boss");
        bossCollider = boss.GetComponent<Boss>().bodyCollider;
        Physics2D.IgnoreCollision(bodyCollider, bossCollider);
    }

    // Update is called once per frame
    void OnCollisionEnter2D(Collision2D col)
    {
        if (playerHealth > 0 && col.collider.name.Equals(player.GetComponent<PlayerController>().bodyCollider.name))
        {
            player.GetComponent<PlayerController>().TakeDamage(damage);
        }
        Die();
    }

    void Die()
    {
        dieSound.Play();
        Destroy(gameObject);
    }
}
