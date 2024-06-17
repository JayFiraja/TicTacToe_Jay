using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This SubManager class is pivotal when handling GameStates.
/// - Note for this demo simplicity this will also take care of
/// - Propagate if a game state changed.
/// - Manage win conditions
/// </summary>
public class GameStateManager : SubManager
{
    // mini analytics keys example
    private const string TOTAL_TURN_COUNT = "TOTAL_PLAY_COUNTS";
    private const string TOTAL_WIN_PLAYER_COUNT = "TOTAL_WINS_PLAYER_COUNT";
    private const string TOTAL_WINS_AI_COUNT = "TOTAL_WINS_COUNT";
    
    [Header("Parameters")]
    [SerializeField]
    private float timeToCallNextPlayerTurn;
    [SerializeField]
    private float timeToMoveFromResultsToMainMenu;
    [SerializeField]
    private AudioClip[] winVoiceSfx;
    [SerializeField]
    private AudioClip drawSfx;
    
    private int[,] _gameGrid;
    private Coroutine _checkForWinnerCo;
    private WaitForSeconds _waitSecondsForWinnerCheck;
    private float _timeSinceResultsScreen;
        
    /// <summary>
    /// Getter for current game state
    /// </summary>
    public GameState CurrentState => _currentState;
    private GameState _currentState = GameState.UnInitialized;

    /// <summary>
    /// Getter for current Player Turn
    /// </summary>
    public PlayerTurn PlayerTurn => _playerTurn;
    public PlayerTurn _playerTurn = PlayerTurn.None;

    /// <summary>
    /// True if AI is playing as PlayerB (by definition)
    /// </summary>
    public bool IsAIPlaying =>_isAIPlaying;
    private bool _isAIPlaying = false;

    private bool hasAIPlayedMarker;
    
    /// <summary>
    /// Control for disallowing to play marks until 1 has been fully registered
    /// </summary>
    public bool CanPlayCell => _canPlayCell;
    private bool _canPlayCell = false;
    
    #region SubManager
    /// <inheritdoc/>
    public override void SubManagerInitialize(GameManager gameManager)
    {
        base.SubManagerInitialize(gameManager);
        GlobalActions.TryGoToGameState += TryGoToGameState;
        GlobalActions.CellDataSelected += CellDataSelected;
        GlobalActions.MarkDrawCompletedOnCell += OnMarkDrawCompletedOnCell;
        GlobalActions.StartGameLoop += StartGameLoop;
        
        _waitSecondsForWinnerCheck = new WaitForSeconds(timeToCallNextPlayerTurn);
    }

    /// <inheritdoc/>
    public override void UpdateManager()
    {
        if (_currentState == GameState.Results)
        {
            bool transitionToMainMenu = Time.timeSinceLevelLoad > (_timeSinceResultsScreen + timeToMoveFromResultsToMainMenu);
            if (transitionToMainMenu)
            {
                TryGoToGameState(GameState.MainMenu);
            }
        }

        if (_currentState == GameState.GameLoop)
        {
            if (_isAIPlaying && _playerTurn == PlayerTurn.PlayerB)
            {
                if (!hasAIPlayedMarker && GameLogic.TryGetRandomEmptyCell(_gameGrid, out Vector2 emptyCell))
                {
                    GlobalActions.PlayAICell.Invoke(emptyCell);
                    hasAIPlayedMarker = true;
                }
            }
        }
    }

    public override void UnSubscribe()
    {
        base.UnSubscribe();
        GlobalActions.TryGoToGameState -= TryGoToGameState;
        GlobalActions.CellDataSelected -= CellDataSelected;
        GlobalActions.MarkDrawCompletedOnCell -= OnMarkDrawCompletedOnCell;
        GlobalActions.StartGameLoop -= StartGameLoop;
    }
    
    #endregion SubManager

    private void CellDataSelected(CellData cellData)
    {
        _canPlayCell = false;
    }

    private void TryGoToGameState(GameState newGameState)
    {
        if (_currentState == newGameState)
        {
            return;
        }
        
        GameState lastState = _currentState;
        _currentState = newGameState;
        GlobalActions.OnGameStateChanged?.Invoke(lastState, _currentState);
    }

    /// <summary>
    /// Single point where the game loop can start from.
    /// </summary>
    private void StartGameLoop(PlayerTurn startingPlayer, bool isAIPlaying)
    {
        if (_currentState == GameState.GameLoop)
        {
            return;
        }
        
        TryGoToGameState(GameState.GameLoop);

        // update our local variables.
        _gameGrid = GameLogic.CreateNewGrid();
        _playerTurn = startingPlayer;
        _isAIPlaying = isAIPlaying;
        _canPlayCell = true;
    }

