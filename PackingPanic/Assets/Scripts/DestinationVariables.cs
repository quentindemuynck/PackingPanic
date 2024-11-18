using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestinationVariables : MonoBehaviour
{
    private bool _isTaken;

    public void SetIsTaken(bool isTaken)
    {
        _isTaken = isTaken;
    }

    public bool GetIsTaken()
    { 
        return _isTaken; 
    }

}
