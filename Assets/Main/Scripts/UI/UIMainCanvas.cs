using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Simple class that listens the game events and transitions the visibility of the canvasGroups
/// </summary>
public class UIMainCanvas : MonoBehaviour
{
    [Header("Set Children CanvasGroups"), SerializeField]
    private List<StateCanvasGroup> StateCanvasGroups = new List<StateCanvasGroup>();
    [SerializeField]
    private float fadeSpeed = 3f;
    private Coroutine _fadeCoroutine;
    
    private Queue<StateCanvasGroup> _canvasesToTransition = new Queue<StateCanvasGroup>();

    #region Monobehaviour
    private void Awake()
    {
        GlobalActions.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GlobalActions.OnGameStateChanged -= OnGameStateChanged;
    }

    private void Start()
    {
        GameState startingState = GameState.UnInitialized;
        if (GameManager.S != null && GameManager.S.GameStateManager != null)
        {
            // Initialize with all Canvas set to invisible
            SetAllCanvasAlpha(0);
            return;
        }
        
        startingState = GameManager.S.GameStateManager.CurrentState;
        
        // call this in case this instance was spawned after Loading.
        OnGameStateChanged(GameState.UnInitialized, startingState);
    }
    #endregion

    private void OnGameStateChanged(GameState fromState, GameState toState)
    {
        _canvasesToTransition.Clear();
        StateCanvasGroup nextVisibleCanvas = new StateCanvasGroup();
        bool nextStateFound = false;
        
        for (int i = 0; i < StateCanvasGroups.Count; i++)
        {
            StateCanvasGroup stateCanvasGroup = StateCanvasGroups[i];
            bool otherStateAndVisible = stateCanvasGroup.GameState != toState && stateCanvasGroup.GetCanvasAlpha() > 0;
            bool sameAsToState = stateCanvasGroup.GameState == toState;
                
            if (otherStateAndVisible)
            {
                stateCanvasGroup.transitionAlphaTarget = 0;
                _canvasesToTransition.Enqueue(stateCanvasGroup);
            }

            if (!nextStateFound && sameAsToState)
            {
                nextVisibleCanvas = stateCanvasGroup;
                nextVisibleCanvas.transitionAlphaTarget = 1;
                nextStateFound = true;
            }
        }

        if (nextStateFound)
        {
            // by using a queue we ensure the target visible canvas group is the last one Enqueued
            // and therefor the last one to be Dequeued, to ensure nice transitions
            _canvasesToTransition.Enqueue(nextVisibleCanvas);
        }

        if (_canvasesToTransition.Count > 0)
        {
            if (_fadeCoroutine != null)
            {
                StopCoroutine(_fadeCoroutine);
            }
            _fadeCoroutine = StartCoroutine(TransitionPendingCanvases());
        }
    }

    private IEnumerator TransitionPendingCanvases()
    {
        while (_canvasesToTransition.Count > 0)
        {
            StateCanvasGroup stateCanvasGroup = _canvasesToTransition.Dequeue();
            float targetAlpha = stateCanvasGroup.transitionAlphaTarget;

            while (!Mathf.Approximately(stateCanvasGroup.GetCanvasAlpha(), targetAlpha))
            {
                float newAlpha = Mathf.MoveTowards(stateCanvasGroup.GetCanvasAlpha(), targetAlpha, fadeSpeed * Time.deltaTime);
                stateCanvasGroup.SetCanvasAlpha(newAlpha);
                yield return null;
            }
            stateCanvasGroup.SetCanvasAlpha(targetAlpha);
            stateCanvasGroup.UpdateCanvasInteractivity();
        }
        _fadeCoroutine = null;
    }
    
    private void SetAllCanvasAlpha(float alpha)
    {
        foreach (StateCanvasGroup stateCanvasGroup in StateCanvasGroups)
        {
            stateCanvasGroup.SetCanvasAlpha(alpha);
            stateCanvasGroup.UpdateCanvasInteractivity();
        }
    }
    
}

[System.Serializable]
public struct StateCanvasGroup
{
    public CanvasGroup CanvasGroup;
    public GameState GameState;
    /// <summary>
    /// This is set when added on a queue for processing transitions
    /// </summary>
    public float transitionAlphaTarget;

    public bool IsCanvasGroupVisible()
    {
        return CanvasGroup.alpha > 0;
    }
    public float GetCanvasAlpha()
    {
        return CanvasGroup.alpha;
    }
    public void SetCanvasAlpha(float newAlpha)
    {
        CanvasGroup.alpha = newAlpha;
    }

    /// <summary>
    /// We ensure that invisible or almost invisible canvases never block Raycasting and
    /// are not marked as interactive
    /// </summary>
    public void UpdateCanvasInteractivity()
    {
        bool allowsInteractivity = CanvasGroup.alpha > 0.1f;
        CanvasGroup.blocksRaycasts = allowsInteractivity;
        CanvasGroup.interactable = allowsInteractivity;
    }
}