    private void OnMarkDrawCompletedOnCell(CellData cellData)
    {
        if (_checkForWinnerCo != null)
        {
            StopCoroutine(_checkForWinnerCo);
        }

        _checkForWinnerCo = StartCoroutine(CheckForWinnerCo(cellData));
    }

    /// <summary>
    /// Tries to register a new marker on the given cellData,
    /// updating the unique grid <see cref="_gameGrid"/>
    /// </summary>
    /// <param name="grid"></param>
    /// <param name="markerRegistered"> mark is registered depending on current player turn</param>
    /// <returns>True if operation is successful, false if a marker was already registered</returns>
    private bool TryRegisterNewMarker(CellData cellData, out int markerRegistered)
    {
        markerRegistered = 0;
        // register new Move
        int x = (int)cellData.Coords.x;
        int y = (int)cellData.Coords.y;
        
        if (_gameGrid[x, y] != 0)
        {
            // Already registered
            return false;
        }
        
        switch (_playerTurn)
        {
            case PlayerTurn.PlayerA:
                markerRegistered = 1;
                break;
            case PlayerTurn.PlayerB:
                markerRegistered = 2;
                break;
        }
        
        _gameGrid[x, y] = markerRegistered;
        return true;
    }

    private void EvaluateWinner(int marker)
    {
        // Evaluate winner
        if (GameLogic.CheckWinner(_gameGrid, target: marker, out List<Vector2> matchingCells))
        {
            // Inform the winning sequence
            GlobalActions.WinningSequence?.Invoke(matchingCells);
                
            // Save which player won, this may unlock an alternative mesh for that player
            // using player prefs to save for simplicity reasons in this prototype
            PlayerPrefs.SetInt(_playerTurn.ToString(), 1);
            
            // Fire Game won by current player
            GlobalActions.PlayerWon?.Invoke(_playerTurn, _isAIPlaying);
            _timeSinceResultsScreen = Time.timeSinceLevelLoad;
            // Go to results state
            TryGoToGameState(GameState.Results);
            
            GameManager.AudioManager.PlayVoiceSFX(winVoiceSfx);
 
            // little demo of storing stats...
            if (!_isAIPlaying || _playerTurn == PlayerTurn.PlayerA)
            {
                int totalPlayerWinCounts = PlayerPrefs.GetInt(TOTAL_WIN_PLAYER_COUNT);
                totalPlayerWinCounts++;
                PlayerPrefs.SetInt(TOTAL_WIN_PLAYER_COUNT, totalPlayerWinCounts);
            }
            else if(_isAIPlaying)
            {
                int totalAIWinCounts = PlayerPrefs.GetInt(TOTAL_WINS_AI_COUNT);
                totalAIWinCounts++;
                PlayerPrefs.SetInt(TOTAL_WINS_AI_COUNT, totalAIWinCounts);
            }
        }
        else
        {
            // check if nobody won and there are no free cells
            if (!GameLogic.TryGetRandomEmptyCell(_gameGrid, out Vector2 emptyCell))
            {
                _canPlayCell = false;
                GameManager.AudioManager.PlayUISFX(drawSfx);
                _timeSinceResultsScreen = Time.timeSinceLevelLoad;
                GlobalActions.GameEndedInDraw?.Invoke();
                TryGoToGameState(GameState.Results);
                return;
            }
            
            // Call next player turn
            PlayerTurn nextTurn = _playerTurn == PlayerTurn.PlayerA ? PlayerTurn.PlayerB : PlayerTurn.PlayerA;
            _playerTurn = nextTurn;
            if (_isAIPlaying && _playerTurn == PlayerTurn.PlayerB)
            {
                // only reset this flag here
                hasAIPlayedMarker = false;
            }
            
            _canPlayCell = true;
            GlobalActions.OnPlayerTurnChanged?.Invoke(_playerTurn, _isAIPlaying);

            // little demo of storing stats...
            int totalTurnCounts = PlayerPrefs.GetInt(TOTAL_TURN_COUNT);
            totalTurnCounts++;
            PlayerPrefs.SetInt(TOTAL_TURN_COUNT, totalTurnCounts);
        }
    }
    
    private IEnumerator CheckForWinnerCo(CellData cellData)
    {
        yield return _waitSecondsForWinnerCheck;

        if (TryRegisterNewMarker(cellData, out int markerRegistered))
        {
            EvaluateWinner(markerRegistered);
        }
        
        _checkForWinnerCo = null;
    }
}
