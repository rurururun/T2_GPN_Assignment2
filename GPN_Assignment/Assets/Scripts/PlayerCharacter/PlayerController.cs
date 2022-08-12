using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Variable for displaying dmg taken
    public TextMeshProUGUI dmgTaken;
    public Image healthbar;
    public Image manabar;

    public float walkSpeed, jumpVelocity;
    private Rigidbody2D p;
    private bool isTouchingGround;

    public Collider2D bodyCollider;
    public LayerMask ground;
    public Animator playerAnimator;
    public LayerMask monster1;
    public LayerMask monster2;
    public LayerMask monster3;
    public LayerMask monster4;

    //Variable for attack
    public Transform AttackPoint;
    public float attackRange = 0.5f;
   
    public float attackRate = 0.2f;
    public float attackRateBoost = 0f; //Get from item

    //Variable for character status
    float maxHealth;
    public float currentHealth;
    float maxMana;
    public float currentMana;
    int defense;
    public int atkDMG;
    bool attacking = false;
    int lvl;
    double maxexp;
    public double exp;
    int attackCount = 1;
    public int gold;
    public int amountKilled;
    public Quest quest1;

    // Variable for fireball
    public GameObject fireBall;
    int shootSpeed = 80;
    bool canShoot;
    bool canRegenMana;
    bool canRegenHP;

    // Variable for rage
    bool canRage;

    // Variables for audio
    public AudioSource swing;
    public AudioSource footstep;
    public AudioSource rage;
    public AudioSource rageEnd;
    public AudioSource fireball;


    // Start is called before the first frame update
    void Start()
    {
        dmgTaken.enabled = false;
        p = GetComponent<Rigidbody2D>();
        List<Quest> questList = DataHandler.ReadListFromJSON<Quest>("Quest");
        foreach (Quest quest in questList)
        {
            if (quest.questStatus == "Accepted")
            {
                quest1 = quest;
                break;
            }
        }
        CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
        maxHealth = character.health;
        currentHealth = maxHealth;
        atkDMG = character.strength;
        lvl = character.level;
        exp = character.experience;
        maxexp = (character.level + 1000) * 1.3;
        defense = character.defense;
        currentMana = character.mana;
        maxMana = currentMana;
        gold = character.gold;
        canShoot = true;
        canRage = true;
        canRegenMana = true;
        canRegenHP = true;

        // Allow player to walk through monster
        Physics2D.IgnoreLayerCollision(7, 9);
        Physics2D.IgnoreLayerCollision(7, 11);
        Physics2D.IgnoreLayerCollision(7, 12);
        Physics2D.IgnoreLayerCollision(7, 14);
    }

    // Update is called once per frame
    void Update()
    {
        healthbar.fillAmount = currentHealth / maxHealth;
        manabar.fillAmount = currentMana / maxMana;

        if (currentHealth > 0)
        {
            // Lvling up
            if (exp >= maxexp)
            {
                CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
                exp = exp - maxexp;
                character.level += 1;
                character.remainingStatsPt += 1;
                maxexp = (character.level + 1000) * 1.3;
                DataHandler.SaveToJSON(character, "CharacterAttribute");
            }

            isTouchingGround = bodyCollider.IsTouchingLayers(ground);
            float direction = Input.GetAxisRaw("Horizontal");
            float jump = Input.GetAxisRaw("Vertical");

            p.velocity = new Vector2(walkSpeed * direction * Time.fixedDeltaTime, p.velocity.y);
            // Running
            if (direction != 0f)
            {
                playerAnimator.SetBool("IsRunning", true);
                transform.localScale = new Vector2(Mathf.Abs(transform.localScale.x) * direction, transform.localScale.y);
                if (!footstep.isPlaying)
                {
                    footstep.Play();
                }
            }
            else
            {
                playerAnimator.SetBool("IsRunning", false);
                footstep.Stop();
            }

            // Jumping
            if (jump > 0 && isTouchingGround)
            {
                playerAnimator.SetBool("Jump", true);
                p.velocity = new Vector2(p.velocity.x, jumpVelocity * jump * Time.fixedDeltaTime);
            }
            else if (jump == 0 && isTouchingGround)
            {
                playerAnimator.SetBool("Jump", false);
            }
            
            // Attacking
            if (Input.GetKeyDown(KeyCode.Space) && isTouchingGround && !attacking && attackCount == 1)
            {
                StartCoroutine(Attacking());
            }
            else if (Input.GetKeyDown(KeyCode.Space) && isTouchingGround && !attacking && attackCount == 2)
            {
                StartCoroutine(Attacking());
            }

            // Fireball
            if (Input.GetKeyDown(KeyCode.E) && canShoot && !(currentMana <= 30))
            {
                StartCoroutine(Fireball());
            }

            // Regen Mana
            if (Input.GetKeyDown(KeyCode.Q) && currentMana < maxMana && canRegenMana)
            {
                StartCoroutine(RegenMana());
            }

            // Regen HP
            if (Input.GetKeyDown(KeyCode.F) && currentHealth < maxHealth && canRegenHP)
            {
                StartCoroutine(RegenHP());
            }

            // Rage
            if (Input.GetKeyDown(KeyCode.R) && canRage)
            {
                StartCoroutine(Rage());
            }
        }
    }

    IEnumerator DisplayDmgTaken(int damage)
    {
        dmgTaken.text = "- " + damage.ToString() + " HP";
        dmgTaken.transform.position = new Vector2(p.transform.position.x, p.transform.position.y + 10);
        dmgTaken.enabled = true;

        yield return new WaitForSeconds(1);

        dmgTaken.enabled = false;
    }

    public void TakeDamage(int damage)
    {
        int damageTaken = damage - defense;
        if (damageTaken > 0)
        {
            currentHealth -= damageTaken;
        }
        else
        {
            damageTaken = 0;
        }

        //Hurt Animation
        playerAnimator.SetTrigger("Hurt");
        if (currentHealth <= 0)
        {
            StartCoroutine(DisplayDmgTaken(damageTaken));
            Die();
        }
        else
        {
            StartCoroutine(DisplayDmgTaken(damageTaken));
        }
    }

    void Die()
    {
        //Die animation
        playerAnimator.SetBool("IsDead", true);
        p.velocity = Vector2.zero;
    }

    void OnDrawGizmosSelected()
    {
        if (AttackPoint == null)
        {
            return;
        }
        Gizmos.DrawWireSphere(AttackPoint.position, attackRange);
    }

    IEnumerator Attacking()
    {
        attacking = true;

        if (attackCount == 1)
        {
            // Player animator
            playerAnimator.SetTrigger("Attack");
            swing.Play();
            attackCount = 2;
        }
        else
        {
            // Player animator
            playerAnimator.SetTrigger("Attack2");
            swing.Play();
            attackCount = 1;
        }

        // Detect skeletons in range of attack
        Collider2D[] skeletons = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, monster1);

        if (skeletons.Length != 0)
        {
            //Damage enemies
            foreach (Collider2D skeleton in skeletons)
            {
                skeleton.GetComponent<Skeleton>().TakeDamage(atkDMG);
                Debug.Log(atkDMG);
            }
        }

        // Detect archers in range of attack
        Collider2D[] archers = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, monster2);

        if (archers.Length != 0)
        {
            // Damage archers
            foreach (Collider2D archer in archers)
            {
                archer.GetComponent<Archer>().TakeDamage(atkDMG);
            }
        }

        // Detect hell_hands in range of attack
        Collider2D[] hell_hands = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, monster4);

        if (hell_hands.Length != 0)
        {
            // Damage hell_hands
            foreach (Collider2D hell_hand in hell_hands)
            {
                hell_hand.GetComponent<Hell_Hand>().TakeDamage(atkDMG);
            }
        }

        // Detect boss in range of attack
        Collider2D[] boss = Physics2D.OverlapCircleAll(AttackPoint.position, attackRange, monster3);

        if (boss.Length != 0)
        {
            // Damage boss
            foreach (Collider2D Boss in boss)
            {
                Boss.GetComponent<Boss>().TakeDamage(atkDMG);
            }
        }

        yield return new WaitForSeconds(0.6f);

        attacking = false;
    }

    IEnumerator Fireball()
    {
        canShoot = false;

        currentMana -= fireBall.GetComponent<FireBall>().manaCost;

        fireball.Play();

        GameObject newFireBall = Instantiate(fireBall, AttackPoint.position, Quaternion.identity);

        if (transform.localScale.x > 0)
        {
            newFireBall.GetComponent<Rigidbody2D>().velocity = new Vector2(shootSpeed, 0f);
        }
        else
        {
            newFireBall.transform.localScale = new Vector2(newFireBall.transform.localScale.x * -1, newFireBall.transform.localScale.y);
            newFireBall.GetComponent<Rigidbody2D>().velocity = new Vector2(shootSpeed * -1, 0f);
        }

        yield return new WaitForSeconds(2);

        canShoot = true;
    }

    IEnumerator Rage()
    {
        canRage = false;

        atkDMG += (int)(atkDMG * 0.3);

        rage.Play();

        Debug.Log("Rage Started");

        yield return new WaitForSeconds(15);

        Debug.Log("Rage Ended");

        atkDMG -= (int)(atkDMG * 0.3);

        rageEnd.Play();

        yield return new WaitForSeconds(30);

        canRage = true;
    }

    IEnumerator RegenMana()
    {
        canRegenMana = false;

        currentMana += (float)(maxMana * 0.3);

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        yield return new WaitForSeconds(15);

        canRegenMana = true;
    }

    IEnumerator RegenHP()
    {
        canRegenHP = false;

        currentHealth += (float)(maxHealth * 0.3);

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        yield return new WaitForSeconds(15);

        canRegenHP = true;
    }
}
