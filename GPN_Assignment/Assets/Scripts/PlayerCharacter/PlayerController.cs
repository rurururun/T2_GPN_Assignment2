using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlayerController : MonoBehaviour
{
    // Variable for displaying dmg taken
    public TextMeshProUGUI dmgTaken;
    public TextMeshProUGUI lvlup;
    public TextMeshProUGUI manaCost;
    public Image healthbar;
    public Image manabar;
    public Image fireBallSpell;
    public Image rageSpell;
    public Image revitalizeSpell;
    public TextMeshProUGUI healIndicator;
    public Image rejuvinateSpell;
    public TextMeshProUGUI manaIndicator;

    public Image questPanel;
    public TextMeshProUGUI questTitle;
    public Image questProgressionBar;
    public TextMeshProUGUI questProgression;

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
    int maxexp;
    public int exp;
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
    public AudioSource levelUp;
    public AudioSource heal;
    public AudioSource regenMana;


    // Start is called before the first frame update
    void Start()
    {
        lvlup.enabled = false;
        dmgTaken.enabled = false;
        healIndicator.enabled = false;
        manaIndicator.enabled = false;
        manaCost.enabled = false;
        p = GetComponent<Rigidbody2D>();
        quest1 = null;
        List<Quest> questList = DataHandler.ReadListFromJSON<Quest>("Quest");
        foreach (Quest quest in questList)
        {
            if (quest.questStatus == "Accepted")
            {
                quest1 = quest;
                break;
            }
        }
        if (quest1 != null)
        {
            questPanel.gameObject.SetActive(true);

            float achieved_amount = quest1.archiveAmount;
            float needed_amount = quest1.objectiveAmount;

            if (achieved_amount < needed_amount)
            {
                questTitle.text = quest1.questTitle;
                questProgression.text = quest1.archiveAmount + "/" + quest1.objectiveAmount;
                questProgressionBar.fillAmount = achieved_amount / needed_amount;
            }
            else
            {
                questTitle.text = quest1.questTitle;
                questProgression.text = "Completed";
                questProgressionBar.fillAmount = 1f;
            }
        }
        else
        {
            questPanel.gameObject.SetActive(false);
        }
        CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
        maxHealth = character.health;
        currentHealth = maxHealth;
        atkDMG = character.strength;
        lvl = character.level;
        exp = character.experience;
        maxexp = Mathf.FloorToInt((character.level * 200) + 1000);
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

        if (quest1 != null)
        {
            float achieved_amount = quest1.archiveAmount;
            float needed_amount = quest1.objectiveAmount;

            if (achieved_amount < needed_amount)
            {
                questProgression.text = quest1.archiveAmount + "/" + quest1.objectiveAmount;
                questProgressionBar.fillAmount = achieved_amount / needed_amount;
            }
            else
            {
                List<Quest> questList = DataHandler.ReadListFromJSON<Quest>("Quest");
                for (int i = 0; i < questList.Count; i++)
                {
                    if (questList[i].questTitle == quest1.questTitle)
                    {
                        questList[i].questStatus = "Completed";
                        quest1.questStatus = "Completed";
                        break;
                    }
                }
                DataHandler.SaveToJSON(questList, "Quest");
                questProgression.text = "Completed";
                questProgressionBar.fillAmount = 1f;
            }
        }

        if (currentHealth > 0)
        {
            // Lvling up
            if (exp >= maxexp)
            {
                levelUp.Play();
                StartCoroutine(DisplayLvlUp());
                CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
                exp = exp - maxexp;
                character.level += 1;
                character.experience = exp;
                character.remainingStatsPt += 1;
                maxexp = Mathf.FloorToInt((character.level * 200) + 1000);
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

    IEnumerator DisplayLvlUp()
    {
        lvlup.enabled = true;

        yield return new WaitForSeconds(1);

        lvlup.enabled = false;
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
        footstep.Stop();
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

        fireBallSpell.fillAmount = 0f;

        currentMana -= fireBall.GetComponent<FireBall>().manaCost;

        StartCoroutine(DisplayManaCost());

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

        yield return new WaitForSeconds(1);

        canShoot = true;

        fireBallSpell.fillAmount = 1f;
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

        rageSpell.fillAmount = 0f;

        yield return new WaitForSeconds(30);

        canRage = true;

        rageSpell.fillAmount = 1f;
    }

    IEnumerator RegenMana()
    {
        canRegenMana = false;

        regenMana.Play();

        currentMana += (float)(maxMana * 0.3);

        if (currentMana > maxMana)
        {
            currentMana = maxMana;
        }

        StartCoroutine(DisplayManaIndicator());

        rejuvinateSpell.fillAmount = 0f;

        yield return new WaitForSeconds(15);

        rejuvinateSpell.fillAmount = 1f;

        canRegenMana = true;
    }

    IEnumerator RegenHP()
    {
        canRegenHP = false;

        heal.Play();

        currentHealth += (float)(maxHealth * 0.3);

        if (currentHealth > maxHealth)
        {
            currentHealth = maxHealth;
        }

        StartCoroutine(DisplayHealIndicator());

        revitalizeSpell.fillAmount = 0f;

        yield return new WaitForSeconds(15);

        revitalizeSpell.fillAmount = 1f;

        canRegenHP = true;
    }

    IEnumerator DisplayHealIndicator()
    {
        healIndicator.text = "+ " + (float)(maxHealth * 0.3) + "HP";
        healIndicator.transform.position = new Vector2(p.transform.position.x, p.transform.position.y + 20);
        healIndicator.enabled = true;

        yield return new WaitForSeconds(1);

        healIndicator.enabled = false;
    }

    IEnumerator DisplayManaIndicator()
    {
        manaIndicator.text = "+ " + (float)(maxMana * 0.3) + "MP";
        manaIndicator.transform.position = new Vector2(p.transform.position.x, p.transform.position.y + 20);
        manaIndicator.enabled = true;

        yield return new WaitForSeconds(1);

        manaIndicator.enabled = false;
    }

    IEnumerator DisplayManaCost()
    {
        manaCost.text = "- " + fireBall.GetComponent<FireBall>().manaCost + "MP";
        manaCost.transform.position = new Vector2(p.transform.position.x, p.transform.position.y + 20);
        manaCost.enabled = true;

        yield return new WaitForSeconds(1);

        manaCost.enabled = false;
    }
}
