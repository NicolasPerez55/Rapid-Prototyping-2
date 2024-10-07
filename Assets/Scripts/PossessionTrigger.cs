using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossessionTrigger : MonoBehaviour
{
    //This Script is used for PossessionObjects that trigger a unique effect when possessed. 
    public SpecialObjectTrigger linkedObject;
    public SpecialObjectTrigger linkedObjectTwo;

    public void activateTrigger(bool justEntered)
    {
        linkedObject.specialTrigger(justEntered);
        if (linkedObjectTwo != null)
        {
            linkedObjectTwo.specialTrigger(justEntered);
        }
    }


}
