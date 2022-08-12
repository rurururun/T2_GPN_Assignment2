using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestButton : MonoBehaviour
{
    public GameObject questPanel;
    public GameObject statusBar;
    public AudioSource completeSound;
    public void AcceptQuest()
    {
        Quest currentQuest = SetUpQuest.getCurrentQuest();
        List<Quest>questList = DataHandler.ReadListFromJSON<Quest>("Quest");
        foreach(Quest quest in questList)
        {
            if(quest.questTitle == currentQuest.questTitle)
            {
                quest.questStatus = "Accepted";
                break;
            }
        }
        DataHandler.SaveToJSON(questList, "Quest");
        questPanel.SetActive(false);
        questPanel.SetActive(true);
    }

    public void RefreshQuest()
    {
        questPanel.SetActive(false);
        questPanel.SetActive(true);
    }

    public void CompleteQuest()
    {
        completeSound.Play();
        Quest currentQuest = SetUpQuest.getCurrentQuest();
        List<Quest> questList = DataHandler.ReadListFromJSON<Quest>("Quest");
        foreach (Quest quest in questList)
        {
            if (quest.questTitle == currentQuest.questTitle)
            {
                quest.questStatus = "Not Accepted";
                quest.archiveAmount = 0;
                CharacterAttribute character = DataHandler.ReadFromJSON<CharacterAttribute>("CharacterAttribute");
                character.experience += quest.rewardExp;
                character.gold += quest.rewardGold;
                if (character.experience >= Mathf.FloorToInt((float)((character.level + 1000) * 1.5)))
                {
                    character.experience = character.experience - Mathf.FloorToInt((float)((character.level + 1000) * 1.5));
                    character.level += 1;
                    character.remainingStatsPt += 1;
                }
                DataHandler.SaveToJSON(character, "CharacterAttribute");
                break;
            }
        }
        DataHandler.SaveToJSON(questList, "Quest");
        
        questPanel.SetActive(false);
        questPanel.SetActive(true);
        statusBar.SetActive(false);
        statusBar.SetActive(true);
    }

  
}
