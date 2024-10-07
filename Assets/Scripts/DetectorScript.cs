using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class DetectorScript : MonoBehaviour
{
    //Used by detectors attached to 'roaming' objects. Can either detect objects in 1 (SUBJECT TO CHANGE) layer/s or a physical obstacle, depending on the type
    [SerializeField] private AIEntity owner;
    public string detectorType = "none";
    private Vector3 placement;

    public UnityEvent<GameObject> collided;
    public int layerTarget;

    private void Start()
    {
        owner = GetComponentInParent<AIEntity>();
        placement = transform.localPosition;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.layer == layerTarget || (collision.gameObject.layer == 11))
        {
            collided.Invoke(collision.gameObject);
        }
        //transform.localPosition = placement;
    }

}
