using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FireBall : MonoBehaviour
{
    public float dieTime;
    public int damage;
    public int manaCost;
    public GameObject diePEFFECT;
    public AudioSource dieSound;
    GameObject[] skeleton;
    GameObject[] archer;
    GameObject[] hell_hand;
    GameObject boss;
    GameObject player;
    int bossHealth;

    // Start is called before the first frame update
    void Start()
    {
        Physics2D.IgnoreLayerCollision(7, 13);
        player = GameObject.Find("Player");
        damage = player.GetComponent<PlayerController>().atkDMG * 2;
        manaCost = 30;
        skeleton = GameObject.FindGameObjectsWithTag("Skeleton");
        archer = GameObject.FindGameObjectsWithTag("Archer");
        hell_hand = GameObject.FindGameObjectsWithTag("Hell_Hand");
        boss = GameObject.FindGameObjectWithTag("Boss");
        bossHealth = boss.GetComponent<Boss>().currentHealth;
    }

    // Update is called once per frame
    void OnCollisionEnter2D(Collision2D col)
    {
        Die();
        foreach (GameObject skele in skeleton)
        {
            float health = skele.GetComponent<Skeleton>().currentHealth;
            if (health > 0 && col.collider.name.Equals(skele.GetComponent<Skeleton>().bodyCollider.name))
            {
                skele.GetComponent<Skeleton>().TakeDamage(damage);
            }
        }
        foreach (GameObject arc in archer)
        {
            float health = arc.GetComponent<Archer>().currentHealth;
            if (health > 0 && col.collider.name.Equals(arc.GetComponent<Archer>().bodyCollider.name))
            {
                arc.GetComponent<Archer>().TakeDamage(damage);
            }
        }
        foreach (GameObject hell in hell_hand)
        {
            float health = hell.GetComponent<Hell_Hand>().currentHealth;
            if (health > 0 && col.collider.name.Equals(hell.GetComponent<Hell_Hand>().bodyCollider.name))
            {
                hell.GetComponent<Hell_Hand>().TakeDamage(damage);
            }
        }
        if (bossHealth > 0 && col.collider.name.Equals(boss.GetComponent<Boss>().bodyCollider.name))
        {
            boss.GetComponent<Boss>().TakeDamage(damage);
            Debug.Log("Hit");
        }
    }

    void Die()
    {
        dieSound.Play();
        Destroy(gameObject);
    }
}
