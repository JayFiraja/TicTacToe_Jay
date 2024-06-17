using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class UI_DisplayUnlockables : MonoBehaviour
{
    private CanvasGroup _canvasGroup;

    [SerializeField]
    private GameObject playerARef;
    [SerializeField]
    private GameObject playerBRef;
    
    private void Start()
    {
        TryGetComponent(out _canvasGroup);
        ToggleCanvasGroup(false);
        GlobalActions.OnGameStateChanged += OnGameStateChanged;
    }

    private void OnDestroy()
    {
        GlobalActions.OnGameStateChanged -= OnGameStateChanged;
    }

    private void OnGameStateChanged(GameState fromState, GameState toState)
    {
        if (toState != GameState.MainMenu)
        {
            return;
        }
        int playerAWon = PlayerPrefs.GetInt(PlayerTurn.PlayerA.ToString());
        int playerB = PlayerPrefs.GetInt(PlayerTurn.PlayerB.ToString());

        bool hasPlayerAWins = playerAWon > 0;
        bool hasPlayerBWins = playerAWon > 0;

        if (hasPlayerAWins || hasPlayerBWins)
        {
            ToggleCanvasGroup(true);
        }
        playerARef.SetActive(hasPlayerAWins);
        playerBRef.SetActive(hasPlayerBWins);
    }

    private void ToggleCanvasGroup(bool bValue)
    {
        _canvasGroup.alpha = bValue ? 1 : 0;
        _canvasGroup.interactable = bValue ? true : false;
        _canvasGroup.blocksRaycasts = bValue ? true : false;
    }
    
}
