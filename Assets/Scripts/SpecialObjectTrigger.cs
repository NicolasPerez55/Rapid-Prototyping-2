using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpecialObjectTrigger : MonoBehaviour
{
    [SerializeField] private PossessionObject thisObject;
    [SerializeField] private AIEntity thisAI;
    [SerializeField] private BoxCollider2D collider;
    [SerializeField] private SpriteRenderer renderer;

    public string type = "screen"; //screen, semisolid, mute, truck, alarm
    public bool oneshot = false; //Whether the effect can only trigger once
    public bool effectTriggered = false;
    public bool retriggerOnLeave = false;

    [Space, Header("Type Specific stuff")]
    public bool effectActive = false; //wether this is the trigger or the retrigger
    public Sprite newDefaultSprite;

    [SerializeField] private Sprite openDoorSprite;
    [SerializeField] private Sprite closedDoorSprite;
    public bool doorOpen = false;
    public float doorTimer = 5f;

    [SerializeField] private AIEntity alarm1;
    [SerializeField] private AIEntity alarm2;
    [SerializeField] private AIEntity alarm3;
    [SerializeField] private AIEntity relevantHuman1;
    [SerializeField] private Vector3 alarmTarget1;
    [SerializeField] private Vector3 alarmTarget2;
    [SerializeField] private Vector3 alarmTarget3;

    [SerializeField] private SpecialObjectTrigger trapdoor1;
    [SerializeField] private SpecialObjectTrigger trapdoor2;
    [SerializeField] private SpecialObjectTrigger trapdoor3;
    [SerializeField] private SpecialObjectTrigger trapdoor4;


    public void specialTrigger(bool justEntered)
    {
        if ((justEntered || retriggerOnLeave) && (!effectTriggered || !oneshot))
        switch (type)
        {
            case "truck":
                thisAI.moveNext = true;
                break;
            case "alarm":
                break;
            case "door":
                    if (!doorOpen)
                    {
                        renderer.sprite = openDoorSprite;
                        doorOpen = true;
                        collider.enabled = false;
                    }
                    else
                    {
                        collider.enabled = true;
                        doorOpen = false;
                        renderer.sprite = closedDoorSprite;
                    }
                break;
            case "mute":
                break;
            case "screen":
                thisObject.canPossess = true;
                thisObject.defaultSprite = newDefaultSprite;
                break;
            case "alarmControl":
                    //alarm1.specialTrigger(justEntered);
                    //alarm2.specialTrigger(justEntered);
                    //alarm3.specialTrigger(justEntered);
                    alarm1.alarmActive = true;
                    alarm2.alarmActive = true;
                    alarm3.alarmActive = true;
                    switch (relevantHuman1.currentFloor)
                    {
                        case 1:
                            relevantHuman1.becomeAlerted(alarmTarget3);
                            //alarm1.alarmActive = true;
                            break;
                        case 2:
                            relevantHuman1.becomeAlerted(alarmTarget2);
                            //alarm2.alarmActive = true;
                            break;
                        case 3:
                        default:
                            relevantHuman1.becomeAlerted(alarmTarget1);
                            //alarm3.alarmActive = true;
                            break;
                    }
                    break;
                case "trapdoorControl":
                    trapdoor1?.specialTrigger(justEntered);
                    trapdoor2?.specialTrigger(justEntered);
                    trapdoor3?.specialTrigger(justEntered);
                    trapdoor4?.specialTrigger(justEntered);
                    break;
            default:
                break;
        }
        effectTriggered = true;
    }

    public void humanDoorInteraction()
    {
        if (!doorOpen)
        {
            renderer.sprite = openDoorSprite;
            doorOpen = true;
            collider.enabled = false;
            StartCoroutine(keepDoorOpen());
        }
    }

    IEnumerator keepDoorOpen()
    {
        float timer = doorTimer;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        collider.enabled = true;
        doorOpen = false;
        renderer.sprite = closedDoorSprite;
    }
}
