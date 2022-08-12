using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Skeleton : MonoBehaviour
{
    // Variable for displaying dmg taken
    public TextMeshProUGUI dmgTaken;
    public TextMeshProUGUI goldGranted;
    public TextMeshProUGUI expGranted;
    public Image healthBar;

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
    public Rigidbody2D skeleton;
    GameObject[] skele;
    GameObject[] archer;
    GameObject[] hell;

    // Variables for detecting player
    private Transform player;
    public LayerMask playerLayer;
    private Collider2D playerCollider;
    float playerHealth;
    float distToPlayer;
    public int range;

    // Variables for monster stats
    public float maxHealth { get; private set; }
    public float currentHealth { get; private set; }
    int atk = 15;
    bool canAttack;
    bool hurt;

    // Animator for monster
    public Animator skeletonAnimator;

    public AudioSource swing;
    public AudioSource death;

    // Start is called before the first frame update
    void Start()
    {
        // Initializing the monster
        maxHealth = 25f;
        healthBar.enabled = false;
        dmgTaken.enabled = false;
        goldGranted.enabled = false;
        expGranted.enabled = false;
        mustPatrol = true;
        skeleton = GetComponent<Rigidbody2D>();
        player = GameObject.Find("Player").transform;
        playerCollider = GameObject.Find("Player").GetComponent<PlayerController>().bodyCollider;
        currentHealth = maxHealth;
        canAttack = true;
        hurt = false;
        skele = GameObject.FindGameObjectsWithTag("Skeleton");
        archer = GameObject.FindGameObjectsWithTag("Archer");
        hell = GameObject.FindGameObjectsWithTag("Hell_Hand");
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth > 0)
        {
            healthBar.transform.position = new Vector2(skeleton.transform.position.x, skeleton.transform.position.y + 10);
            healthBar.fillAmount = currentHealth / maxHealth;
        }

        // Setting walking/running animation of monster based on the condition mustPatrol
        skeletonAnimator.SetBool("IsRunning", mustPatrol);

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
            if (Physics2D.OverlapCircle(attackPos.position, 0.1f, playerLayer) && canAttack && !hurt)
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

        // Allowing monsters to walk past each other
        foreach (GameObject g in skele)
        {
            Physics2D.IgnoreCollision(bodyCollider, g.GetComponent<Skeleton>().bodyCollider);
        }
        foreach (GameObject g in archer)
        {
            Physics2D.IgnoreCollision(bodyCollider, g.GetComponent<Archer>().bodyCollider);
        }
        foreach (GameObject g in hell)
        {
            Physics2D.IgnoreCollision(bodyCollider, g.GetComponent<Hell_Hand>().bodyCollider);
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
        skeletonAnimator.SetTrigger("Attack");

        swing.Play();

        yield return new WaitForSeconds(1);

        playerCollider.GetComponent<PlayerController>().TakeDamage(atk);

        yield return new WaitForSeconds(1);

        canAttack = true;
    }

    IEnumerator Hurt()
    {
        hurt = true;

        yield return new WaitForSeconds(2);

        hurt = false;
    }

    IEnumerator DisplayDmgTaken(int damage)
    {
        dmgTaken.text = "- " + damage.ToString() + " HP";
        dmgTaken.transform.position = new Vector2(skeleton.transform.position.x + 5, skeleton.transform.position.y + 15);
        dmgTaken.enabled = true;

        yield return new WaitForSeconds(1);

        dmgTaken.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;
        healthBar.enabled = true;

        // Hurt animation
        skeletonAnimator.SetTrigger("Hurt");

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
            StartCoroutine(Hurt());
        }
    }

    IEnumerator DisplayThingsGranted()
    {
        goldGranted.text = "+ " + 100 + "G";
        expGranted.text = "+ " + 100 + "EXP";
        goldGranted.enabled = true;
        expGranted.enabled = true;

        yield return new WaitForSeconds(1);

        goldGranted.enabled = false;
        expGranted.enabled = false;
    }

    void Die()
    {
        healthBar.enabled = false;

        death.Play();

        CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");

        // Death animation
        skeletonAnimator.SetBool("IsDead", true);

        // Disables the monster collider
        GetComponent<Collider2D>().enabled = false;

        // Heals the player
        player.GetComponent<PlayerController>().currentHealth += 10;
        if (player.GetComponent<PlayerController>().currentHealth > character.health)
        {
            player.GetComponent<PlayerController>().currentHealth = character.health;
        }

        // Gives player exp and gold
        player.GetComponent<PlayerController>().exp += 100;
        player.GetComponent<PlayerController>().gold += 100;
        character.experience += 100;
        character.gold += 100;
        DataHandler.SaveToJSON(character, "CharacterAttribute");

        StartCoroutine(DisplayThingsGranted());

        // Quest
        Quest currentQuest = player.GetComponent<PlayerController>().quest1;
        if (currentQuest.archiveAmount < currentQuest.objectiveAmount && currentQuest.questTitle == "Warrior Skeleton Invasion!")
        {
            List<Quest> questList = DataHandler.ReadListFromJSON<Quest>("Quest");
            for (int i = 0; i < questList.Count; i++)
            {
                if (questList[i].questTitle == currentQuest.questTitle)
                {
                    questList[i].archiveAmount += 1;
                    player.GetComponent<PlayerController>().quest1.archiveAmount += 1;
                    break;
                }
            }
            DataHandler.SaveToJSON(questList, "Quest");
        }

        // Monster revives after a set amount of time
        StartCoroutine(MonsterRespawn());
    }

    IEnumerator MonsterRespawn()
    {
        yield return new WaitForSeconds(20);

        // Disables the death animation
        skeletonAnimator.SetBool("IsDead", false);

        // Enables the monster collider
        GetComponent<Collider2D>().enabled = true;

        // Set the condition of monster to before death
        currentHealth = maxHealth;
        mustPatrol = true;
    }
}
