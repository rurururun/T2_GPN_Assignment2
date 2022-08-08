using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float walkSpeed, jumpVelocity;
    private Rigidbody2D p;
    private bool isTouchingGround;

    public Collider2D bodyCollider;
    public LayerMask ground;
    public Animator playerAnimator;
    public LayerMask monster1;
    public LayerMask monster2;
    public LayerMask monster3;

    //Variable for attack
    public Transform AttackPoint;
    public float attackRange = 0.5f;
   
    public float attackRate = 0.2f;
    public float attackRateBoost = 0f; //Get from item

    //Variable for character status
    public int maxHealth = 1000;
    private int equipmentHealth;
    public int currentHealth;
    int atkDMG = 100;
    bool attacking = false;
    int lvl = 1;
    public int exp = 0;
    public int gold = 0;
    int lvlUp;
    int attackCount = 1;

    // Variable for fireball
    public GameObject fireBall;
    public int shootSpeed;
    private bool canShoot;

    // Variable for rage
    bool canRage;


    // Start is called before the first frame update
    void Start()
    {
        p = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth + equipmentHealth;
        canShoot = true;
        canRage = true;

        // Allow player to walk through monster
        Physics2D.IgnoreLayerCollision(7, 9);
        Physics2D.IgnoreLayerCollision(7, 11);
        Physics2D.IgnoreLayerCollision(7, 12);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentHealth > 0)
        {
            // Lvling up
            lvlUp = lvl * 50;

            if (exp >= lvlUp)
            {
                exp = exp - lvlUp;
                lvl += 1;
                atkDMG += 10;
                maxHealth += 20;
                currentHealth = maxHealth;
                Debug.Log(lvl);
                Debug.Log(atkDMG);
                Debug.Log(currentHealth);
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
            }
            else
            {
                playerAnimator.SetBool("IsRunning", false);
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
            if (Input.GetKeyDown(KeyCode.E) && canShoot)
            {
                StartCoroutine(Fireball());
            }

            // Rage
            if (Input.GetKeyDown(KeyCode.R) && canRage)
            {
                StartCoroutine(Rage());
            }
        }
    }
        
    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        Debug.Log(currentHealth);

        //Hurt Animation
        playerAnimator.SetTrigger("Hurt");
        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        //Die animation
        playerAnimator.SetBool("IsDead", true);
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
            attackCount = 2;
        }
        else
        {
            // Player animator
            playerAnimator.SetTrigger("Attack2");
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

        yield return new WaitForSeconds(5);

        canShoot = true;

        Debug.Log("Can Shoot");
    }

    IEnumerator Rage()
    {
        canRage = false;

        atkDMG += 30;

        Debug.Log("Rage Started");

        yield return new WaitForSeconds(15);

        Debug.Log("Rage Ended");

        atkDMG -= 30;

        yield return new WaitForSeconds(30);

        canRage = true;
    }
}