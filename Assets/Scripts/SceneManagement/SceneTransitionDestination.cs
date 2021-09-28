﻿using UnityEngine;
using UnityEngine.Events;

namespace SceneManagement
{
    public class SceneTransitionDestination : MonoBehaviour
    {
        public enum DestinationTag
        {
            A, B, C, D, E, F, G,
        }


        public DestinationTag destinationTag;    // This matches the tag chosen on the TransitionPoint that this is the destination for.
        [Tooltip("This is the game object that has transitioned.  For example, the player.")]
        public UnityEvent onReachDestination;
    }
}