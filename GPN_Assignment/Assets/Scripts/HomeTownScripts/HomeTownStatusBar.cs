using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class HomeTownStatusBar : MonoBehaviour
{
    public TextMeshProUGUI maxHealth;
    public TextMeshProUGUI maxMana;
    public TextMeshProUGUI gold;
    public TextMeshProUGUI experience;
    float maxhealth;
    float currenthealth;
    float maxmana;
    float currentmana;
    double currentexp;
    double maxexp;
    GameObject player;

    void Start()
    {
        player = GameObject.Find("Player");
        updateStatusBar();
    }

    void Update()
    {
        updateStatusBar();
    }

    private void OnEnable()
    {
        Debug.Log("Update status bar");
        updateStatusBar();
    }
    public void updateStatusBar()
    {
        CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
        maxhealth = character.health;
        currenthealth = player.GetComponent<HomeTownPlayerController>().currentHealth;
        maxmana = character.mana;
        currentmana = player.GetComponent<HomeTownPlayerController>().currentMana;
        maxexp = Mathf.FloorToInt((character.level * 200) + 1000);
        currentexp = player.GetComponent<HomeTownPlayerController>().exp;
        maxHealth.text = currenthealth.ToString() + " / " + maxhealth.ToString();
        maxMana.text = currentmana.ToString() + " / " + maxmana.ToString();
        gold.text = character.gold.ToString();
        experience.text = currentexp.ToString() + " / " + maxexp.ToString();
    }
}
