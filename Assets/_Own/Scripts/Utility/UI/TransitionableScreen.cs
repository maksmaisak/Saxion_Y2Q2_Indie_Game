using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using DG.Tweening;
using UnityEngine.UI;

#pragma warning disable 0649

[RequireComponent(typeof(CanvasGroup))]
public class TransitionableScreen : MyBehaviour
{
    protected static readonly Stack<TransitionableScreen> previousScreens = new Stack<TransitionableScreen>();
    protected static TransitionableScreen currentlySelected;

    [SerializeField] bool startSelected;
    [SerializeField] bool deactivateOnTransitionOut;

    [SerializeField] UnityEvent onTransitionIn  = new UnityEvent();
    [SerializeField] UnityEvent onTransitionOut = new UnityEvent();

    protected CanvasGroup canvasGroup;

    public bool wasSelectedLastFrame { get; private set; }

    public bool isCurrentlySelected => this == currentlySelected;

    protected virtual void Start()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (startSelected) TransitionIn();
        else OnStartUnselected();
    }

    protected virtual void LateUpdate()
    {
        wasSelectedLastFrame = isCurrentlySelected;
    }

    public void TransitionIn()
    {
        if (isCurrentlySelected) return;

        if (currentlySelected != null)
        {
            Deactivate(currentlySelected);
            previousScreens.Push(currentlySelected);
        }

        currentlySelected = this;
        Activate(this);
    }

    public void TransitionOut()
    {
        if (!isCurrentlySelected) return;
        
        Deactivate(this);
        previousScreens.Push(this);
        currentlySelected = null;
    }

    public void TransitionToPreviousScreen() => TransitionToPrevious();

    public static void TransitionToPrevious()
    {
        if (previousScreens.Count == 0) return;

        if (currentlySelected != null)
        {
            Deactivate(currentlySelected);
        }

        currentlySelected = previousScreens.Pop();
        Activate(currentlySelected);
    }
    
    private static void Activate(TransitionableScreen screen)
    {
        var canvasGroup = screen.canvasGroup ?? screen.GetComponent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        screen.gameObject.SetActive(true);
        screen.OnTransitionIn();
        screen.onTransitionIn.Invoke();
    }

    private static void Deactivate(TransitionableScreen screen)
    {
        var canvasGroup = screen.canvasGroup ?? screen.GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        if (screen.deactivateOnTransitionOut)
        {
            screen.gameObject.SetActive(false);
        }

        screen.OnTransitionOut();
        screen.onTransitionOut.Invoke();
    }

    protected virtual void OnStartUnselected() {}
    protected virtual void OnTransitionIn() {}
    protected virtual void OnTransitionOut() {}

    protected void SelectFirstButton()
    {
        var button = GetComponentInChildren<Button>();
        if (button) button.Select();
    }
}