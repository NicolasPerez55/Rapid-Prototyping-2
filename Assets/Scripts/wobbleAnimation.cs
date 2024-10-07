using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class wobbleAnimation : MonoBehaviour
{
    public bool enabled = true; //Does not actually work currentlym but that is barely relevant so leaving it be
    [Range(0.1f, 5f)]
    public float betweenTime = 0.5f;

    [Range(5f, 50f)]
    public float intensity = 10f;

    private Quaternion targetAngle;

    void Start()
    {
        InvokeRepeating("ChangeTarget", 0, betweenTime);
    }

    void Update()
    {
        transform.rotation = Quaternion.Lerp(transform.rotation, targetAngle, Time.deltaTime);
    }

    void ChangeTarget()
    {
        //float randomIntensity = Random.Range(5f, intensity);
        var sinCurve = Mathf.Sin(Time.time);//(Random.Range(0, Mathf.PI*2));
        targetAngle = Quaternion.Euler(Vector3.forward * sinCurve * intensity);
    }
}
