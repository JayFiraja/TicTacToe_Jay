using System;
using UnityEngine;

/// <summary>
/// This class implements the singleton pattern.
/// 1.- Static accessor
/// 2.- Handles SubManagers
/// 3.- Pivotal point for all systems
/// </summary>
public class GameManager : MonoBehaviour
{
    private static GameManager _S;
    
    // NOTE: Having the following sub-managers readily available in this GameManager
    // serves similar to the ServiceLocator paradigm, where subManagers are pivotal and single serving Global references to any class.
    
    /// <summary>
    /// Gets the Audio Manager
    /// </summary>
    public AudioManager AudioManager => _audiomanager;
    [SerializeField]
    private AudioManager _audiomanager;
    
    /// <summary>
    /// Gets the Game State Manager
    /// </summary>
    public GameStateManager GameStateManager => _gameStateManager;
    [SerializeField]
    private GameStateManager _gameStateManager;

    private SubManager[] _subManagers;

    public PlayerData[] PlayerDatas => _playerDatas;
    [SerializeField]
    private PlayerData[] _playerDatas;

    /// <summary>
    /// Visibility status of debug elements
    /// </summary>
    public bool DebugElementsVisible
    {
        get
        {
            return _debugElementsVisible;
        }
        set
        {
            _debugElementsVisible = value;
        }
    }
    
    // initialized as true
    private bool _debugElementsVisible = true;
    
    /// <summary>
    /// <para>This static private property provides some protection for the Singleton _S.</para>
    /// <para>get {} does return null, but throws an error first.</para>
    /// <para>set {} allows overwrite of _S by a 2nd instance, but throws an error first.</para>
    /// <para>Another advantage of using a property here is that it allows you to place
    /// a breakpoint in the set clause and then look at the call stack if you fear that 
    /// something random is setting your _S value.</para>
    /// </summary>
    static public GameManager S
    {
        get
        {
            if (_S == null)
            {
                Debug.LogError("GameManager:S getter - Attempt to get value of S before it has been set.");
                return null;
            }
            return _S;
        }
        private set
        {
            if (_S != null)
            {
                Debug.LogError("GameManager:S setter - Attempt to set S when it has already been set.");
            }
            _S = value;
        }
    }
    
    private void Awake()
    {
        if (_S != null)
        {
            Debug.Log("GameManager: Only one instance can exist at any time. Destroying potential usurper.", _S.gameObject);
            Destroy(gameObject);
            return;
        }
        else 
        {
            S = this;
        }
    }
    
    private void Start()
    {
        _subManagers = GetComponentsInChildren<SubManager>();

        foreach (SubManager subManager in _subManagers)
        {
            subManager.SubManagerInitialize(this);
        }
        
        // Kick start the game loading
        GlobalActions.TryGoToGameState(GameState.Loading);
        GlobalActions.UsingAlternativeMarkToggled += OnUsingAlternativeMarkToggled;
    }

    private void OnDestroy()
    {
        GlobalActions.UsingAlternativeMarkToggled -= OnUsingAlternativeMarkToggled;
    }

    private void Update()
    {
        // Note: having this single Monobehaviour Update removes the overhead of having to call multiple individual update loops
        foreach (SubManager subManager in _subManagers)
        {
            subManager.UpdateManager();
        }
    }

    public bool TryGetCurrentPlayerTurnData(out PlayerData foundPlayerData)
    {
        foundPlayerData = new PlayerData();
        foundPlayerData.PlayerTurn = PlayerTurn.None;
        
        PlayerTurn playerTurn = _gameStateManager.PlayerTurn;
        return  TryGetPlayerTurnData(playerTurn, out foundPlayerData);
    }
    
    public bool TryGetPlayerTurnData(PlayerTurn playerTurn, out PlayerData foundPlayerData)
    {
        foundPlayerData = new PlayerData();
        foundPlayerData.PlayerTurn = PlayerTurn.None;
        
        foreach (PlayerData playerData in _playerDatas)
        {
            if (playerData.PlayerTurn == playerTurn)
            {
                foundPlayerData = playerData;
                break;
            }
        }
        
        return foundPlayerData.PlayerTurn != PlayerTurn.None;
    }

    private void OnUsingAlternativeMarkToggled(PlayerData playerData, bool useAlternativeMark)
    {
        PlayerTurn playerTurn = playerData.PlayerTurn;

        for (int i = 0; i < _playerDatas.Length; i++)
        {
            if (_playerDatas[i].PlayerTurn == playerTurn)
            {
                _playerDatas[i].usingAlternativeMark = useAlternativeMark;
                break;
            }
        }
    }
}

