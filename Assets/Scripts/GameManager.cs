using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    //Handles global events, UI, and some sound effects/sprites

    public bool gameRunning = true;
    public UnityEvent resetPosition;

    public GameObject cameraPoint;
    public playerController player;
    [SerializeField] private TextMeshProUGUI voidCountText;
    [SerializeField] private TextMeshProUGUI endingText;

    public int voidedCount = 1;
    public int totalPossessableObjects = 1;

    [SerializeField] private PossessionObject[] targetObjects;

    public UnityEvent revealAllStatuses;
    public UnityEvent hideAllStatuses;

    public Sprite clockIcon;
    public Sprite heartIcon;
    [SerializeField] private AudioSource wooshSound;
    public AudioSource bipSound;

    public PossessionObject startobjectLevel1;
    public PossessionObject startobjectLevel2;
    public PossessionObject startobjectLevel3;
    public PossessionObject startobjectLevel4;

    public Vector3 mainMenuCameraPos;
    public Vector3 cameraPosLevel1;
    public Vector3 cameraPosLevel2;
    public Vector3 cameraPosLevel3;
    public Vector3 cameraPosLevel4;
    public int currentLevel = 1;

    public GameObject note;
    public TextMeshPro levelName;
    public TextMeshPro noteText;
    public GameObject gameInfoText;
    public Canvas canvas;

    void Start()
    {
        targetObjects = FindObjectsByType<PossessionObject>(FindObjectsSortMode.None);
        totalPossessableObjects = targetObjects.Length;
        voidCountText.text = "VOIDED: " + voidedCount + " / " + totalPossessableObjects;
        cameraPoint.transform.position = cameraPosLevel1;
        backToMainMenu();
    }

    
    void Update()
    {
        //reveal all the statuses
        if ((Input.GetKeyDown(KeyCode.LeftShift) || Input.GetKeyDown(KeyCode.RightShift)) && gameRunning)
        {
            Debug.Log("About to reveal all statuses");
            revealAllStatuses.Invoke();
        }
        
        if ((Input.GetKeyUp(KeyCode.LeftShift) || Input.GetKeyUp(KeyCode.RightShift)) && gameRunning)
        {
            Debug.Log("About to hide all statuses");
            hideAllStatuses.Invoke();
        }

        if (Input.GetKey(KeyCode.R))
        {
            gameOver(false);
        }

        if (Input.GetKey(KeyCode.Escape))
        {
            if (currentLevel == 0)
                Application.Quit();
            else
            {
                backToMainMenu();
            }
        }
    }

    public void backToMainMenu()
    {
        gameRunning = false;
        currentLevel = 0;
        cameraPoint.transform.position = mainMenuCameraPos;
        gameInfoText.SetActive(false);
        noteText.gameObject.SetActive(false);
        levelName.gameObject.SetActive(false);
        note.SetActive(false);
        canvas.gameObject.SetActive(true);
    }

    public void goToLevel1()
    {
        startNextLevel();
    }
    public void goToLevel2()
    {
        currentLevel = 1;
        startNextLevel();
    }
    public void goToLevel3()
    {
        currentLevel = 2;
        startNextLevel();
    }


    public void updateTheNote(string newText)
    {
        switch (newText)
        {
            case "SPECIAL1":
                noteText.text = "LEFT CLICK - Move (Hold)\nRIGHT CLICK - Possess object overlapping you\nSHIFT - See all possessable objects";
                break;
            default:
                noteText.text = newText;
                break;
        }
    }

    public void startNextLevel()
    {
        gameRunning = true;
        gameInfoText.SetActive(true);
        noteText.gameObject.SetActive(true);
        levelName.gameObject.SetActive(true);
        canvas.gameObject.SetActive(false);
        note.SetActive(true);
        switch (currentLevel)
        {
            case 0:
                cameraPoint.transform.position = cameraPosLevel1;
                player.possessNewObject(startobjectLevel1);
                levelName.text = "LEVEL 1: FRIENDLY CHATTER";
                break;
            case 1:
                cameraPoint.transform.position = cameraPosLevel2;
                player.possessNewObject(startobjectLevel2);
                levelName.text = "LEVEL 2: FRIENDLY CHATTER";
                break;
            case 2:
                cameraPoint.transform.position = cameraPosLevel4; //FOR NOW I am just going to skip to level 4 and have no level 3, use that as level 3
                player.possessNewObject(startobjectLevel4);
                currentLevel += 1;
                levelName.text = "LEVEL 3: RAISE THE ALARM";
                break;
            case 3:
                cameraPoint.transform.position = cameraPosLevel4;
                player.possessNewObject(startobjectLevel4);
                break;
            case 4:
                gameOver(true);
                break;
            default:
                break;
        }
        currentLevel += 1;
        gameOver(false);

    }

    public void newPossession()
    {
        //voidedCount += 1;
        //voidCountText.text = "VOIDED: " + voidedCount + " / " + totalPossessableObjects;
        wooshSound.Play();

        if (totalPossessableObjects == voidedCount)
        {
            //endingText.gameObject.SetActive(true);
        }
    }

    public void gameOver(bool victory)
    {
        int levelNow = currentLevel;
        Scene scene = SceneManager.GetActiveScene();
        if (victory)
        {

        }
        else
        {
            //SceneManager.LoadScene(scene.name);
            //gameRunning = false;
            hideAllStatuses.Invoke();
            currentLevel = levelNow;
            resetPosition.Invoke();
            switch (currentLevel)
            {
                case 1:
                    cameraPoint.transform.position = cameraPosLevel1;
                    player.possessNewObject(startobjectLevel1);
                    break;
                case 2:
                    cameraPoint.transform.position = cameraPosLevel2;
                    player.possessNewObject(startobjectLevel2);
                    break;
                case 3:
                    cameraPoint.transform.position = cameraPosLevel3;
                    player.possessNewObject(startobjectLevel3);
                    break;
                case 4:
                    cameraPoint.transform.position = cameraPosLevel4;
                    player.possessNewObject(startobjectLevel4);
                    break;
                default:
                    break;
            }
        }
    }
}
