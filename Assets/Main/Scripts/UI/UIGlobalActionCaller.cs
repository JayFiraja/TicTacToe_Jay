using UnityEngine;

/// <summary>
/// Helper UI class for Invoking button actions, through Unity Events
/// </summary>
public class UIGlobalActionCaller : MonoBehaviour
{
    /// <summary>
    /// Starts the game as single or multiplayer
    /// </summary>
    /// <param name="singlePlayer">True for player vs AI, false for Player vs Player</param>
    public void StartGameLoop(bool singlePlayer)
    {
        GlobalActions.StartGameLoop?.Invoke(PlayerTurn.PlayerA, singlePlayer);
    }
    
    /// <summary>
    /// Tries to go to MainMenu
    /// </summary>
    public void TryGoToMainMenu()
    {
        GlobalActions.TryGoToGameState?.Invoke(GameState.MainMenu);
    }
}
