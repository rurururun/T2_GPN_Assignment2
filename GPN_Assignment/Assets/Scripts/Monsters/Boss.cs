using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Boss : MonoBehaviour
{
    // Variable for displaying dmg taken
    public TextMeshProUGUI dmgTaken;
    public Image healthBar1;
    public Image healthBar2;

    // Variable for movement
    public int walkSpeed;

    // Variables for patrol
    private bool mustPatrol;
    private bool mustTurn;

    // Variables for detecting collision
    public Transform groundCheckPos;
    public Transform objectCheckPos;
    public Transform attackPos;
    public LayerMask groundLayer;
    public LayerMask obstacles;
    public Collider2D bodyCollider;
    public Rigidbody2D boss;

    // Variables for detecting player
    Transform player;
    public LayerMask playerLayer;
    float playerHealth;
    float distToPlayer;
    int range = 150;

    // Variables for monster stats
    public float maxHealth { get; private set; }
    public float currentHealth { get; private set; }
    int atk = 500;
    float atkSpeed = 1;
    bool canAttack;
    bool canShoot;
    public GameObject fireBall;
    int shootSpeed = 80;

    // Animator for monster
    public Animator bossAnimator;

    public AudioSource fireball;

    // Start is called before the first frame update
    void Start()
    {
        // Initializing the monster
        mustPatrol = true;
        boss = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player").transform;
        canAttack = true;
        canShoot = true;
        maxHealth = 2000;
        currentHealth = maxHealth;
        dmgTaken.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth >= 1000f)
        {
            if (transform.localScale.x > 0)
            {
                healthBar1.transform.position = new Vector2(boss.transform.position.x - 20, boss.transform.position.y - 70);
                healthBar2.transform.position = new Vector2(boss.transform.position.x - 20, boss.transform.position.y - 80);
            }
            else
            {
                healthBar1.transform.position = new Vector2(boss.transform.position.x + 20, boss.transform.position.y - 70);
                healthBar2.transform.position = new Vector2(boss.transform.position.x + 20, boss.transform.position.y - 80);
            }
            float cHP = currentHealth - 1000f;
            healthBar1.fillAmount = cHP / 1000f;
            healthBar2.fillAmount = 1f;
        }
        else if (currentHealth <= 1000f && currentHealth >= 0f)
        {
            if (transform.localScale.x > 0)
            {
                healthBar2.transform.position = new Vector2(boss.transform.position.x - 20, boss.transform.position.y - 80);
            }
            else
            {
                healthBar2.transform.position = new Vector2(boss.transform.position.x + 20, boss.transform.position.y - 80);
            }
            healthBar1.fillAmount = 0f;
            healthBar2.fillAmount = currentHealth / 1000f;
        }

        // Setting walking/running animation of monster based on the condition mustPatrol
        bossAnimator.SetBool("IsRunning", mustPatrol);

        // Getting player's health to see whether player is dead or alive
        playerHealth = player.GetComponent<PlayerController>().currentHealth;

        // Checking if monster should walk/run, if should then calls the Patrol() method
        if (mustPatrol)
        {
            Patrol();
        }

        // Getting distance between monster and player
        distToPlayer = Vector2.Distance(transform.position, player.position);

        // Checking if player is within the aggro range of the monster,
        // If it is then it will chase after the player
        // If the monster or player is dead then this code will not activate
        if (distToPlayer <= range && currentHealth > 0 && playerHealth > 0)
        {
            // Checking whether player is on the left or right side of the monster,
            // Then the monster will turn to the side where the player is at
            if (player.position.x > transform.position.x && transform.localScale.x < 0 || player.position.x < transform.position.x && transform.localScale.x > 0)
            {
                Flip();
            }

            if (currentHealth > 0 && currentHealth < 1000)
            {
                // Forces monster to stop moving when attacking
                mustPatrol = false;
                boss.velocity = Vector2.zero;
            }

            // Checking whether player is within the monster's attack range,
            // If it is then the monster will attack the player
            if (Physics2D.OverlapCircle(attackPos.position, 0.1f, playerLayer) && canAttack && currentHealth > 1000)
            {
                // Forces monster to stop moving when attacking
                mustPatrol = false;
                boss.velocity = Vector2.zero;

                // Dealing damage
                StartCoroutine(Attack());
            }
            else if (canShoot && currentHealth > 0 && currentHealth < 1000)
            {
                // Increase its range
                range = 500;

                // Dealing damage
                StartCoroutine(Fireball());
            }
        }
        else if (currentHealth <= 0 || playerHealth <= 0)
        {
            mustPatrol = false;
            boss.velocity = Vector2.zero;
        }
        else
        {
            mustPatrol = true;
        }
    }

    void FixedUpdate()
    {
        if (mustPatrol)
        {
            // Checking if monster is still on the platform
            mustTurn = !Physics2D.OverlapCircle(groundCheckPos.position, 0.1f, groundLayer);
        }
    }

    void Patrol()
    {
        // Checking if monster needs to turn
        if (mustTurn || bodyCollider.IsTouchingLayers(obstacles))
        {
            Flip();
        }

        // Setting the movement of the monster
        boss.velocity = new Vector2(walkSpeed * Time.fixedDeltaTime, boss.velocity.y);
    }

    void Flip()
    {
        mustPatrol = false;
        transform.localScale = new Vector2(transform.localScale.x * -1, transform.localScale.y);
        walkSpeed *= -1;
        mustPatrol = true;
    }

    IEnumerator Attack()
    {
        canAttack = false;

        // Activates the attack animation of the monster
        bossAnimator.SetTrigger("Attack");

        yield return new WaitForSeconds(atkSpeed);

        player.GetComponent<PlayerController>().TakeDamage(atk);

        yield return new WaitForSeconds(atkSpeed);

        canAttack = true;
    }

    IEnumerator Fireball()
    {
        canShoot = false;

        fireball.Play();

        GameObject newFireBall = Instantiate(fireBall, attackPos.position, Quaternion.identity);

        if (transform.localScale.x > 0)
        {
            newFireBall.GetComponent<Rigidbody2D>().velocity = new Vector2(shootSpeed, 0f);
        }
        else
        {
            newFireBall.transform.localScale = new Vector2(newFireBall.transform.localScale.x * -1, newFireBall.transform.localScale.y);
            newFireBall.GetComponent<Rigidbody2D>().velocity = new Vector2(shootSpeed * -1, 0f);
        }

        yield return new WaitForSeconds(1);

        canShoot = true;
    }

    IEnumerator DisplayDmgTaken(int damage)
    {
        dmgTaken.text = "- " + damage.ToString() + " HP";
        dmgTaken.transform.position = new Vector2(boss.transform.position.x, boss.transform.position.y - 30);
        dmgTaken.enabled = true;

        yield return new WaitForSeconds(1);

        dmgTaken.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Hurt animation
        bossAnimator.SetTrigger("Hurt");

        // Check if monster is dead,
        // If it is dead, then call the Die() method
        if (currentHealth <= 0)
        {
            StartCoroutine(DisplayDmgTaken(damage));
            Die();
        }
        else
        {
            StartCoroutine(DisplayDmgTaken(damage));
        }
    }

    void Die()
    {
        CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");

        // Death animation
        bossAnimator.SetBool("IsDead", true);

        // Disables the monster collider
        GetComponent<Collider2D>().enabled = false;

        // Heals the player
        player.GetComponent<PlayerController>().currentHealth += 10;
        if (player.GetComponent<PlayerController>().currentHealth > character.health)
        {
            player.GetComponent<PlayerController>().currentHealth = character.health;
        }

        // Gives player exp and gold
        player.GetComponent<PlayerController>().exp += 1000;
        player.GetComponent<PlayerController>().gold += 2000;
        character.experience += 1000;
        character.gold += 2000;
        DataHandler.SaveToJSON(character, "CharacterAttribute");

        // Monster revives after a set amount of time
        StartCoroutine(MonsterRespawn());
    }

    IEnumerator MonsterRespawn()
    {
        yield return new WaitForSeconds(20);

        // Disables the death animation
        bossAnimator.SetBool("IsDead", false);

        // Enables the monster collider
        GetComponent<Collider2D>().enabled = true;

        // Set the condition of monster to before death
        currentHealth = maxHealth;
        mustPatrol = true;
    }
}
