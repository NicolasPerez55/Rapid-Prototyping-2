using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessionObject : MonoBehaviour
{
    //A PossessionObject can be possessed by the player, moving their 'home' to it.

    public string containedText = "";

    [SerializeField] private GameManager manager;
    private playerController player;

    //[SerializeField] private StatusIndicatorScript indicatorPrefab; //The prefab, only used for instancing. CURRENTLY UNUSED, decided to go for the brute force method and just add the status child manually
    public StatusIndicatorScript indicator;
    public bool globallyRevealed = false;

    public Sprite defaultSprite;
    public Sprite defaultScreenSprite;
    public Sprite possessedSprite;
    public Sprite selectedSprite;
    public Sprite emptySprite;
    public SpriteRenderer renderer;

    public string specialCondition = "";

    public float maxHealth = 5; //While possessed, 1 hp is substracted every second
    public float currentHealth = 5; //How much health it currently has. Does not heal within a level

    public bool isTemporary = false; //If true, this object's lifetime counts down constantly, and it vanishes when it ends. Is not affected by health. "Sound" objects are temporary
    public float lifetime = 1;

    public bool canPossess = true;
    public bool virgin = true; //If true, this object has not been possessed before
    public bool possessed = false;
    public bool selected = false;
    public bool specialInteraction = false; //'static' means no interaction. 'objectTrigger' means it triggers a special effect on another object. 'mute' means it mutes the background music while possessed
    //public PossessionTrigger objectTrigger; //This object's trigger
    public Vector3 direction = Vector3.zero;
    public bool isEndFlag = false;

    void Start()
    {
        renderer = GetComponent<SpriteRenderer>();
        manager = FindAnyObjectByType<GameManager>();
        player = manager.player;
        player.newPossession.AddListener(becomePossessed);
        manager.revealAllStatuses.AddListener(globalStatusReveal);
        manager.hideAllStatuses.AddListener(globalStatusHide);
        manager.resetPosition.AddListener(resetToStart);

        currentHealth = maxHealth;

        if (isTemporary)
        {
            indicator.setupStatus(lifetime, specialCondition, manager.clockIcon, true);
        }
        else
            indicator.setupStatus(currentHealth, specialCondition, manager.heartIcon, false);
    }

    
    void Update()
    {
        if (possessed)
        {
            renderer.sprite = possessedSprite;
            if (!isTemporary)
            {
                //REMOVED THIS, NEW HEALTH SYSTEM
                /*
                currentHealth -= Time.deltaTime;
                if (currentHealth <= 0)
                {
                    currentHealth = 0;
                    manager.gameOver();
                }*/
            }
        }
        else if (!selected)
        {
            if (currentHealth >= 1)
                renderer.sprite = defaultSprite;
            else
                renderer.sprite = emptySprite;
        }

        //Tick down lifespan for temp objects
        if (isTemporary)
        {
            lifetime -= Time.deltaTime;
            indicator.updateStatus(lifetime);
            if (lifetime <= 0)
            {
                lifetime = 0;
                if (possessed)
                {
                    //manager.gameOver();
                }
                else
                {
                    //PLACEHOLDER, add some animation or effect if I wanna be fancy
                    Destroy(gameObject);
                }
            }
        }
        else
        {
            indicator.updateStatus(currentHealth);
        }
    }

    public void becomePossessed()
    {
        if (possessed && canPossess)
        {
            manager.updateTheNote(containedText);
            //This being a virgin is changed in the playerController to avoid conflicts
            renderer.sprite = possessedSprite;
            selected = false;
            indicator.revealStatus();
            if (!isTemporary)
            {
                currentHealth -= 1;
            }
            //Activate all "on possess triggers"
            if (specialInteraction)
            {
                foreach (PossessionTrigger x in GetComponents<PossessionTrigger>())
                {
                    x.activateTrigger(true);
                }
            }
            if (isEndFlag)
            {
                Debug.Log("Starting new level!");
                manager.startNextLevel();
            }
        }
        else
        {
            if (currentHealth >= 1)
                renderer.sprite = defaultSprite;
            else
                renderer.sprite = emptySprite;
        }
    }

    public void depossess()
    {
        possessed = false;
        if (!selected && !globallyRevealed && !isTemporary)
        {
            indicator.hideStatus();
        }
        //Object ran out of health, can no longer be possessed. Adjust sprite accordingly
        if (currentHealth <= 0 && !isTemporary)
        {
            renderer.sprite = emptySprite;
            indicator.hideStatus();
        }
        //Activate all "on possess triggers"
        if (specialInteraction)
        {
            foreach (PossessionTrigger x in GetComponents<PossessionTrigger>())
            {
                x.activateTrigger(false);
            }
        }
    }


    //NOTE FOR FUTURE: add particles and wiggle for the Radio, add a "darkening and fading while going up" animation for when a sound expires
    public void setupSoundObject(string type, float duration, Vector3 direction, bool flip)
    {
        //Depending on type, gets a different set of sprites from the manager
        lifetime = duration;
        switch (type)
        {
            case "speechBubble":
                break;
            case "radioMusic":
                break;
            case "alarm":
                break;
            default:
                break;
        }
        if (flip)
        {
            renderer.flipX = true;
        }
    }

    public void selection(bool selected)
    {
        if (selected && currentHealth >= 1)
        {
            renderer.sprite = selectedSprite;
            manager.bipSound.Play();
            this.selected = true;
            indicator.revealStatus();
        }
        else if (currentHealth >= 1)
        {
            renderer.sprite = defaultSprite;
            this.selected = false;
            if (!isTemporary)
                indicator.hideStatus();
        }
        else
        {
            renderer.sprite = emptySprite;
            this.selected = false;
            if (!isTemporary)
                indicator.hideStatus();
        }
    }

    private void resetToStart()
    {
        if (!isTemporary)
        {
            currentHealth = maxHealth;
            renderer.sprite = defaultSprite;
        }
        else
        {
            lifetime = 0;
        }
        virgin = true;
        if (GetComponent<SpecialObjectTrigger>() != null && GetComponent<SpecialObjectTrigger>().type == "screen" && GetComponent<SpecialObjectTrigger>().effectTriggered == true)
        {
            canPossess = false;
            defaultSprite = defaultScreenSprite;
            GetComponent<SpecialObjectTrigger>().effectTriggered = false;
        }
    }

    private void globalStatusReveal()
    {
        if (!possessed && !selected && canPossess)
        {
            globallyRevealed = true;
            indicator.revealStatus();
        }
            
    }

    private void globalStatusHide()
    {
        if (!possessed && !selected && !isTemporary && canPossess)
        {
            globallyRevealed = false;
            indicator.hideStatus();
        }
            
    }

    public void setupSpeechBubbleText(string source)
    {
        string output = "";
        int randomNumber = Random.Range(0, 4);
        switch (source)
        {
            case "human":
                switch (randomNumber)
                {
                    case 0:
                        output = "*You wanna do something after this? We could go over to my place, just got Die Hard on dvd*";
                        break;
                    case 1:
                        output = "*I'm so focused on patrolling, sometimes I don't even notice holes on the floor. It's crazy*";
                        break;
                    case 2:
                        output = "*Huh, I feel something weird. Like there's someone listening in on us*";
                        break;
                    case 3:
                        output = "*I think we need to cut down on our Notice Board budget, but you didn't hear that from me*";
                        break;
                    default:
                        output = "";
                        break;
                }
                break;
            case "alarm":
                output = "*ALARM NOISES*";
                break;
            case "radio":
                switch (randomNumber)
                {
                    case 0:
                        output = "...And in other news, the presidential elections are in, with a landsldie victory for...";
                        break;
                    case 1:
                        output = "...And now the weather. We will be experiencing large storms up North by the region of...";
                        break;
                    case 2:
                        output = "...And he scores! That is 2-1 on the score marker, in favour of...";
                        break;
                    case 3:
                        output = "...Now coming up, another classic Rock n' Roll ditty, from the king of Rock himself...";
                        break;
                    default:
                        output = "";
                        break;
                }
                break;
            case "fakeRadio":
                switch (randomNumber)
                {
                    case 0:
                        output = "*Another day guarding this specimen cell... Not like the thing's ever gonna get out*";
                        break;
                    case 1:
                        output = "*Do you think it might be a containtment risk to have those notice boards so close to the cell?*";
                        break;
                    case 2:
                        output = "*Huh, I feel something weird. Like there's someone listening in on us*";
                        break;
                    case 3:
                        output = "*It's really quiet here huh? This really is the most boring post in the facility*";
                        break;
                    default:
                        output = "";
                        break;
                }
                break;
            default:
                break;
        }
        containedText = output;
    }
}
