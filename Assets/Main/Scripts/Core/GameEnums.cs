/// <summary>
/// Used for keeping track of the current player turn.
/// </summary>
public enum PlayerTurn
{
    None,
    /// <summary>
    /// Starting player by definition
    /// </summary>
    PlayerA,
    /// <summary>
    /// AI controlled, when playing Player vsAI
    /// </summary>
    PlayerB
}

/// <summary>
/// Used for keeping track of the current game state.
/// </summary>
public enum GameState
{
    /// <summary>
    /// Default state
    /// </summary>
    UnInitialized,
    /// <summary>
    /// Initial default value
    /// </summary>
    Loading,
    /// <summary>
    /// Game type selection, first landing interactive state
    /// </summary>
    MainMenu,
    /// <summary>
    /// Main game state where the game happens.
    /// </summary>
    GameLoop,
    /// <summary>
    /// End game display state
    /// </summary>
    Results
}

