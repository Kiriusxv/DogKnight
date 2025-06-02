using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionDestination : MonoBehaviour
{
    public enum DestinationTag
    {
        ENETER, A, B, C, ROOM
    }

    public DestinationTag destinationTag;
}