using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class playerController : MonoBehaviour
{
    private SpriteRenderer renderer;
    private Rigidbody2D rb;
    private GameManager manager;

    public GameObject home;
    public GameObject cameraPoint; //Disabled moving this to the home, gonna try out having the camera remain locked so the level is smaller and more navigatable
    public PossessionObject currentTarget;
    public UnityEvent newPossession;
    public UnityEvent leftPossession;

    public bool possessionSettled = false;
    public PossessionObject currentSelection;

    [Space, Header("Movement Modifiers")]
    public float speed = 3;
    [SerializeField] private float horizontalInput;
    [SerializeField] private float verticalInput;

    [SerializeField] private bool moving = false;

    void Start()
    {
        manager = FindFirstObjectByType<GameManager>();
        renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();

        //Find the inital possessed object and jump in
        foreach (PossessionObject x in FindObjectsByType<PossessionObject>(FindObjectsSortMode.None))
        {
            if (x.possessed)
            {
                currentTarget = x;
                transform.SetParent(x.transform, false);
                transform.localPosition = new Vector3(0, 0, transform.position.z);
                home.transform.localPosition = x.transform.position;
                //cameraPoint.transform.position = home.transform.position;

                //leftPossession.Invoke();
                newPossession.Invoke();
                renderer.enabled = false;
                possessionSettled = true;
            }
        }
    }

    
    void Update()
    {
        //Object switching is finished and player is not dying or in another weird state
        if (possessionSettled)
        {
            home.transform.localPosition = currentTarget.transform.position;

            //Left click held means it moves towards the mouse
            if (Input.GetMouseButton(0) && manager.gameRunning)
            {
                moving = true;
            }
            else
            {
                moving = false;
            }

            //TODO: Make the transition smoother, use 'possessionSettled' and maybe coroutines, and some animation / make camera move over
            //Jump into the selected object, can use space or right click
            if ((Input.GetKey(KeyCode.Space) || Input.GetMouseButton(1)) && manager.gameRunning)
            {
                if (currentSelection != null)
                {
                    currentTarget.depossess();
                    currentTarget = currentSelection;
                    currentSelection = null;
                    currentTarget.selected = false;
                    currentTarget.possessed = true;
                    transform.SetParent(currentTarget.transform, false);
                    transform.localPosition = new Vector3(0, 0, transform.position.z);
                    home.transform.localPosition = currentTarget.transform.position;
                    //cameraPoint.transform.position = home.transform.position;

                    //leftPossession.Invoke();
                    newPossession.Invoke();
                    renderer.enabled = false;
                    possessionSettled = true;

                    if (currentTarget.virgin)
                    {
                        manager.newPossession();
                        currentTarget.virgin = false;
                    }
                }
            }
            //If pretty much at object center, render this invisible
            //if (transform.localPosition.x < 0.1 && transform.localPosition.x > -0.1 && transform.localPosition.y < 0.1 && transform.localPosition.y > -0.1)
            if (Vector3.Distance(transform.position, home.transform.position) < 0.1)
            {
                renderer.enabled = false;
            }
            else
            {
                renderer.enabled = true;
            }
            //transform.LookAt(home.transform.position);
        }
        else
        {
            newPossession.Invoke();
            possessionSettled = true;
        }
        
    }

    public void possessNewObject(PossessionObject newObject)
    {
        currentTarget.depossess();
        currentTarget = newObject;
        currentSelection = null;
        currentTarget.selected = false;
        currentTarget.possessed = true;
        transform.SetParent(currentTarget.transform, false);
        transform.localPosition = new Vector3(0, 0, transform.position.z);
        home.transform.localPosition = currentTarget.transform.position;
        //cameraPoint.transform.position = home.transform.position;

        //leftPossession.Invoke();
        newPossession.Invoke();
        renderer.enabled = false;
        possessionSettled = true;
        manager.newPossession();
        if (currentTarget.virgin)
        {
            //manager.newPossession();
            currentTarget.virgin = false;
        }
    }

    private void FixedUpdate()
    {
        Vector3 mousePos = cameraPoint.GetComponentInChildren<Camera>().ScreenToWorldPoint(Input.mousePosition);
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        rb.velocity = new Vector2(horizontalInput * speed, verticalInput * speed);

        if (horizontalInput == 0 && verticalInput == 0 && !moving) //Move towards home
        {
            transform.position = Vector3.MoveTowards(transform.position, home.transform.position, 0.4f);
        }
        else if (moving) //Move towards the mouse
        {
            transform.position = Vector3.MoveTowards(transform.position, mousePos, 0.5f); //0.9f
        }
        
        transform.localPosition = new Vector3(Mathf.Clamp(transform.localPosition.x, -2f, 2f), Mathf.Clamp(transform.localPosition.y, -2f, 2f), 0); //2.5f
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Debug.Log("collision!");
        Debug.Log(collision.gameObject.layer);
        switch (collision.gameObject.layer)
        {
            case 6: //A possessable object
                Debug.Log("layer 6");
                PossessionObject target = collision.GetComponent<PossessionObject>();
                if (!target.possessed && target.currentHealth >= 1 && target.canPossess)
                {
                    currentSelection = target;
                    target.selection(true);
                }
                break;
            default:
                break;
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        Debug.Log("collision exited!");
        switch (collision.gameObject.layer)
        {
            case 6: //A possessable object
                PossessionObject target = collision.GetComponent<PossessionObject>();
                if (!target.possessed && target.selected && target.canPossess)
                {
                    currentSelection = null; //THIS COULD CAUSE ISSUES!!!!!!! (IF MULTIPLE OBJECTS NEAR EACHOTHER)
                    target.selection(false);

                }
                break;
            default:
                break;
        }
    }

}
