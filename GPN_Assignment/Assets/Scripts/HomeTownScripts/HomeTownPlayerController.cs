using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HomeTownPlayerController : MonoBehaviour
{
    float walkSpeed = 10000;
    float jumpVelocity = 8000;
    Rigidbody2D p;
    bool isTouchingGround;
    public Collider2D bodyCollider;
    public LayerMask ground;
    public Animator playerAnimator;

    //Variable for character status
    float maxHealth;
    public float currentHealth;
    float maxMana;
    public float currentMana;
    int defense;
    public int atkDMG;
    int lvl;
    int maxexp;
    public int exp;
    public int gold;

    public Image healthbar;
    public Image manabar;

    public AudioSource footstep;

    // Start is called before the first frame update
    void Start()
    {
        p = GetComponent<Rigidbody2D>();
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
    }

    // Update is called once per frame
    void Update()
    {
        if (!DialogueManager.GetInstance().dialogueIsPlaying)
        {
            healthbar.fillAmount = currentHealth / maxHealth;
            manabar.fillAmount = currentMana / maxMana;

            // Lvling up
            if (exp >= maxexp)
            {
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

        }
        else
        {
            return;
        }

    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

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
        p.velocity = Vector2.zero;
    }
}
