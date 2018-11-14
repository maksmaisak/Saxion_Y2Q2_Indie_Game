using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

#pragma warning disable 0649

/// Makes it possible to define transitions between screens based on button clicks in the inspector.
[RequireComponent(typeof(TransitionableScreen))]
public class ScreenTransitions : MonoBehaviour
{
    [Serializable]
    struct Transition
    {
        public KeyCode keyPress;
        public TransitionableScreen target;
    }

    [SerializeField] TransitionableScreen targetScreen;
    [SerializeField] Transition[] transitions;

    void Start()
    {
        targetScreen = targetScreen ? targetScreen : GetComponent<TransitionableScreen>();
    }

    void Update()
    {
        if (!targetScreen.isCurrentlySelected) return;
        if (!targetScreen.wasSelectedLastFrame) return;

        foreach (Transition transition in transitions)
        {
            if (Input.GetKeyDown(transition.keyPress))
            {
                transition.target.TransitionIn();
                break;
            }
        }
    }
}