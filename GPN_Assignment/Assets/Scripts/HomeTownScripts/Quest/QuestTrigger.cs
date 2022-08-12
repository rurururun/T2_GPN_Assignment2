using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class QuestTrigger : MonoBehaviour
{
    [Header("Quest Visual Cue")]
    [SerializeField] private GameObject questVisualCue;

    [Header("Quest Visual Hint")]
    [SerializeField] private GameObject questHintText;

    [Header("Quest Panel")]
    [SerializeField] private GameObject questPanel;

    public GameObject playButton;
    public GameObject characterButton;
    public GameObject howToPlayButton;

    

    private bool playerInRange;

    private void Update()
    {
        if (playerInRange)
        {
            questVisualCue.SetActive(true);
            questHintText.SetActive(true);
            if (Input.GetKeyDown(KeyCode.I))
            {
                playButton.SetActive(false);
                characterButton.SetActive(false);
                howToPlayButton.SetActive(false);
                questPanel.SetActive(true);
            }
        }
        else
        {
            questHintText.SetActive(false);
            questVisualCue.SetActive(false);
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
}
