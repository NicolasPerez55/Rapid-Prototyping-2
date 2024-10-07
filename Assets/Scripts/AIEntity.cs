using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIEntity : MonoBehaviour
{
    //This Script is used for any entity that moves or does something special on its own. For entities that have triggered effects when possessed, check the PossessionTrigger script

    public string behaviorType = "static"; //can be "static" (does not move), "roaming" (wanders until it runs into an wall/certain objects, then does something), or "waypoints" (moves to the next waypoint when prompted to)
    public float movementSpeed = 0.1f;
    public bool schmooving = false;
    [SerializeField] private SpriteRenderer renderer;

    [Space, Header("Behavior Specific Vars")]
    public List<Vector2> waypoints = new List<Vector2>(); //First waypoint is be the starting location of the object
    public int currentWaypoint = 0;
    public bool moveNext = false;

    public string currentState = "walking";
    public string objectType = "human";
    public DetectorScript obstacleDetector;
    public DetectorScript objectDetector;
    public string facing = "right";
    public Vector3 alertLocation = Vector3.zero; //When alerted, the location where the human moves to
    public int currentFloor = 3; //Which floor this human is on, used for alerts

    [SerializeField] private float obstacleWait = 1f;
    [SerializeField] private bool socialAnxiety = false; //Cooldown after talking before the human can talk again
    [SerializeField] GameObject soundObjectPrefab; //Instanced to create a sound object
    private float fallingTimer = 1;
    [SerializeField] private GameObject mostRecentDoor; //for fixing an issue with alarms

    public bool isAlarm = false;
    public bool isFakeRadio = false; //For the two humans in the first level that are actually radios
    public bool alarmActive = false;

    public bool isRadio = false; //fuck it.
    private float radioTimer = 5;
    public float radioTimerLength = 5f;
    public bool singingLyrics = false;

    private Vector3 startPos;
    private string startFacing;
    private string startState;
    private int startFloor;

    private GameManager manager;

    void Start()
    {
        manager = FindAnyObjectByType<GameManager>();
        manager.resetPosition.AddListener(resetToStart);
        startPos = transform.position;
        startFacing = facing;
        startState = currentState;
        startFloor = currentFloor;

        renderer = gameObject.GetComponent<SpriteRenderer>();
        switch (behaviorType)
        {
            case "roaming":
                //Setup the detectors to trigger stuff when they hit something
                foreach (DetectorScript x in GetComponentsInChildren<DetectorScript>())
                {
                    if (x.detectorType == "obstacles")
                        obstacleDetector = x;
                    else if (x.detectorType == "objects")
                        objectDetector = x;
                }

                obstacleDetector.collided.AddListener(obstacleEncountered);
                objectDetector.collided.AddListener(objectEncountered);
                break;
            case "waypoints":
                transform.position = waypoints[currentWaypoint];
                
                break;
            case "static":
            default:
                break;
        }
        radioTimer = radioTimerLength;
    }

    
    void FixedUpdate()
    {
        if (manager.gameRunning)
        switch (behaviorType)
        {
            case "roaming":
                if (transform.position.y > -3.8 && transform.position.y > -1.2)
                {
                    currentFloor = 3;
                }
                else if (transform.position.y > -3.8 && transform.position.y < -1.2)
                {
                    currentFloor = 2;
                }
                else
                {
                    currentFloor = 1;
                }
                if (GetComponent<Rigidbody2D>().velocity.y > 0.6 || GetComponent<Rigidbody2D>().velocity.y < -0.6)
                {
                    currentState = "falling";
                    StopAllCoroutines();
                    fallingTimer = 1;
                }
                switch (currentState)
                {
                    case "walking":
                        if (facing == "right")
                        {
                            transform.position = new Vector3(transform.position.x + movementSpeed, transform.position.y, transform.position.z);
                        }
                        else
                        {
                            transform.position = new Vector3(transform.position.x - movementSpeed, transform.position.y, transform.position.z);
                        }
                        break;
                    case "alerted": //Runs towards the alarm location
                        if (Vector3.Distance(transform.position, alertLocation) > 1.5f) //too far, move to the alertLocation
                        {
                            if (alertLocation.x < transform.position.x)
                            {
                                transform.position = new Vector3(transform.position.x - (movementSpeed * 1.5f), transform.position.y, transform.position.z);
                                if (facing == "right")
                                {
                                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                                    facing = "left";
                                }
                            }
                            else if (alertLocation.x > transform.position.x)
                            {
                                transform.position = new Vector3(transform.position.x + (movementSpeed * 1.5f), transform.position.y, transform.position.z);
                                if (facing == "left")
                                {
                                    transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
                                    facing = "right";
                                }
                            }
                        }
                        else //location reached, stop
                        {
                            currentState = "stopped";
                            StartCoroutine(stopThenTurn());
                        }
                        break;
                    case "falling":
                        fallingTimer -= Time.deltaTime;
                        if (fallingTimer <= 0)
                        {
                            if (currentState == "falling")
                                currentState = "walking";
                            fallingTimer = 1;
                            socialAnxiety = false;
                        }
                        break;
                    case "stopped":
                        break;
                    default:
                        break;
                }
                break;
            case "waypoints":
                if (moveNext)
                {
                    moveNext = false;
                    moveToNextWaypoint();
                }
                break;
            case "static":
            default:
                if (isRadio)
                {
                    radioTimer -= Time.deltaTime;
                    if (radioTimer <= 0)
                    {
                        radioTimer = radioTimerLength;
                        if (singingLyrics) //Speech bubble is present (though about to expire)
                        {
                            singingLyrics = false;
                            radioTimer = radioTimerLength / 2;
                        }
                        else //create the speech bubble
                        {
                            bool flip = false;
                            GameObject speechBubble = Instantiate(soundObjectPrefab);
                            if (facing == "right")
                            {
                                if (isFakeRadio)
                                    speechBubble.transform.position = new Vector3(transform.position.x + 1.5f, transform.position.y + 1.5f, transform.position.z);
                                else
                                    speechBubble.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f, transform.position.z);
                            }
                            else
                            {
                                if (isFakeRadio)
                                    speechBubble.transform.position = new Vector3(transform.position.x - 1.5f, transform.position.y + 1.5f, transform.position.z);
                                else
                                    speechBubble.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, transform.position.z);
                                flip = true;
                            }
                            speechBubble.GetComponent<PossessionObject>().setupSoundObject("speechBubble", radioTimerLength, Vector3.zero, flip);
                            singingLyrics = true;
                            if (isFakeRadio)
                            {
                                speechBubble.GetComponent<PossessionObject>().setupSpeechBubbleText("fakeRadio");
                            }
                            else
                            {
                                speechBubble.GetComponent<PossessionObject>().setupSpeechBubbleText("radio");
                            }
                        }
                    }
                }
                if (isAlarm)
                {
                    if (alarmActive)
                    {
                        alarmActive = false;
                        bool flip = false;
                        GameObject speechBubble = Instantiate(soundObjectPrefab);
                        if (facing == "right")
                            speechBubble.transform.position = new Vector3(transform.position.x + 0.5f, transform.position.y + 0.5f, transform.position.z);
                        else
                        {
                            speechBubble.transform.position = new Vector3(transform.position.x - 0.5f, transform.position.y + 0.5f, transform.position.z);
                            flip = true;
                        }
                        speechBubble.GetComponent<PossessionObject>().setupSoundObject("speechBubble", radioTimerLength, Vector3.zero, flip);
                        speechBubble.GetComponent<PossessionObject>().setupSpeechBubbleText("alarm");
                    }
                }
                break;
        }
    }

    public void becomeAlerted(Vector3 targetLocation)
    {

        if (currentState == "stopped")
            StopAllCoroutines();
        currentState = "alerted";
        alertLocation = targetLocation;
        if (objectDetector.gameObject.GetComponent<BoxCollider2D>().IsTouching(mostRecentDoor.GetComponent<Collider2D>()))
        {
            mostRecentDoor.GetComponent<SpecialObjectTrigger>().humanDoorInteraction();
        }
    }

    //Ran into physical object. Only for Roaming
    private void obstacleEncountered(GameObject collision)
    {
        if (objectType == "human")
        {
            if (currentState == "walking")
            {
                //Initiate stopping then turning
                currentState = "stopped";
                StartCoroutine(stopThenTurn());
            }
            else if (currentState == "alerted" && collision.layer == 11) //encountered a door, but is running to an alarm
            {
                collision.GetComponent<SpecialObjectTrigger>().humanDoorInteraction();
            }
            
        }
    }

    //Ran into an object that this has a special interaction with. Only for roaming
    private void objectEncountered(GameObject collision)
    {
        //Humans only interact if they are currently walking
        if (objectType == "human" && (currentState == "walking" || currentState == "stopped"))
        {
            if (collision.layer == 8 && !socialAnxiety) //human layer
            {
                if (currentState == "stopped")
                {
                    StopAllCoroutines();
                }
                    //StopCoroutine(stopThenTurn());
                beginTalking(collision);
            }
            else if (collision.layer == 11)
            {
                mostRecentDoor = collision;
            }
        }
    }

    public void beginTalking(GameObject targetHuman)
    {
        StartCoroutine(stopAndTalk(targetHuman));
    }

    //Stops to talk
    IEnumerator stopAndTalk(GameObject targetHuman)
    {
        float timer = 7;
        bool flip = false;
        GameObject otherHuman = targetHuman;
        if (otherHuman.GetComponent<AIEntity>().currentState != "waitingToMove")
            otherHuman.GetComponent<AIEntity>().currentState = "waitingToTalk";
        currentState = "talking";
        GameObject speechBubble = Instantiate(soundObjectPrefab);
        if (facing == "right")
            speechBubble.transform.position = new Vector3(transform.position.x + 1.5f, transform.position.y + 1.5f, transform.position.z);
        else
        {
            speechBubble.transform.position = new Vector3(transform.position.x - 1.5f, transform.position.y + 1.5f, transform.position.z);
            flip = true;
        }
        speechBubble.GetComponent<PossessionObject>().setupSoundObject("speechBubble", 7, Vector3.zero, flip);
        speechBubble.GetComponent<PossessionObject>().setupSpeechBubbleText("human");

        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        //Checks if the other human already talked, otherwise makes them talk
        currentState = "waitingToMove";
        if (otherHuman.GetComponent<AIEntity>().currentState != "waitingToMove")
        {
            otherHuman.GetComponent<AIEntity>().beginTalking(gameObject);
        }
        //both humans are done talking, they can resume moving
        else
        {
            otherHuman.GetComponent<AIEntity>().currentState = "walking";
            otherHuman.GetComponent<AIEntity>().talkingCooldown();
            currentState = "walking";
            talkingCooldown();
        }
    }

    public void talkingCooldown()
    {
        socialAnxiety = true;
        StartCoroutine(talkCooldownTimer());
    }

    IEnumerator talkCooldownTimer()
    {
        float timer = 5;
        while (timer > 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        socialAnxiety = false;
    }

    //Turns around
    IEnumerator stopThenTurn()
    {
        float timer = obstacleWait;
        while (timer >= 0)
        {
            timer -= Time.deltaTime;
            yield return null;
        }
        transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
        if (facing == "right")
            facing = "left";
        else
            facing = "right";
        currentState = "walking";
    }

    public void moveToNextWaypoint()
    {
        if (behaviorType == "waypoints" && currentWaypoint < (waypoints.Count - 1))
        {
            currentWaypoint += 1;
            schmooving = true;
            StartCoroutine(moveToWaypoint());
        }
    }

    IEnumerator moveToWaypoint()
    {
        while (Vector3.Distance(transform.position, waypoints[currentWaypoint]) > 0.1f)
        {
            transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypoint], movementSpeed);
            yield return null;
        }
        schmooving = false;
    }

    private void resetToStart()
    {
        StopAllCoroutines();
        socialAnxiety = false;
        currentFloor = startFloor;
        if (facing != startFacing)
        {
            transform.localScale = new Vector3(-transform.localScale.x, transform.localScale.y, transform.localScale.z);
            facing = startFacing;
        }
        //facing = startFacing;
        currentState = startState;
        transform.position = startPos;
        radioTimer = radioTimerLength;
        singingLyrics = false;
        fallingTimer = 1;
        obstacleWait = 1f;
        alarmActive = false;
    }
}
