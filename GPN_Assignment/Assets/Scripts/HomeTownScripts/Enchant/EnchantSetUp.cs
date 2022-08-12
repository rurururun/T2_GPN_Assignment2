using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class EnchantSetUp : MonoBehaviour
{
    public TextMeshProUGUI SelectedItemType;
    public TextMeshProUGUI EnchantItemName;
    public TextMeshProUGUI EnchantItemAttribute;
    public TextMeshProUGUI EnchantItemEnchantLvl;
    public TextMeshProUGUI EnchantItemCost;

    public GameObject enchantButton;
    public static Equipment selectedEquipment;

    private string selectedEquipmentType = null;

    public void setUpEnchant()
    {       
        selectedEquipmentType = SelectedItemType.text.ToString();
        //selectedEquipment = new Equipment("null",0,0);
        enchantButton.SetActive(true);
        selectedEquipment = EnchantTrigger.GetEquipmentAttribute(selectedEquipmentType);
        EnchantItemName.text = selectedEquipment.equipmentType;
        Debug.Log("selected" + selectedEquipment.equipmentType);

        //Define type of equipment attribute
        string Attribute = selectedEquipment.equipmentArritbute.ToString();
        if (selectedEquipmentType == "Weapon")
        {
            Attribute = selectedEquipment.equipmentArritbute.ToString() + " Strength";
        }
        else if (selectedEquipmentType == "Ring")
        {
            Attribute = selectedEquipment.equipmentArritbute.ToString() + " Mana";
        }
        else if (selectedEquipmentType == "Helmet")
        {
            Attribute = selectedEquipment.equipmentArritbute.ToString() + " Health";
        }
        else if (selectedEquipmentType == "Armor")
        {
            Attribute = selectedEquipment.equipmentArritbute.ToString() + " Defense";
        }
        else
        {
            Debug.Log(selectedEquipmentType + "Equipment type error");
        }
        EnchantItemCost.text = selectedEquipment.equipmentEnchantCost.ToString();
        EnchantItemAttribute.text = Attribute;
        EnchantItemEnchantLvl.text = selectedEquipment.equipmentEnchantLvl.ToString() + " / " + "24";
    }

    void OnEnable()
    {
        if(selectedEquipmentType != null)
        {
            //Define type of equipment attribute
            string Attribute = selectedEquipment.equipmentArritbute.ToString();
            if (selectedEquipmentType == "Weapon")
            {
                Attribute = selectedEquipment.equipmentArritbute.ToString() + " Strength";
            }
            else if (selectedEquipmentType == "Ring")
            {
                Attribute = selectedEquipment.equipmentArritbute.ToString() + " Mana";
            }
            else if (selectedEquipmentType == "Helmet")
            {
                Attribute = selectedEquipment.equipmentArritbute.ToString() + " Health";
            }
            else if (selectedEquipmentType == "Armor")
            {
                Attribute = selectedEquipment.equipmentArritbute.ToString() + " Defense";
            }
            else
            {
                Debug.Log(selectedEquipmentType + "Equipment type error");
            }
            EnchantItemCost.text = selectedEquipment.equipmentEnchantCost.ToString();
            EnchantItemAttribute.text = Attribute;
            EnchantItemEnchantLvl.text = selectedEquipment.equipmentEnchantLvl.ToString() + " / " + "24";
        }
        
    }

    public static Equipment currentEquipment()
    {
        return selectedEquipment;
    }


}
