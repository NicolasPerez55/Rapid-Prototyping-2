using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class StatusIndicatorScript : MonoBehaviour
{
    //For managing the HP / Time Remaining / Special Effect indicators on PossessionObjects. Each PossessionObject needs a child with this script

    private GameManager manager;
    [SerializeField] private SpriteRenderer icon;
    [SerializeField] private TextMeshPro timerText;
    [SerializeField] private TextMeshPro conditionText;

    private float timer;
    private string condition;
    private Sprite iconImage;
    private PossessionObject parentScript;

    public void setupStatus(float newTimer, string newCondition, Sprite newImage, bool temporary)
    {
        timer = newTimer;
        condition = newCondition;
        iconImage = newImage;

        icon.sprite = iconImage;
        timerText.text = "" + timer;
        conditionText.text = condition;

        //if (condition == "")
        if (!temporary)
        {
            icon.enabled = false;
            timerText.enabled = false;
            conditionText.enabled = false;
        }
        parentScript = GetComponentInParent<PossessionObject>();
        manager = FindAnyObjectByType<GameManager>();

    }

    public void updateStatus(float newTimer)
    {
        timerText.text = "" + (int)newTimer;
    }

    public void revealStatus()
    {
        icon.enabled = true;
        timerText.enabled = true;
        if (condition != "") conditionText.enabled = true;
        if (parentScript.isTemporary)
        {
            timerText.text = "" + (int)parentScript.lifetime;
        }
        else
        {
            timerText.text = "" + (int)parentScript.currentHealth;
        }
    }

    public void hideStatus()
    {
        icon.enabled = false;
        timerText.enabled = false;
        conditionText.enabled = false;
    }
}
