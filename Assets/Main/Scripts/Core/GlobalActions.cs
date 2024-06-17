using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Single access point for global actions so there can be subscriptions and Invokes.
/// </summary>
public static class GlobalActions
{
    /// <summary>
    /// Fires when starting a new game loop is confirmed,
    /// int value is number of human players, (1 is for player vs AI), (2 is player vs player)
    /// </summary>
    public static Action<int> OnStartGameLoop;

    /// <summary>
    /// Attempts to go to a GameState
    /// </summary>
    public static Action<GameState> TryGoToGameState;
    
    /// <summary>
    /// Communicates the from GameState and new GameState
    /// </summary>
    public static Action<GameState, GameState> OnGameStateChanged;

    /// <summary>
    /// Communicates starting player and if the PlayerB is an AI
    /// </summary>
    public static Action<PlayerTurn, bool> StartGameLoop;
    
    /// <summary>
    /// Informs that there is a new player turn change, bool is for isSinglePlayer
    /// </summary>
    public static Action<PlayerTurn, bool> OnPlayerTurnChanged;
    
    /// <summary>
    /// Communicates which cell data has been selected
    /// note (Used for decoupling the logic from the view.)
    /// </summary>
    public static Action<CellData> CellDataSelected;
    
    /// <summary>
    /// Communicates the completion of the mark being drawn on the cell.
    /// This then informs the logic, to process winner checks and proceed with the other player's turn
    /// </summary>
    public static Action<CellData> MarkDrawCompletedOnCell;

    /// <summary>
    /// Player won event, if bool is true, it was the AI as PlayerB only
    /// </summary>
    public static Action<PlayerTurn, bool> PlayerWon;
    /// <summary>
    /// Both players ended up in a draw.
    /// </summary>
    public static Action GameEndedInDraw;

    /// <summary>
    /// Play a cell coords for the AI
    /// </summary>
    public static Action<Vector2> PlayAICell;

    /// <summary>
    /// Communicates the mark toggle for using alternative meshes.
    /// </summary>
    public static Action<PlayerData, bool> UsingAlternativeMarkToggled;

    /// <summary>
    /// Contains the matching sequence with the Coords of the winning cells.
    /// </summary>
    public static Action<List<Vector2>> WinningSequence;
}

