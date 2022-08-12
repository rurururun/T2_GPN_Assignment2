using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnchantTrigger : MonoBehaviour
{
    [Header("Enchant Visual Cue")]
    [SerializeField] private GameObject enchantVisualCue;

    [Header("Enchant Panel")]
    [SerializeField] private GameObject enchantPanel;

    [Header("Visual Hint")]
    [SerializeField] private GameObject visualHint;

    public GameObject playButton;
    public GameObject characterButton;
    public GameObject howToPlayButton;   

    private bool playerInRange;

    public static List<Equipment> equipmentList;

    private void Awake()
    {
        // Setting up Equipment detail for enchant
        equipmentList = DataHandler.ReadListFromJSON<Equipment>("Equipment");
        if (equipmentList.Count > 0)
        {
            Debug.Log("Enchant Set up");
        }
        else
        {
            Debug.Log("Enchant Set up failed");
        }

        //Set default to false
        playerInRange = false;
        enchantVisualCue.SetActive(false);
        visualHint.SetActive(false);
    }

    private void Update()
    {
        if (playerInRange)
        {
            enchantVisualCue.SetActive(true);
            visualHint.SetActive(true);
            if (Input.GetKeyDown(KeyCode.I))
            {
                playButton.SetActive(false);
                characterButton.SetActive(false);
                howToPlayButton.SetActive(false);
                enchantPanel.SetActive(true);
            }
        }
        else
        {
            visualHint.SetActive(false);
            enchantVisualCue.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collider)
    {
        if (collider.gameObject.tag == "Player")
        {
            playerInRange = false;
        }
    }

    public static Equipment GetEquipmentAttribute(string EquipmentType)
    {
        foreach (Equipment equipment in equipmentList)
        {
            if (equipment.equipmentType == EquipmentType)
            {
                return equipment;
            }
        }
        return null;
    }
    public static List<Equipment> GetEquipmentList()
    {
        return equipmentList;
    }

    public static void updateEquipmentList()
    {
        equipmentList = DataHandler.ReadListFromJSON<Equipment>("Equipment");

        if (equipmentList.Count > 0)
        {
            Debug.Log("Equipment list updated");
        }
        else
        {
            Debug.Log("Equipment list update failed");
        }
    }
}
