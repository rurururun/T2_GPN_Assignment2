using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : MonoBehaviour
{
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
    public LayerMask monster;
    public Collider2D bodyCollider;
    public Rigidbody2D skeleton;

    // Array of colliders of the monsters
    private Collider2D[] monsters;

    // Variables for detecting player
    public Transform player;
    public LayerMask playerLayer;
    public Collider2D playerCollider;
    float playerHealth;
    float distToPlayer;
    public int range;

    // Variables for monster stats
    public int maxHealth;
    public int currentHealth;
    public int atk;
    float atkSpeed = 1;
    bool canAttack;
    //bool hurt;

    // Animator for monster
    public Animator bossAnimator;

    // Start is called before the first frame update
    void Start()
    {
        // Initializing the monster
        mustPatrol = true;
        skeleton = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player").transform;
        currentHealth = maxHealth;
        canAttack = true;
        //hurt = false;
    }

    // Update is called once per frame
    void Update()
    {
        // Setting walking/running animation of monster based on the condition mustPatrol
        bossAnimator.SetBool("IsRunning", mustPatrol);

        // Getting player's health to see whether player is dead or alive
        playerHealth = playerCollider.GetComponent<PlayerController>().currentHealth;

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

            // Checking whether player is within the monster's attack range,
            // If it is then the monster will attack the player
            if (Physics2D.OverlapCircle(attackPos.position, 0.1f, playerLayer) && canAttack)
            {
                // Forces monster to stop moving when attacking
                mustPatrol = false;
                skeleton.velocity = Vector2.zero;

                // Dealing damage
                StartCoroutine(Attack());
            }
        }
        else if (currentHealth <= 0 || playerHealth <= 0)
        {
            mustPatrol = false;
            skeleton.velocity = Vector2.zero;
        }
        else
        {
            mustPatrol = true;
        }

        // Checking for the different stages of the boss
        if (currentHealth <= maxHealth * 0.3)
        {
            atk += 90;
            atkSpeed = 0.25f;
        }
        else if (currentHealth <= maxHealth * 0.7)
        {
            atk += 60;
            atkSpeed = 0.5f;
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
        // Getting the list of monsters that it has came into contact with
        monsters = Physics2D.OverlapCircleAll(objectCheckPos.position, 0.1f, monster);

        // Checking if monster needs to turn
        if (mustTurn || bodyCollider.IsTouchingLayers(obstacles))
        {
            Flip();
        }

        // Allowing monsters to walk past each other
        if (monsters.Length != 0)
        {
            foreach (Collider2D enemy in monsters)
            {
                if (bodyCollider.IsTouching(enemy))
                {
                    Physics2D.IgnoreCollision(bodyCollider, enemy);
                }
            }
        }

        // Setting the movement of the monster
        skeleton.velocity = new Vector2(walkSpeed * Time.fixedDeltaTime, skeleton.velocity.y);
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

        playerCollider.GetComponent<PlayerController>().TakeDamage(atk);

        yield return new WaitForSeconds(atkSpeed);

        canAttack = true;
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
            Die();
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
        player.GetComponent<PlayerController>().currentHealth += 20;

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
