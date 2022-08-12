using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class EnchantEquipment : MonoBehaviour
{
    public TextMeshProUGUI enchantStatus;
    public static List<Equipment> equipmentList;
    public Equipment enchantEquipment;

    public GameObject enchantPanel;
    public GameObject statusBar;

    public AudioSource enchantSound;

    private CharacterAttribute character;
    public void enchantItem()
    {
        enchantEquipment = EnchantSetUp.currentEquipment();

        Debug.Log("selectedEnchant" +  enchantEquipment.equipmentType);

        //Check if is max level
        if (enchantEquipment.equipmentEnchantLvl < 24)
        {
            //Check if enough gold
            character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
            if(enchantEquipment.equipmentEnchantCost <= character.gold)
            {
                enchantSound.Play();
                equipmentList = EnchantTrigger.GetEquipmentList();
                string selectedEquipmentType = enchantEquipment.equipmentType;
                if (selectedEquipmentType == "Weapon")
                {
                    enchantEquipment.equipmentArritbute += 5;
                }
                else if (selectedEquipmentType == "Ring")
                {
                    enchantEquipment.equipmentArritbute += 10;
                }
                else if (selectedEquipmentType == "Helmet")
                {
                    enchantEquipment.equipmentArritbute += 10;
                }
                else if (selectedEquipmentType == "Armor")
                {
                    enchantEquipment.equipmentArritbute += 5;
                }
                else
                {
                    Debug.Log("Enchant type error");
                }
                character.gold -= enchantEquipment.equipmentEnchantCost;
                enchantEquipment.equipmentEnchantLvl += 1;
                enchantEquipment.equipmentEnchantCost = (200 * enchantEquipment.equipmentEnchantLvl) + 1000;

                foreach (Equipment equipment in equipmentList)
                {
                    if (equipment.equipmentType == selectedEquipmentType)
                    {
                        equipment.equipmentArritbute = enchantEquipment.equipmentArritbute;
                        equipment.equipmentEnchantCost = enchantEquipment.equipmentEnchantCost;
                        equipment.equipmentEnchantLvl = enchantEquipment.equipmentEnchantLvl;
                        break;
                    }
                }
                enchantStatus.text = "Enchance Successfuly!!";
                updateChracterAttribute(equipmentList, enchantEquipment, character);
                //EnchantTrigger.updateEquipmentList();
                enchantPanel.SetActive(false);
                enchantPanel.SetActive(true);
                statusBar.SetActive(false);
                statusBar.SetActive(true);
            }
            else
            {
                enchantStatus.text = "Not enough gold.";
            }
        }
        else
        {
            enchantStatus.text = "Equipment is maxed Level!";
        }
    }

    //Update Overall Character Attribute
    public static void updateChracterAttribute(List<Equipment> equipmentList, Equipment currentEquipment, CharacterAttribute character)
    {
        foreach (Equipment equipment in equipmentList)
        {
            if (equipment.equipmentType == currentEquipment.equipmentType && equipment.equipmentType == "Weapon")
            {
                character.strength = equipment.equipmentArritbute;
                break;
            }
            else if (equipment.equipmentType == currentEquipment.equipmentType && equipment.equipmentType == "Ring")
            {
                character.mana = equipment.equipmentArritbute;
                break;
            }
            else if (equipment.equipmentType == currentEquipment.equipmentType && equipment.equipmentType == "Helmet")
            {
                character.health = equipment.equipmentArritbute;
                break;
            }
            else if (equipment.equipmentType == currentEquipment.equipmentType && equipment.equipmentType == "Armor")
            {
                character.defense = equipment.equipmentArritbute;
                break;
            }
        }
        for (int i = 0; i < character.strengthStatsPt; i++)
        {
            character.strength += 5;
        }

        for (int i = 0; i < character.healthStatsPt; i++)
        {
            character.health += 10;
        }

        for (int i = 0; i < character.defenseStatsPt; i++)
        {
            character.defense += 5;
        }

        character.remainingStatsPt = character.level - character.healthStatsPt - character.strengthStatsPt - character.defenseStatsPt - 1;
        DataHandler.SaveToJSON(character, "CharacterAttribute");
        DataHandler.SaveToJSON(equipmentList, "Equipment");
    }
}

